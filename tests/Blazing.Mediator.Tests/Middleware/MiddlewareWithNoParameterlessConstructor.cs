namespace Blazing.Mediator.Tests;

/// <summary>
/// Middleware with no parameterless constructor used for testing middleware creation failure scenarios.
/// Excluded from auto-discovery so the source generator does not embed it in any request pipeline
/// (its DI-incompatible constructor would cause ContainerMetadata construction to fail globally).
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