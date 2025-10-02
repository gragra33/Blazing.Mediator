using AnalyzerExample.Common.Domain;

namespace AnalyzerExample.Orders.Domain;

/// <summary>
/// Order domain models and related entities
/// </summary>
public class Order : BaseEntity, IAuditableEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public OrderShippingAddress? ShippingAddress { get; set; }
    public OrderBillingAddress? BillingAddress { get; set; }
    public List<OrderStatusHistory> StatusHistory { get; set; } = new();
}