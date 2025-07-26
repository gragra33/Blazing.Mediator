using Blazing.Mediator;
using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Examples;

/// <summary>
/// Generic post-processor middleware for all requests using Blazing.Mediator.
/// This demonstrates post-processing logic, equivalent to MediatR's IRequestPostProcessor.
/// Compare with MediatR version: uses Blazing.Mediator middleware instead of MediatR.IRequestPostProcessor&lt;T,R&gt;.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class GenericRequestPostProcessor<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Gets the execution order for this middleware.
    /// Post-processors should execute in the middle.
    /// </summary>
    public int Order => 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericRequestPostProcessor{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="writer">The text writer for output.</param>
    public GenericRequestPostProcessor(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Post-processes the request after the handler has executed.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response from the pipeline.</returns>
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();
        await _writer.WriteLineAsync("- All Done");
        return response;
    }
}
