namespace FountainBistro.Web.Models.DTOs.Product;

public class CategoryDto
{
    public string Name { get; set; } = string.Empty;
    public List<ProductDto> Products { get; set; } = new();
}
