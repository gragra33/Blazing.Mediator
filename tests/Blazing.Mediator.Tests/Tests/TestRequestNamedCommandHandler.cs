namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestRequestNamedCommand.
/// </summary>
public class TestRequestNamedCommandHandler : IRequestHandler<TestRequestNamedCommand, bool>
{
    public async ValueTask<bool> Handle(TestRequestNamedCommand request, CancellationToken cancellationToken)
    {
        return true;
    }
}