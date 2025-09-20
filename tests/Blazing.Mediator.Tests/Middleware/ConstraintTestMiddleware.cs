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

/// <summary>
/// Test notification middleware with constraint for testing notification constraint analysis.
/// </summary>
/// <typeparam name="TNotification">Notification type that must be a reference type.</typeparam>
public class NotificationConstraintMiddleware<TNotification> : INotificationMiddleware
    where TNotification : class, INotification
{
    public int Order => 600;

    public async Task InvokeAsync<TNotificationInner>(TNotificationInner notification, NotificationDelegate<TNotificationInner> next, CancellationToken cancellationToken)
        where TNotificationInner : INotification
    {
        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Test notification middleware with interface constraint.
/// </summary>
public class DomainEventNotificationMiddleware : INotificationMiddleware
{
    public int Order => 700;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}