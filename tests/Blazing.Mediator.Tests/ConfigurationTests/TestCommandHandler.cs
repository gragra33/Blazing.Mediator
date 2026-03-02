namespace Blazing.Mediator.Tests.ConfigurationTests;

public class CfgTestCommandHandler : IRequestHandler<CfgTestCommand>
{
    public ValueTask Handle(CfgTestCommand request, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}

