namespace Blazing.Mediator.Tests;

public class TestMultiInterfaceHandler :
    IRequestHandler<TestMultiCommand>,
    IRequestHandler<TestMultiQuery, string>
{
    public Task Handle(TestMultiCommand request, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<string> Handle(TestMultiQuery request, CancellationToken cancellationToken = default)
        => Task.FromResult("multi");
}