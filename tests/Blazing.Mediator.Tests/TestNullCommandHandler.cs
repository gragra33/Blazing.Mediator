namespace Blazing.Mediator.Tests;

public class TestNullCommandHandler : IRequestHandler<TestNullCommand>
{
    public Task Handle(TestNullCommand request, CancellationToken cancellationToken = default)
    {
        return null!; // Simulate returning null
    }
}