using Blazing.Mediator;

namespace AnalyzerExample.Orders.Commands;

/// <summary>
/// Order-specific command interfaces
/// </summary>
public interface IOrderCommand : ICommand
{
    int OrderId { get; set; }
}