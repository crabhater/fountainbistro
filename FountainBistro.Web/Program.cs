using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Linq;
using FountainBistro.Web.Infrastructure.Data;
using FountainBistro.Web.Infrastructure.Extensions;
using FountainBistro.Web.Infrastructure.Data.SeedData;
using FountainBistro.Web.Middleware;
using FountainBistro.Web.Services.Implementations;

// 🔥 Получаем порт из окружения
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

Console.WriteLine($"🚀 Запуск в окружении: {environment}");
Console.WriteLine($"📡 Порт: {port}");
Console.WriteLine($"📁 Текущая директория: {Directory.GetCurrentDirectory()}");

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    EnvironmentName = environment,
    ContentRootPath = Directory.GetCurrentDirectory()
});

// 🔥 ОЧИЩАЕМ ВСЕ КОНФИГУРАЦИИ
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// 🔥 Принудительно устанавливаем URL
builder.WebHost.UseUrls($"http://*:{port}");

// Настройка логирования
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "/app/logs/app-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        );
});

// Добавляем сервисы
builder.Services.AddControllersWithViews();

// 🔥 Путь к БД - используем текущую директорию
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "FountainBistro.db");
Console.WriteLine($"🗄️ Путь к БД: {dbPath}");

// Настройка SQLite
var connectionString = $"Data Source={dbPath};Cache=Shared";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Регистрация сервисов
builder.Services.AddApplicationServices();
builder.Services.AddApplicationRepositories();
builder.Services.AddFluentValidationServices();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

// Cookie
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.SameAsRequest;
});

// Кэш
builder.Services.AddMemoryCache();

// HTTP
builder.Services.AddHttpClient();

// Фоновый сервис
builder.Services.AddHostedService<OrderBackgroundService>();

var app = builder.Build();

// ❌ Убираем UseHttpsRedirection
app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseMiddleware<AuthMiddleware>();

app.UseRouting();

// Маршруты
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/health");

// 🔥 Создаем БД при старте
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        Console.WriteLine("✅ База данных создана/проверена");
        
        await DatabaseSeeder.SeedAsync(dbContext);
        Console.WriteLine("✅ Данные загружены");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка при создании БД: {ex.Message}");
        Console.WriteLine($"📁 Текущая директория: {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"📁 Файлы в директории: {string.Join(", ", Directory.GetFiles(Directory.GetCurrentDirectory()).Select(Path.GetFileName))}");
    }
}

Console.WriteLine($"✅ Приложение запущено на порту {port}");
await app.RunAsync();
