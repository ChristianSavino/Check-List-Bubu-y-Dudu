using CheckList.Core.Compra.DataAccess;
using CheckList.Core.Infrastructure;
using CheckList.Core.Tarea.DataAccess;
using CheckList.Core.Tarea.Logic;
using CheckList.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
    options.ListenAnyIP(7226, listenOptions => listenOptions.UseHttps());
});

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7226;
});

// Entity Framework Core + SQLite
builder.Services.AddDbContext<CheckListDbContext>(options =>
    options.UseSqlite("Data Source=checklist.db")
           .LogTo(_ => { }, Microsoft.Extensions.Logging.LogLevel.None)
);

// Repositorios
builder.Services.AddScoped<ITareaRepository, TareaRepository>();
builder.Services.AddScoped<ICompraRepository, CompraRepository>();
builder.Services.AddScoped<IAppSettingRepository, AppSettingRepository>();

// Servicios
builder.Services.AddScoped<ITareaService, TareaService>();
builder.Services.AddScoped<ITareaCleanupService, TareaCleanupService>();

builder.Services.AddSingleton<FeriadoService>();
builder.Services.AddSingleton<IFeriadoService>(sp => sp.GetRequiredService<FeriadoService>());
builder.Services.AddHttpClient<FeriadoService>();

builder.Services.AddHostedService<PortForwardingService>();

// SignalR
builder.Services.AddSignalR();

// Controladores y Razor Pages
builder.Services.AddControllers();
builder.Services.AddRazorPages()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

var app = builder.Build();

// Migrar DB y ejecutar cleanup al arrancar
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CheckListDbContext>();

    // EnsureCreated no corre migraciones, usamos MigrateAsync para aplicar cambios de esquema
    await dbContext.Database.EnsureCreatedAsync();

    // Agregar columna DiaSemana si no existe (para DBs existentes sin migraciones)
    var conn = dbContext.Database.GetDbConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        PRAGMA table_info(Tareas);
    ";
    var columns = new List<string>();
    using (var reader = await cmd.ExecuteReaderAsync())
        while (await reader.ReadAsync())
            columns.Add(reader.GetString(1));

    if (!columns.Contains("DiaSemana"))
    {
        cmd.CommandText = "ALTER TABLE Tareas ADD COLUMN DiaSemana TEXT;";
        await cmd.ExecuteNonQueryAsync();
    }

    if (!columns.Contains("Orden"))
    {
        cmd.CommandText = "ALTER TABLE Tareas ADD COLUMN Orden INTEGER NOT NULL DEFAULT 0;";
        await cmd.ExecuteNonQueryAsync();
    }

    if (!columns.Contains("FechaFin"))
    {
        cmd.CommandText = "ALTER TABLE Tareas ADD COLUMN FechaFin TEXT;";
        await cmd.ExecuteNonQueryAsync();
    }

    cmd.CommandText = @"
    CREATE TABLE IF NOT EXISTS Compras (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Nombre TEXT NOT NULL,
        Tipo TEXT NOT NULL DEFAULT 'Diaria',
        Completada INTEGER NOT NULL DEFAULT 0,
        Orden INTEGER NOT NULL DEFAULT 0,
        FechaCreacion TEXT DEFAULT CURRENT_TIMESTAMP
    );";
    await cmd.ExecuteNonQueryAsync();

    cmd.CommandText = "PRAGMA table_info(Compras);";
    var compraColumns = new List<string>();
    using (var reader2 = await cmd.ExecuteReaderAsync())
        while (await reader2.ReadAsync())
            compraColumns.Add(reader2.GetString(1));

    if (compraColumns.Any() && !compraColumns.Contains("Orden"))
    {
        cmd.CommandText = "ALTER TABLE Compras ADD COLUMN Orden INTEGER NOT NULL DEFAULT 0;";
        await cmd.ExecuteNonQueryAsync();
    }

    await conn.CloseAsync();

    var cleanupService = scope.ServiceProvider.GetRequiredService<ITareaCleanupService>();
    await cleanupService.CleanupAsync();

    var feriadoService = app.Services.GetRequiredService<IFeriadoService>();
    await feriadoService.CargarFeriadosAsync(DateTime.Now.Year);
    await feriadoService.CargarFeriadosAsync(DateTime.Now.Year + 1);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Hub de SignalR
app.MapHub<ChecklistHub>("/checklistHub");

app.MapControllers();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();