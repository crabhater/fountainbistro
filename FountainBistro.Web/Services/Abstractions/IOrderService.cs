using FountainBistro.Web.Models.Entities;
using FountainBistro.Web.Models.Enums;

namespace FountainBistro.Web.Services.Abstractions;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Guid userId, Cart cart, string? comment = null);
    Task<Order?> GetOrderByIdAsync(Guid orderId);
    Task<Order?> GetCurrentOrderAsync(Guid userId);
    Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
    Task<List<Order>> GetUserOrdersAsync(Guid userId);
    Task<bool> CancelOrderAsync(Guid orderId);
    Task<List<Order>> GetActiveOrdersAsync(); // Новый метод
}
