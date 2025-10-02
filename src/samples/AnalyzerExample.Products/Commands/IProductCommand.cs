using Blazing.Mediator;

namespace AnalyzerExample.Products.Commands;

/// <summary>
/// Product-specific command interfaces
/// </summary>
public interface IProductCommand : ICommand
{
    int ProductId { get; set; }
}