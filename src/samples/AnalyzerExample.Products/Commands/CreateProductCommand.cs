using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Products.Commands;

/// <summary>
/// Product commands demonstrating various patterns
/// </summary>
public class CreateProductCommand : IAuditableCommand<OperationResult<int>>, ITransactionalCommand<OperationResult<int>>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int InitialStock { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
    public bool RequiresTransaction => true;
}