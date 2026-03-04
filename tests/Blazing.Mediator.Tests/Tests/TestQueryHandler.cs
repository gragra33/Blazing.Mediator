namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test query handler for Statistics namespace.
/// </summary>
public class TestsTestQueryHandler : IRequestHandler<TestsTestQuery, string>
{
    public ValueTask<string> Handle(TestsTestQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"Handled: {request.Value}");
    }
}