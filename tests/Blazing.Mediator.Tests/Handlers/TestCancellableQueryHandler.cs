namespace Blazing.Mediator.Tests;

/// <summary>
/// Test query handler for testing cancellation token handling.
/// Used to verify that query handlers properly respond to cancellation requests.
/// </summary>
public class TestCancellableQueryHandler : IRequestHandler<TestCancellableQuery, string>
{
    /// <summary>
    /// Gets the last cancellation token that was passed to the handler.
    /// Used for testing purposes to verify the token was properly passed.
    /// </summary>
    public static CancellationToken LastCancellationToken { get; private set; }
    /// <summary>
    /// Handles the test cancellable query and checks for cancellation.
    /// </summary>
    /// <param name="request">The test cancellable query to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task containing the string result after a delay.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public async Task<string> Handle(TestCancellableQuery request, CancellationToken cancellationToken = default)
    {
        LastCancellationToken = cancellationToken;
        await Task.Delay(1000, cancellationToken);
        return "Cancellable result";
    }
}