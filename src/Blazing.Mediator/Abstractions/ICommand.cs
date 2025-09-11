namespace Blazing.Mediator;

/// <summary>
/// Marker interface for commands that don't return a response in the CQRS pattern.
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// Marker interface for commands in the CQRS pattern.
/// All command classes should implement this interface.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the command</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
