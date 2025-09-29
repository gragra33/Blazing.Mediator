using Blazing.Mediator;
using System.Diagnostics;

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
        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"[TIMING] {DateTime.Now:HH:mm:ss.fff} - GenericRequestPreProcessor starting for {typeof(TRequest).Name}");
        
        await _writer.WriteLineAsync("- Starting Up");
        var result = await next();
        
        stopwatch.Stop();
        Console.WriteLine($"[TIMING] {DateTime.Now:HH:mm:ss.fff} - GenericRequestPreProcessor completed for {typeof(TRequest).Name} in {stopwatch.ElapsedMilliseconds}ms");
        
        return result;
    }
}
