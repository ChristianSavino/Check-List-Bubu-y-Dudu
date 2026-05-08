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
);

// Repositorios
builder.Services.AddScoped<ITareaRepository, TareaRepository>();
builder.Services.AddScoped<IAppSettingRepository, AppSettingRepository>();

// Servicios
builder.Services.AddScoped<ITareaService, TareaService>();
builder.Services.AddScoped<ITareaCleanupService, TareaCleanupService>();

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
    await conn.CloseAsync();

    var cleanupService = scope.ServiceProvider.GetRequiredService<ITareaCleanupService>();
    await cleanupService.CleanupAsync();
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