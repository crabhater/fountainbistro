using Serilog;
using FountainBistro.Web.Infrastructure.Data;
using FountainBistro.Web.Infrastructure.Extensions;
using FountainBistro.Web.Infrastructure.Data.SeedData;
using FountainBistro.Web.Middleware;
using FountainBistro.Web.Services.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Настройка логирования
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/app-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        );
});

// 2. Добавление сервисов
builder.Services.AddControllersWithViews();

// 3. Настройка БД
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connectionString);
});

// 4. Регистрация сервисов
builder.Services.AddApplicationServices();

// 5. Регистрация репозиториев
builder.Services.AddApplicationRepositories();

// 6. AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// 7. FluentValidation
builder.Services.AddFluentValidationServices();

// 8. Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

// 9. Настройка Cookie
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.SameAsRequest;
});

// 10. Настройка кэширования
builder.Services.AddMemoryCache();

// 11. HTTP клиенты
builder.Services.AddHttpClient();

// 12. Фоновый сервис - только AddHostedService
builder.Services.AddHostedService<OrderBackgroundService>();

var app = builder.Build();

// 13. Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSerilogRequestLogging();

app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();

// 14. Auth Middleware
app.UseMiddleware<AuthMiddleware>();

// 15. Маршруты
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/health");

// 16. Инициализация БД и Seed данных
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    await DatabaseSeeder.SeedAsync(dbContext);
}

app.Run();
