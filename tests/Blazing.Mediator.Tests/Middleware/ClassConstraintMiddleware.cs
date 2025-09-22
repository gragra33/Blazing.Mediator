using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests.TestMiddleware;

/// <summary>
/// Test middleware with class constraint for testing constraint analysis.
/// </summary>
/// <typeparam name="TRequest">Request type that must be a reference type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public class ClassConstraintMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    public int Order => 100;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}