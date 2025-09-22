using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests.TestMiddleware;

/// <summary>
/// Test middleware with interface constraint for testing constraint analysis.
/// </summary>
/// <typeparam name="TRequest">Request type that must implement ICommand.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public class InterfaceConstraintMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public int Order => 200;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}