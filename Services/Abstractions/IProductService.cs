using FountainBistro.Web.Models.DTOs.Product;

namespace FountainBistro.Web.Services.Abstractions;

public interface IProductService
{
    Task<List<CategoryDto>> GetMenuByCategoriesAsync();
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<bool> IsProductAvailableAsync(Guid id);
}
