namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test command handler for Statistics namespace.
/// </summary>
public class TestsTestCommandHandler : IRequestHandler<TestsTestCommand>
{
    public ValueTask Handle(TestsTestCommand request, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}