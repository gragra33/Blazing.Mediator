namespace Blazing.Mediator;

/// <summary>
/// Marker interface to represent a stream request with a response
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IStreamRequest<TResponse> : IRequest<TResponse>
{
}