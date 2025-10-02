using AnalyzerExample.Orders.Domain;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Notifications;

public class OrderDelayedNotification : INotification
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public OrderStatus CurrentStatus { get; set; }
    public DateTime ExpectedDate { get; set; }
    public DateTime ActualDate { get; set; }
    public string DelayReason { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}