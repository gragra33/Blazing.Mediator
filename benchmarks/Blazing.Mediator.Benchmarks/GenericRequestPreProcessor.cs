namespace Blazing.Mediator.Benchmarks;

// Pre-processing logic integrated into a middleware component
internal class GenericRequestPreProcessor<TRequest, TResponse>(TextWriter writer)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("- Starting Up").ConfigureAwait(false);
        var response = await next().ConfigureAwait(false);
        return response;
    }
}
