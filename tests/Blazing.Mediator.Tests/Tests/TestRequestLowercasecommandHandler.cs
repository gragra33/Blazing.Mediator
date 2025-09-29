namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestRequestLowercasecommand.
/// </summary>
public class TestRequestLowercasecommandHandler : IRequestHandler<TestRequestLowercasecommand, int>
{
    public Task<int> Handle(TestRequestLowercasecommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Value?.Length ?? 0);
    }
}