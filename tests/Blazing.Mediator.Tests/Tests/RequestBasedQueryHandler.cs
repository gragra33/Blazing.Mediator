namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Request-based query handler.
/// </summary>
public class RequestBasedQueryHandler : IRequestHandler<RequestBasedQuery, int>
{
    public async ValueTask<int> Handle(RequestBasedQuery request, CancellationToken cancellationToken)
    {
        return 42;
    }
}