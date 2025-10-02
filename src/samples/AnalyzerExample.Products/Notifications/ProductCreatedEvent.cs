using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Products.Domain;

namespace AnalyzerExample.Products.Notifications;

/// <summary>
/// Product domain events and notifications
/// </summary>
public class ProductCreatedEvent : IDomainEvent
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "ProductCreated";
    public int Version => 1;
}