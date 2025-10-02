using Blazing.Mediator;

namespace AnalyzerExample.Orders.Notifications;

public class PaymentRequiredNotification : INotification
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDueDate { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}