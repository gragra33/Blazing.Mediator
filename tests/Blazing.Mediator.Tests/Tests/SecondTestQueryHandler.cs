namespace Blazing.Mediator.Tests;

/// <summary>
/// Second test query handler for multiple handler tests.
/// </summary>
public class SecondTestQueryHandler : IRequestHandler<TestQuery, string>
{
    public Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Second: {request.Value}");
    }
}