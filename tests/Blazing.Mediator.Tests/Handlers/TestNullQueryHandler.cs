namespace Blazing.Mediator.Tests;

public class TestNullQueryHandler : IRequestHandler<TestNullQuery, string>
{
    public Task<string> Handle(TestNullQuery request, CancellationToken cancellationToken = default)
    {
        return null!; // Simulate returning null
    }
}