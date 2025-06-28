namespace Blazing.Mediator.Tests;

public abstract class TestAbstractHandler : IRequestHandler<TestAbstractCommand>
{
    public abstract Task Handle(TestAbstractCommand request, CancellationToken cancellationToken = default);
}