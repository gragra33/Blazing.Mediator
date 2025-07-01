namespace Blazing.Mediator.Tests;

/// <summary>
/// Test query that returns null to test null handling behavior.
/// Used to verify mediator behavior when query handlers return null values.
/// </summary>
public class TestNullQuery : IRequest<string>
{
}