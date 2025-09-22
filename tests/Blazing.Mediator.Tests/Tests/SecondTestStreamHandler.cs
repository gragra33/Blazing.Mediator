namespace Blazing.Mediator.Tests;

/// <summary>
/// Second test stream handler for multiple handler tests.
/// </summary>
public class SecondTestStreamHandler : IStreamRequestHandler<TestStreamRequest, string>
{
    public async IAsyncEnumerable<string> Handle(TestStreamRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return $"Second-{request.Value}";
        await Task.Yield();
    }
}