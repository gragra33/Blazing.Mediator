namespace Blazing.Mediator.Tests;

/// <summary>
/// Second test stream handler for multiple handler tests.
/// Excluded from auto-discovery to prevent conflicts with TestStreamHandler.
/// </summary>
[ExcludeFromAutoDiscovery]
public class SecondTestStreamHandler : IStreamRequestHandler<TestsTestStreamRequest, string>
{
    public async IAsyncEnumerable<string> Handle(TestsTestStreamRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return $"Second-{request.Value}";
        await Task.Yield();
    }
}