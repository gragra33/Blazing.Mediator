using Blazing.Mediator;

namespace Blazing.Mediator.Examples;

/// <summary>
/// Constrained post-processor middleware specifically for Ping requests using Blazing.Mediator.
/// This demonstrates conditional middleware that only applies to specific request types.
/// Compare with MediatR version: uses Blazing.Mediator conditional middleware instead of constrained pipeline behaviors.
/// </summary>
public class ConstrainedRequestPostProcessor : IConditionalMiddleware<Ping, Pong>
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Gets the execution order for this middleware.
    /// Constrained post-processors should execute closest to the handler.
    /// </summary>
    public int Order => 25;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstrainedRequestPostProcessor"/> class.
    /// </summary>
    /// <param name="writer">The text writer for output.</param>
    public ConstrainedRequestPostProcessor(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Determines whether this middleware should be applied to the request.
    /// </summary>
    /// <param name="request">The request to evaluate.</param>
    /// <returns>True if the middleware should be applied; otherwise, false.</returns>
    public bool ShouldExecute(Ping request)
    {
        // Only execute for Ping requests
        return true;
    }

    /// <summary>
    /// Post-processes Ping requests specifically.
    /// </summary>
    /// <param name="request">The ping request.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The pong response.</returns>
    public async Task<Pong> HandleAsync(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
    {
        var response = await next();
        await _writer.WriteLineAsync("- All Done with Ping");
        return response;
    }
}
