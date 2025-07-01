using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Middleware that throws an exception when accessing the Order property, used for testing exception handling in ordering.
/// </summary>
public class MiddlewareWithExceptionInOrder : IRequestMiddleware<MiddlewareTestQuery, string>
{
    public int Order 
    { 
        get 
        { 
            throw new InvalidOperationException("Cannot get order"); 
        } 
    }

    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        var result = await next();
        return $"Exception Order: {result}";
    }
}