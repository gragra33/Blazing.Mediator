namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Handler for ReturningTestCommand.
/// </summary>
public class ReturningTestCommandHandler : IRequestHandler<ReturningTestCommand, int>
{
    public ValueTask<int> Handle(ReturningTestCommand request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(request.Value.Length);
    }
}