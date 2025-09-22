namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Handler for ReturningTestCommand.
/// </summary>
public class ReturningTestCommandHandler : IRequestHandler<ReturningTestCommand, int>
{
    public Task<int> Handle(ReturningTestCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Value.Length);
    }
}