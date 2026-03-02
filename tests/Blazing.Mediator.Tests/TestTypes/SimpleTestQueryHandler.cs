namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Handler for SimpleTestQuery.
/// </summary>
public class SimpleTestQueryHandler : IRequestHandler<SimpleTestQuery, string>
{
    public ValueTask<string> Handle(SimpleTestQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"Result for: {request.SearchTerm}");
    }
}