using Blazing.Mediator;
using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Examples;

/// <summary>
/// Generic pre-processor middleware for all requests using Blazing.Mediator.
/// This demonstrates pre-processing logic, equivalent to MediatR's IRequestPreProcessor.
/// Compare with MediatR version: uses Blazing.Mediator middleware instead of MediatR.IRequestPreProcessor&lt;T&gt;.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class GenericRequestPreProcessor<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Gets the execution order for this middleware.
    /// Pre-processors should execute very early in the pipeline.
    /// </summary>
    public int Order => 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericRequestPreProcessor{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="writer">The text writer for output.</param>
    public GenericRequestPreProcessor(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Pre-processes the request before it reaches the handler.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response from the pipeline.</returns>
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync("- Starting Up");
        var result = await next();
        return result;
    }
}
