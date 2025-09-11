namespace Blazing.Mediator;

/// <summary>
/// Marker interface for queries in the CQRS pattern.
/// All query classes should implement this interface.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the query</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
