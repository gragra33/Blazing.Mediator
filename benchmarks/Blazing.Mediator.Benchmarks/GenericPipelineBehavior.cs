using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Benchmarks;

public class GenericPipelineBehavior<TRequest, TResponse>(TextWriter writer)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("-- Handling Request");
        var response = await next();
        await writer.WriteLineAsync("-- Finished Request");
        return response;
    }
}