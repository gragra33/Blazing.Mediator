using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests.TestMiddleware;

/// <summary>
/// Test middleware with single type parameter constraint.
/// </summary>
/// <typeparam name="TRequest">Request type that must implement ICommand.</typeparam>
public class SingleParameterConstraintMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : ICommand
{
    public int Order => 500;

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
    }
}