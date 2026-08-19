using FountainBistro.Web.Services.Abstractions;
using FountainBistro.Web.Services.Implementations;

namespace FountainBistro.Web.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        // services.AddScoped<IPaymentService, PaymentService>();
        // services.AddScoped<INotificationService, NotificationService>();
        
        return services;
    }

    public static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        // Регистрируем репозитории
        // services.AddScoped<IUserRepository, UserRepository>();
        // services.AddScoped<IOrderRepository, OrderRepository>();
        // services.AddScoped<IProductRepository, ProductRepository>();
        // services.AddScoped<IPaymentRepository, PaymentRepository>();
        // services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();
        
        return services;
    }

    public static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
    {
        // services.AddFluentValidationAutoValidation();
        // services.AddValidatorsFromAssemblyContaining<Program>();
        
        return services;
    }
}
