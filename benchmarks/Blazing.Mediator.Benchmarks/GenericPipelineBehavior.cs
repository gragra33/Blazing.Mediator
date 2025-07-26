using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Blazing.Mediator;
using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Benchmarks
{
    public class GenericPipelineBehavior<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly TextWriter _writer;

        public GenericPipelineBehavior(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync("-- Handling Request");
            var response = await next();
            await _writer.WriteLineAsync("-- Finished Request");
            return response;
        }
    }
}