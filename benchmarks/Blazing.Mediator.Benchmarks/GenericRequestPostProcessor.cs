using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Benchmarks;

// Post-processing logic integrated into a middleware component
internal class GenericRequestPostProcessor<TRequest, TResponse>(TextWriter writer)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next().ConfigureAwait(false);
        await writer.WriteLineAsync("- All Done").ConfigureAwait(false);
        return response;
    }
}
