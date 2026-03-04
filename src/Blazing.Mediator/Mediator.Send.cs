namespace Blazing.Mediator;

public sealed partial class Mediator
{
    /// <summary>
    /// Sends a command request through the middleware pipeline to its corresponding handler.
    /// </summary>
    /// <param name="request">The command request to send</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A ValueTask representing the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is found for the request type</exception>
    public ValueTask Send(IRequest request, CancellationToken cancellationToken = default)
    {
        if (GetDispatcher() is { } d)
            return d.SendAsync(request, cancellationToken);
        throw new InvalidOperationException(
            "Source generator dispatcher not found. Ensure AddMediator() is called and source generators are active.");
    }

    /// <summary>
    /// Sends a query request through the middleware pipeline to its corresponding handler and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected from the handler</typeparam>
    /// <param name="request">The query request to send</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A ValueTask containing the response from the handler</returns>
    /// <exception cref="InvalidOperationException">Thrown when no source-generator dispatcher is registered</exception>
    public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (GetDispatcher() is { } d)
            return d.SendAsync(request, cancellationToken);
        throw new InvalidOperationException(
            "Source generator dispatcher not found. Ensure AddMediator() is called and source generators are active.");
    }

}