using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Products.Notifications;

public class ProductPriceChangedEvent : IDomainEvent
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "ProductPriceChanged";
    public int Version => 1;
}