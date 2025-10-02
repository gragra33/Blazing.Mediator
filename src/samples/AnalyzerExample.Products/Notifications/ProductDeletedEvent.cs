using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Products.Notifications;

public class ProductDeletedEvent : IDomainEvent
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public bool IsSoftDelete { get; set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "ProductDeleted";
    public int Version => 1;
}