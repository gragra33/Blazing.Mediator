using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Orders.Commands;

public class CompleteOrderCommand : IOrderCommand<OperationResult>, IAuditableCommand<OperationResult>
{
    public int OrderId { get; set; }
    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;
    public string? DeliveryNotes { get; set; }
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
}