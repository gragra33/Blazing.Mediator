namespace Blazing.Mediator.Tests;

public class TestRegistrationCommandHandler : IRequestHandler<TestRegistrationCommand>
{
    public Task Handle(TestRegistrationCommand request, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}