using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Products.Notifications;

public class ProductUpdatedEvent : IDomainEvent
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Dictionary<string, object> Changes { get; set; } = new();
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "ProductUpdated";
    public int Version => 1;
}