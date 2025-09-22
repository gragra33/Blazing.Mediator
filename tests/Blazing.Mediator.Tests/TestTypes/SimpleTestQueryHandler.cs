namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Handler for SimpleTestQuery.
/// </summary>
public class SimpleTestQueryHandler : IRequestHandler<SimpleTestQuery, string>
{
    public Task<string> Handle(SimpleTestQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Result for: {request.SearchTerm}");
    }
}