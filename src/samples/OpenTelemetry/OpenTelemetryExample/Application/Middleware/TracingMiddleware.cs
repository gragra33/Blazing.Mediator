using System.Diagnostics;
using Blazing.Mediator;
using Blazing.Mediator.Abstractions;

namespace OpenTelemetryExample.Application.Middleware;

/// <summary>
/// Middleware for tracing requests and responses with OpenTelemetry.
/// </summary>
public class TracingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => -1000; // Run early

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var activitySource = new ActivitySource("OpenTelemetryExample.Mediator");
        using var activity = activitySource.StartActivity($"Mediator:{typeof(TRequest).Name}", ActivityKind.Internal);
        activity?.SetTag("mediator.request_type", typeof(TRequest).FullName);
        activity?.SetTag("mediator.tracing_middleware", true);
        var response = await next();
        activity?.SetTag("mediator.response_type", typeof(TResponse).FullName);
        return response;
    }
}

public class TracingMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    public int Order => -1000; // Run early

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        var activitySource = new ActivitySource("OpenTelemetryExample.Mediator");
        using var activity = activitySource.StartActivity($"Mediator:{typeof(TRequest).Name}", ActivityKind.Internal);
        activity?.SetTag("mediator.request_type", typeof(TRequest).FullName);
        activity?.SetTag("mediator.tracing_middleware", true);
        await next();
    }
}
