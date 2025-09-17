using Blazing.Mediator.Abstractions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Blazing.Mediator.Benchmarks;

// Pre-processing logic integrated into a middleware component
public class GenericRequestPreProcessor<TRequest, TResponse>(TextWriter writer)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("- Starting Up");
        var response = await next();
        return response;
    }
}