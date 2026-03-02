namespace Blazing.Mediator.Tests;

/// <summary>
/// Middleware with no parameterless constructor used for testing middleware creation failure scenarios.
/// Excluded from source-generator auto-discovery so it does not break global ContainerMetadata init.
/// </summary>
[ExcludeFromAutoDiscovery]
public class MiddlewareWithNoParameterlessConstructor : IRequestMiddleware<MiddlewareTestQuery, string>
{
    public MiddlewareWithNoParameterlessConstructor(string requiredParameter)
    {
        // This will cause Activator.CreateInstance to fail
    }

    public int Order => 0;

    public async ValueTask<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}