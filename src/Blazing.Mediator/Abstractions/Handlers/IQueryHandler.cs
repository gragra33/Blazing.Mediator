namespace Blazing.Mediator;

/// <summary>
/// Interface for query handlers in the CQRS pattern.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle</typeparam>
/// <typeparam name="TResponse">The type of response returned by the query</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
