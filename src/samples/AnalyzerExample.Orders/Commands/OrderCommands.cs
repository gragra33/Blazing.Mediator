using Blazing.Mediator;

namespace AnalyzerExample.Orders.Commands;

public interface IOrderCommand<TResponse> : ICommand<TResponse>
{
    int OrderId { get; set; }
}