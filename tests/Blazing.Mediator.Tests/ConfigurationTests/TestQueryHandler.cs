namespace Blazing.Mediator.Tests.ConfigurationTests;

public class TestQueryHandler : IRequestHandler<TestQuery, string>
{
    public async ValueTask<string> Handle(TestQuery request, CancellationToken cancellationToken)
    {
        return $"Handled: {request.Value}";
    }
}