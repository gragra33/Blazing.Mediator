namespace Blazing.Mediator.Tests;

/// <summary>
/// Test command that returns null to test null handling behavior.
/// Used to verify mediator behavior when command handlers return null task values.
/// </summary>
public class TestNullCommand : IRequest
{
}