namespace Blazing.Mediator.Examples;

/// <summary>
/// Generic handler that can handle any request using Blazing.Mediator.
/// This demonstrates generic handlers that can work with multiple request types.
/// Compare with MediatR version: uses Blazing.Mediator interfaces instead of MediatR interfaces.
/// Note: This is a simplified example - Blazing.Mediator handles generic resolution differently than MediatR.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class GenericHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericHandler{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="writer">The text writer for output.</param>
    public GenericHandler(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Handles any request of type TRequest.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A default response of type TResponse.</returns>
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync($"--- Handled generic request: {typeof(TRequest).Name}");
        
        // Return default response (this is just for demonstration)
        return default(TResponse)!;
    }
}
