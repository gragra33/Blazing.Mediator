namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for AmbiguousRequest.
/// </summary>
public class AmbiguousRequestHandler : IRequestHandler<AmbiguousRequest, string>
{
    public ValueTask<string> Handle(AmbiguousRequest request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"ambiguous: {request.Value}");
    }
}