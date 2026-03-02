namespace Blazing.Mediator.Tests;

/// <summary>
/// Command middleware with no parameterless constructor used for testing middleware creation failure scenarios.
/// Excluded from source-generator auto-discovery so it does not break global ContainerMetadata init.
/// </summary>
[ExcludeFromAutoDiscovery]
public class CommandMiddlewareWithNoParameterlessConstructor : IRequestMiddleware<TestCommand>
{
    public CommandMiddlewareWithNoParameterlessConstructor(string requiredParameter)
    {
        // This will cause Activator.CreateInstance to fail
    }

    public int Order => 0;

    public async ValueTask HandleAsync(TestCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
    }
}