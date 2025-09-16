namespace Blazing.Mediator.Tests.Handlers;

/// <summary>
/// Test streaming request handler for unit testing
/// </summary>
public class TestStreamRequestHandler : IStreamRequestHandler<Commands.TestStreamRequest, string>
{
    public static Commands.TestStreamRequest? LastExecutedRequest;

    public async IAsyncEnumerable<string> Handle(Commands.TestStreamRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LastExecutedRequest = request;

        // Simulate some async work
        await Task.Delay(10, cancellationToken).ConfigureAwait(false);

        // Return test data
        for (int i = 1; i <= 3; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            if (string.IsNullOrWhiteSpace(request.SearchTerm))
                yield return $"Item {i}";
            else
                yield return $"Filtered Item {i} - {request.SearchTerm}";

            // Simulate processing delay
            await Task.Delay(5, cancellationToken).ConfigureAwait(false);
        }
    }
}
