using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Products.Notifications;

public class ProductReviewAddedEvent : IDomainEvent
{
    public int ProductId { get; set; }
    public int ReviewId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "ProductReviewAdded";
    public int Version => 1;
}