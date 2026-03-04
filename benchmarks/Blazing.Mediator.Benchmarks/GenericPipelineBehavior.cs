namespace Blazing.Mediator.Benchmarks;

[ExcludeFromAutoDiscovery]
internal class GenericPipelineBehavior<TRequest, TResponse>(TextWriter writer)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("-- Handling Request").ConfigureAwait(false);
        TResponse response = await next().ConfigureAwait(false);
        await writer.WriteLineAsync("-- Finished Request").ConfigureAwait(false);
        return response;
    }
}