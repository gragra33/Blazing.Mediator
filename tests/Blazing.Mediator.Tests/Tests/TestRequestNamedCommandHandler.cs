namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestRequestNamedCommand.
/// </summary>
public class TestRequestNamedCommandHandler : IRequestHandler<TestRequestNamedCommand, bool>
{
    public Task<bool> Handle(TestRequestNamedCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}