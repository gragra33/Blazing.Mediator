namespace Blazing.Mediator.Tests;

/// <summary>
/// Test command that always throws an exception for testing error handling.
/// Used to verify mediator behavior when command handlers fail.
/// </summary>
public class ThrowingCommand : IRequest { }