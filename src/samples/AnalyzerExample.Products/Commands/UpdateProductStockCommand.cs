using AnalyzerExample.Common.Domain;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Commands;

public class UpdateProductStockCommand : ICommand<OperationResult<int>>
{
    public int ProductId { get; set; }
    public int QuantityChange { get; set; } // Can be positive (restock) or negative (sale)
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceId { get; set; } // Order ID, Purchase Order ID, etc.
}