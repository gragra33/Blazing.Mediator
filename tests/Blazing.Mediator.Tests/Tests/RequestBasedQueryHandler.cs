namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Request-based query handler.
/// </summary>
public class RequestBasedQueryHandler : IRequestHandler<RequestBasedQuery, int>
{
    public ValueTask<int> Handle(RequestBasedQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(42);
    }
}