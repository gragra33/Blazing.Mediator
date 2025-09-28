namespace Blazing.Mediator.Tests;

/// <summary>
/// Middleware with no parameterless constructor used for testing middleware creation failure scenarios.
/// </summary>
public class MiddlewareWithNoParameterlessConstructor : IRequestMiddleware<MiddlewareTestQuery, string>
{
    public MiddlewareWithNoParameterlessConstructor(string requiredParameter)
    {
        // This will cause Activator.CreateInstance to fail
    }

    public int Order => 0;

    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}