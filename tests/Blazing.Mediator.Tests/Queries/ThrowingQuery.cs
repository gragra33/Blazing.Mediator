namespace Blazing.Mediator.Tests;

/// <summary>
/// Test query that always throws an exception for testing error handling.
/// Used to verify mediator behavior when query handlers fail.
/// </summary>
public class ThrowingQuery : IRequest<string> { }