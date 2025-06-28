namespace Blazing.Mediator.Tests;

public class TestCancellableCommandHandler : IRequestHandler<TestCancellableCommand>
{
    public static CancellationToken LastCancellationToken { get; private set; }

    public Task Handle(TestCancellableCommand request, CancellationToken cancellationToken = default)
    {
        LastCancellationToken = cancellationToken;
        return Task.CompletedTask;
    }
}