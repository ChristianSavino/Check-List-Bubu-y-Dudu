using CheckList.Core.Tarea.DataAccess;
using CheckList.Core.Tarea.Logic;
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

// Registrar repositorios
builder.Services.AddScoped<ITareaRepository, TareaRepository>();
builder.Services.AddScoped<IAppSettingRepository, AppSettingRepository>();

// Registrar servicios
builder.Services.AddScoped<ITareaService, TareaService>();
builder.Services.AddScoped<ITareaCleanupService, TareaCleanupService>();

// Agregar controladores y Razor Pages
builder.Services.AddControllers();
builder.Services.AddRazorPages()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

var app = builder.Build();

// Crear/migrar base de datos e executar limpeza
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CheckListDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

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

app.MapControllers();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
