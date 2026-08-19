namespace FountainBistro.Web.Models.Entities;

public class CartItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
}

public class Cart
{
    public Guid UserId { get; set; }
    public List<CartItem> Items { get; set; } = new();
    public decimal TotalSum => Items.Sum(i => i.Price * i.Quantity);
    public int TotalItems => Items.Sum(i => i.Quantity);
}
