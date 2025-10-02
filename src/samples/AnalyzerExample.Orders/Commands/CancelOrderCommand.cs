using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Orders.Commands;

public class CancelOrderCommand : IOrderCommand, IAuditableCommand, ITransactionalCommand
{
    public int OrderId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
    public bool RefundPayment { get; set; } = true;
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
    public bool RequiresTransaction => true;
}