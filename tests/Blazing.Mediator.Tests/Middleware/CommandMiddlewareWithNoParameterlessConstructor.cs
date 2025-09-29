namespace Blazing.Mediator.Tests;

/// <summary>
/// Command middleware with no parameterless constructor used for testing middleware creation failure scenarios.
/// </summary>
public class CommandMiddlewareWithNoParameterlessConstructor : IRequestMiddleware<TestCommand>
{
    public CommandMiddlewareWithNoParameterlessConstructor(string requiredParameter)
    {
        // This will cause Activator.CreateInstance to fail
    }

    public int Order => 0;

    public async Task HandleAsync(TestCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
    }
}