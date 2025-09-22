using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests.ConfigurationTests;

public class SecondTestMiddleware : IRequestMiddleware<TestQuery, string>
{
    public async Task<string> HandleAsync(TestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        var result = await next();
        return $"Second: {result}";
    }
}