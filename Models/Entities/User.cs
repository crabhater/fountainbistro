namespace FountainBistro.Web.Models.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<PushSubscription> PushSubscriptions { get; set; } = new List<PushSubscription>();
}
