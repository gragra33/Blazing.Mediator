namespace Blazing.Mediator;

/// <summary>
/// Marker interface for requests that don't return a value (Commands)
/// </summary>
public interface IRequest
{
}

/// <summary>
/// Interface for requests that return a value of type TResponse (Queries)
/// </summary>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface IRequest<out TResponse>
{
}
