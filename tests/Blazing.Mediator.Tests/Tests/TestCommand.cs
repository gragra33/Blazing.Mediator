namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Statistics test command (temporary name to avoid source generator collision until Phase H folder reorganisation).
/// </summary>
public class TestsTestCommand : IRequest
{
    public string? Value { get; set; }
}