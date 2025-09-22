namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Request-based query handler.
/// </summary>
public class RequestBasedQueryHandler : IRequestHandler<RequestBasedQuery, int>
{
    public Task<int> Handle(RequestBasedQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(42);
    }
}