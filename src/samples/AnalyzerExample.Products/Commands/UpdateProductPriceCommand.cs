using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Products.Commands;

public class UpdateProductPriceCommand : IProductCommand<OperationResult>, IAuditableCommand<OperationResult>
{
    public int ProductId { get; set; }
    public decimal NewPrice { get; set; }
    public decimal OldPrice { get; set; }
    public string PriceChangeReason { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
}