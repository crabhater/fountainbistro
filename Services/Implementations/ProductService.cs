using FountainBistro.Web.Models.DTOs.Product;
using FountainBistro.Web.Models.Entities;
using FountainBistro.Web.Services.Abstractions;
using FountainBistro.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FountainBistro.Web.Services.Implementations;

public class ProductService : IProductService
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductService> _logger;
    private const string MenuCacheKey = "menu_categories";

    public ProductService(AppDbContext dbContext, IMemoryCache cache, ILogger<ProductService> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetMenuByCategoriesAsync()
    {
        // Пытаемся получить из кэша
        if (_cache.TryGetValue(MenuCacheKey, out List<CategoryDto>? cachedMenu) && cachedMenu != null)
        {
            return cachedMenu;
        }

        try
        {
            var products = await _dbContext.Products
                .Where(p => p.IsAvailable)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.SortOrder)
                .ToListAsync();

            var categories = products
                .GroupBy(p => p.Category)
                .Select(g => new CategoryDto
                {
                    Name = g.Key,
                    Products = g.Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Category = p.Category,
                        ImageUrl = p.ImageUrl,
                        IsAvailable = p.IsAvailable,
                        QuantityInCart = 0
                    }).ToList()
                })
                .ToList();

            // Сохраняем в кэш на 10 минут
            _cache.Set(MenuCacheKey, categories, TimeSpan.FromMinutes(10));
            
            _logger.LogInformation("Menu loaded from database, {CategoriesCount} categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading menu from database");
            return new List<CategoryDto>();
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsAvailable);

        if (product == null)
            return null;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            ImageUrl = product.ImageUrl,
            IsAvailable = product.IsAvailable
        };
    }

    public async Task<bool> IsProductAvailableAsync(Guid id)
    {
        return await _dbContext.Products
            .AnyAsync(p => p.Id == id && p.IsAvailable);
    }
}
