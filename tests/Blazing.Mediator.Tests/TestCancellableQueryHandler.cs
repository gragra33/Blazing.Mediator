namespace Blazing.Mediator.Tests;

public class TestCancellableQueryHandler : IRequestHandler<TestCancellableQuery, string>
{
    public static CancellationToken LastCancellationToken { get; private set; }

    public Task<string> Handle(TestCancellableQuery request, CancellationToken cancellationToken = default)
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult("Cancellable result");
    }
}