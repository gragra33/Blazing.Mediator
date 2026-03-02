namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for AmbiguousRequest.
/// </summary>
public class AmbiguousRequestHandler : IRequestHandler<AmbiguousRequest, string>
{
    public async ValueTask<string> Handle(AmbiguousRequest request, CancellationToken cancellationToken)
    {
        return $"ambiguous: {request.Value}";
    }
}