namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Handler for SimpleTestQuery.
/// </summary>
public class SimpleTestQueryHandler : IRequestHandler<SimpleTestQuery, string>
{
    public async ValueTask<string> Handle(SimpleTestQuery request, CancellationToken cancellationToken)
    {
        return $"Result for: {request.SearchTerm}";
    }
}