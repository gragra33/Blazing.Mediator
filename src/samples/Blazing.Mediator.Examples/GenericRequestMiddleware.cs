using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Examples;

/// <summary>
/// Generic middleware for all requests using Blazing.Mediator.
/// This demonstrates middleware pattern which is Blazing.Mediator's equivalent of MediatR's pipeline behaviors.
/// Compare with MediatR version: uses Blazing.Mediator middleware instead of MediatR.IPipelineBehavior&lt;T,R&gt;.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class GenericRequestMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Gets the execution order for this middleware.
    /// Main middleware should execute outermost to wrap everything.
    /// </summary>
    public int Order => 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericRequestMiddleware{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="writer">The text writer for output.</param>
    public GenericRequestMiddleware(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Handles the request through the middleware pipeline.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response from the pipeline.</returns>
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync("-- Handling Request");
        var result = await next();
        await _writer.WriteLineAsync("-- Finished Request");
        return result;
    }
}
