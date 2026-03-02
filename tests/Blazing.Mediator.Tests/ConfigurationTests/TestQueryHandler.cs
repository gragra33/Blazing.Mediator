namespace Blazing.Mediator.Tests.ConfigurationTests;

public class CfgTestQueryHandler : IRequestHandler<CfgTestQuery, string>
{
    public ValueTask<string> Handle(CfgTestQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"Handled: {request.Value}");
    }
}

