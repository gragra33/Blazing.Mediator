using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests.TestMiddleware;

/// <summary>
/// Test middleware with new() constraint for testing constraint analysis.
/// </summary>
/// <typeparam name="TRequest">Request type that must have a parameterless constructor.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public class NewConstraintMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, new()
{
    public int Order => 300;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}