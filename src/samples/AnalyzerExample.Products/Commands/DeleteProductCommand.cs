using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Products.Commands;

public class DeleteProductCommand : IProductCommand, IAuditableCommand, ITransactionalCommand
{
    public int ProductId { get; set; }
    public bool SoftDelete { get; set; } = true;
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
    public bool RequiresTransaction => true;
}