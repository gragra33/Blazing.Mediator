namespace Blazing.Mediator.Tests;

public class GenericQueryHandler : IRequestHandler<GenericQuery<List<int>>, string>
{
    public Task<string> Handle(GenericQuery<List<int>> request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Count: {request.Data?.Count ?? 0}");
    }
}