using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Examples.Streams;

/// <summary>
/// Generic stream request middleware that logs stream request processing.
/// This demonstrates stream middleware in Blazing.Mediator.
/// Compare with MediatR version: uses IStreamRequestMiddleware instead of pipeline behaviors.
/// </summary>
/// <typeparam name="TRequest">The stream request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class GenericStreamRequestMiddleware<TRequest, TResponse> : IStreamRequestMiddleware<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericStreamRequestMiddleware{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="writer">The text writer for output.</param>
    public GenericStreamRequestMiddleware(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Gets the execution order for this middleware.
    /// </summary>
    public int Order => 0;

    /// <summary>
    /// Handles the stream request processing with logging.
    /// </summary>
    /// <param name="request">The stream request to process</param>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of response items</returns>
    public async IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request, 
        StreamRequestHandlerDelegate<TResponse> next, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync("-- Handling StreamRequest");
        
        await foreach (var item in next())
        {
            yield return item;
        }
        
        await _writer.WriteLineAsync("-- Finished StreamRequest");
    }
}
