using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Generic middleware used for testing type instantiation handling in the pipeline.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class GenericMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 0;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var result = await next();
        if (result is string str)
        {
            return (TResponse)(object)$"Generic: {str}";
        }
        return result;
    }
}