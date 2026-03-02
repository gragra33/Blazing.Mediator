namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Handler for ReturningTestCommand.
/// </summary>
public class ReturningTestCommandHandler : IRequestHandler<ReturningTestCommand, int>
{
    public async ValueTask<int> Handle(ReturningTestCommand request, CancellationToken cancellationToken)
    {
        return request.Value.Length;
    }
}