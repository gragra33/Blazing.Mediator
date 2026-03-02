namespace Blazing.Mediator.Tests;

public class TestNullQueryHandler : IRequestHandler<TestNullQuery, string>
{
    public ValueTask<string> Handle(TestNullQuery request, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult((string)null!); // Simulate returning null
    }
}