using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Orders.Commands;

public class ShipOrderCommand : IOrderCommand<OperationResult>, IAuditableCommand<OperationResult>
{
    public int OrderId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public DateTime ShippedAt { get; set; } = DateTime.UtcNow;
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
}