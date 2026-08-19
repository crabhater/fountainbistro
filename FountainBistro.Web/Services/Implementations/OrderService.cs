using FountainBistro.Web.Models.Entities;
using FountainBistro.Web.Models.Enums;
using FountainBistro.Web.Services.Abstractions;
using FountainBistro.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FountainBistro.Web.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext dbContext, ILogger<OrderService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(Guid userId, Cart cart, string? comment = null)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.New,
            TotalAmount = cart.TotalSum,
            Comment = comment,
            CreatedAt = DateTime.UtcNow,
            Items = cart.Items.Select(item => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Price
            }).ToList()
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Order created: {OrderId} for user {UserId} with total {Total}", 
            order.Id, userId, order.TotalAmount);

        return order;
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<Order?> GetCurrentOrderAsync(Guid userId)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        var order = await _dbContext.Orders.FindAsync(orderId);
        if (order == null) return false;

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        
        if (status == OrderStatus.Completed || status == OrderStatus.Cancelled)
        {
            order.CompletedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, status);
        return true;
    }

    public async Task<List<Order>> GetUserOrdersAsync(Guid userId)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> CancelOrderAsync(Guid orderId)
    {
        return await UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled);
    }

    public async Task<List<Order>> GetActiveOrdersAsync()
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
    }
}
