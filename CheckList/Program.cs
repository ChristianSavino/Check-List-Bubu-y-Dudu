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

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddRazorPages()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

var app = builder.Build();

// Inicializar DB, cleanup y feriados
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CheckListDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

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

app.MapHub<ChecklistHub>("/checklistHub");
app.MapControllers();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
