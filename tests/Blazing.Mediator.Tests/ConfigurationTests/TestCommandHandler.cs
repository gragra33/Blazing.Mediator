namespace Blazing.Mediator.Tests.ConfigurationTests;

public class TestCommandHandler : IRequestHandler<TestCommand>
{
    public Task Handle(TestCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}