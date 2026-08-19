using FountainBistro.Web.Models.Entities;
using FountainBistro.Web.Services.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace FountainBistro.Web.Services.Implementations;

public class CartService : ICartService
{
    private readonly IMemoryCache _cache;
    private readonly IProductService _productService;
    private readonly ILogger<CartService> _logger;
    private const string CartCachePrefix = "cart_";

    public CartService(IMemoryCache cache, IProductService productService, ILogger<CartService> logger)
    {
        _cache = cache;
        _productService = productService;
        _logger = logger;
    }

    private string GetCacheKey(Guid userId) => $"{CartCachePrefix}{userId}";

    public Cart GetOrCreateCart(Guid userId)
    {
        var key = GetCacheKey(userId);
        if (_cache.TryGetValue(key, out Cart? cart) && cart != null)
        {
            return cart;
        }

        cart = new Cart { UserId = userId };
        _cache.Set(key, cart, TimeSpan.FromMinutes(30));
        _logger.LogDebug("Created new cart for user: {UserId}", userId);
        return cart;
    }

    public async void AddItem(Guid userId, Guid productId, int quantity)
    {
        var cart = GetOrCreateCart(userId);
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", productId);
            return;
        }

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = productId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl
            });
        }

        // Обновляем кэш
        var key = GetCacheKey(userId);
        _cache.Set(key, cart, TimeSpan.FromMinutes(30));
        _logger.LogDebug("Added {Quantity} of {ProductName} to cart for user {UserId}", 
            quantity, product.Name, userId);
    }

    public void RemoveItem(Guid userId, Guid productId)
    {
        var cart = GetOrCreateCart(userId);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            cart.Items.Remove(item);
            var key = GetCacheKey(userId);
            _cache.Set(key, cart, TimeSpan.FromMinutes(30));
            _logger.LogDebug("Removed {ProductName} from cart for user {UserId}", 
                item.ProductName, userId);
        }
    }

    public void UpdateQuantity(Guid userId, Guid productId, int quantity)
    {
        var cart = GetOrCreateCart(userId);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            
            var key = GetCacheKey(userId);
            _cache.Set(key, cart, TimeSpan.FromMinutes(30));
            _logger.LogDebug("Updated quantity for {ProductName} to {Quantity} for user {UserId}", 
                item.ProductName, quantity, userId);
        }
    }

    public void ClearCart(Guid userId)
    {
        var key = GetCacheKey(userId);
        _cache.Remove(key);
        _logger.LogDebug("Cleared cart for user {UserId}", userId);
    }

    public Cart? GetCart(Guid userId)
    {
        var key = GetCacheKey(userId);
        _cache.TryGetValue(key, out Cart? cart);
        return cart;
    }

    public Task<bool> SyncWithDatabaseAsync(Guid userId)
    {
        // TODO: Синхронизация с БД при оформлении заказа
        return Task.FromResult(true);
    }
}
