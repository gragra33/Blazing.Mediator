namespace Blazing.Mediator.Tests.ConfigurationTests;

public class TestCommandHandler : IRequestHandler<TestCommand>
{
    public ValueTask Handle(TestCommand request, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}