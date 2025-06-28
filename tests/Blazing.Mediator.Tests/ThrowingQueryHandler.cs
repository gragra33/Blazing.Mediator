namespace Blazing.Mediator.Tests;

public class ThrowingQueryHandler : IRequestHandler<ThrowingQuery, string>
{
    public Task<string> Handle(ThrowingQuery request, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Query handler threw an exception");
    }
}