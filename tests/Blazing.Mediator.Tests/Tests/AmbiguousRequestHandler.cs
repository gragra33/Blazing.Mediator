namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for AmbiguousRequest.
/// </summary>
public class AmbiguousRequestHandler : IRequestHandler<AmbiguousRequest, string>
{
    public Task<string> Handle(AmbiguousRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"ambiguous: {request.Value}");
    }
}