namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test command for testing purposes.
/// </summary>
public class TestCommand : IRequest
{
    public string? Value { get; set; }
}