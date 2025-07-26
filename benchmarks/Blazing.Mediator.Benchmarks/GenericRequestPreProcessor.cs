using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Blazing.Mediator;
using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Benchmarks
{
    // Pre-processing logic integrated into a middleware component
    public class GenericRequestPreProcessor<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly TextWriter _writer;

        public GenericRequestPreProcessor(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync("- Starting Up");
            var response = await next();
            return response;
        }
    }
}