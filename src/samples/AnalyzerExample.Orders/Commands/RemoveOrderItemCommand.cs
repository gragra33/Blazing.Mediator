using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Orders.Commands;

public class RemoveOrderItemCommand : IOrderCommand<OperationResult>, IAuditableCommand<OperationResult>, ITransactionalCommand<OperationResult>
{
    public int OrderId { get; set; }
    public int OrderItemId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
    public bool RequiresTransaction => true;
}