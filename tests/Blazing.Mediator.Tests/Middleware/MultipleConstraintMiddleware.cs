using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests.TestMiddleware;

/// <summary>
/// Test middleware with multiple constraints for testing constraint analysis.
/// </summary>
/// <typeparam name="TRequest">Request type with multiple constraints.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public class MultipleConstraintMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : class, IQuery<TResponse>, new()
{
    public int Order => 400;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}