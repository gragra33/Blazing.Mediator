namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestRequestLowercasecommand.
/// </summary>
public class TestRequestLowercasecommandHandler : IRequestHandler<TestRequestLowercasecommand, int>
{
    public async ValueTask<int> Handle(TestRequestLowercasecommand request, CancellationToken cancellationToken)
    {
        return request.Value?.Length ?? 0;
    }
}