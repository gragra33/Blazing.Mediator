using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Commands;

public class ProcessBulkOrdersCommand : ICommand<OperationResult<int>>, IAuditableCommand<OperationResult<int>>, ITransactionalCommand<OperationResult<int>>
{
    public List<int> OrderIds { get; set; } = new();
    public BulkOrderAction Action { get; set; }
    public Dictionary<string, object> ActionParameters { get; set; } = new();
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
    public bool RequiresTransaction => true;
}