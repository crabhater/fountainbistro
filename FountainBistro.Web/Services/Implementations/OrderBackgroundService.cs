using FountainBistro.Web.Services.Abstractions;
using FountainBistro.Web.Models.Enums;

namespace FountainBistro.Web.Services.Implementations;

public class OrderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderBackgroundService> _logger;

    public OrderBackgroundService(IServiceProvider serviceProvider, ILogger<OrderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Проверяем каждые 3 секунды
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                await ProcessActiveOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OrderBackgroundService");
            }
        }

        _logger.LogInformation("OrderBackgroundService stopped");
    }

    private async Task ProcessActiveOrdersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

        try
        {
            // Получаем все активные заказы (не завершенные и не отмененные)
            var activeOrders = await orderService.GetActiveOrdersAsync();
            
            if (!activeOrders.Any())
            {
                return;
            }

            _logger.LogDebug("Processing {Count} active orders", activeOrders.Count);

            foreach (var order in activeOrders)
            {
                await ProcessOrderAsync(order, orderService);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing active orders");
        }
    }

    private async Task ProcessOrderAsync(Models.Entities.Order order, IOrderService orderService)
    {
        try
        {
            // Если заказ уже завершен - пропускаем
            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            {
                return;
            }

            var elapsed = DateTime.UtcNow - order.CreatedAt;
            
            _logger.LogDebug("Order {OrderId}: Status={Status}, Elapsed={Elapsed}s", 
                order.Id, order.Status, elapsed.TotalSeconds);

            // Демо-тайминг (в секундах)
            if (order.Status == OrderStatus.New && elapsed.TotalSeconds >= 3)
            {
                await orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Paid);
                _logger.LogInformation("✅ Order {OrderId} status changed to Paid", order.Id);
            }
            else if (order.Status == OrderStatus.Paid && elapsed.TotalSeconds >= 8)
            {
                await orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.InProgress);
                _logger.LogInformation("👨‍🍳 Order {OrderId} status changed to InProgress", order.Id);
            }
            else if (order.Status == OrderStatus.InProgress && elapsed.TotalSeconds >= 16)
            {
                await orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Ready);
                _logger.LogInformation("✅ Order {OrderId} status changed to Ready", order.Id);
            }
            else if (order.Status == OrderStatus.Ready && elapsed.TotalSeconds >= 22)
            {
                await orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Completed);
                _logger.LogInformation("🎉 Order {OrderId} status changed to Completed", order.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order {OrderId}", order.Id);
        }
    }
}
