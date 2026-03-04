namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestRequestLowercasecommand.
/// </summary>
public class TestRequestLowercasecommandHandler : IRequestHandler<TestRequestLowercasecommand, int>
{
    public ValueTask<int> Handle(TestRequestLowercasecommand request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(request.Value?.Length ?? 0);
    }
}