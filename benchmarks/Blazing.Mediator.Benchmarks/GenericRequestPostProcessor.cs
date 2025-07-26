using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Blazing.Mediator;
using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Benchmarks
{
    // Post-processing logic integrated into a middleware component
    public class GenericRequestPostProcessor<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly TextWriter _writer;

        public GenericRequestPostProcessor(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var response = await next();
            await _writer.WriteLineAsync("- All Done");
            return response;
        }
    }
}