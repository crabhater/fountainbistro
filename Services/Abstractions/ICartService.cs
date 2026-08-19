using FountainBistro.Web.Models.Entities;

namespace FountainBistro.Web.Services.Abstractions;

public interface ICartService
{
    Cart GetOrCreateCart(Guid userId);
    void AddItem(Guid userId, Guid productId, int quantity);
    void RemoveItem(Guid userId, Guid productId);
    void UpdateQuantity(Guid userId, Guid productId, int quantity);
    void ClearCart(Guid userId);
    Cart? GetCart(Guid userId);
    Task<bool> SyncWithDatabaseAsync(Guid userId);
}
