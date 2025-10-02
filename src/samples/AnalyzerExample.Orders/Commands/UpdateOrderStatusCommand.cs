using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Orders.Domain;

namespace AnalyzerExample.Orders.Commands;

public class UpdateOrderStatusCommand : IOrderCommand<OperationResult>, IAuditableCommand<OperationResult>, ITransactionalCommand<OperationResult>
{
    public int OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
    public bool RequiresTransaction => true;
}