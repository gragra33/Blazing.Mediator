using Blazing.Mediator;

namespace AnalyzerExample.Orders.Queries;

/// <summary>
/// Order-specific query interfaces
/// </summary>
public interface IOrderQuery<TResponse> : IQuery<TResponse>
{
    bool IncludeCancelled { get; set; }
}