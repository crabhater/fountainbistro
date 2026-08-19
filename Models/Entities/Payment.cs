using FountainBistro.Web.Models.Enums;

namespace FountainBistro.Web.Models.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? SbpLink { get; set; }
    public string? ExternalPaymentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }
    public Order Order { get; set; } = null!;
}
