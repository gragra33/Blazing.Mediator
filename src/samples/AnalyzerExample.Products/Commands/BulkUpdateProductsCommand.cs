using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Commands;

public class BulkUpdateProductsCommand : ICommand<OperationResult<int>>, IAuditableCommand<OperationResult<int>>, ITransactionalCommand<OperationResult<int>>
{
    public List<int> ProductIds { get; set; } = new();
    public BulkUpdateOptions Updates { get; set; } = new();
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
    public bool RequiresTransaction => true;
}