namespace Blazing.Mediator.Tests;

/// <summary>
/// Test stream handler.
/// </summary>
public class TestStreamHandler : IStreamRequestHandler<TestStreamRequest, string>
{
    public async IAsyncEnumerable<string> Handle(TestStreamRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int i = 1; i <= 3; i++)
        {
            yield return $"{request.Value}-{i}";
            await Task.Yield();
        }
    }
}