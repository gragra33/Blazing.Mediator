namespace Blazing.Mediator.Tests;

/// <summary>
/// Test query for testing cancellation token handling.
/// Used to verify that query handlers properly respond to cancellation requests.
/// </summary>
public class TestCancellableQuery : IRequest<string>
{
}