namespace Blazing.Mediator;

/// <summary>
/// Central mediator for handling requests and implementing CQRS pattern
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Send a command that doesn't return a value
    /// </summary>
    /// <param name="request">The command to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task Send(IRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a query that returns a value
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected</typeparam>
    /// <param name="request">The query to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task containing the response</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
