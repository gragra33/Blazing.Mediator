namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test request with lowercase "command" suffix for case-insensitive testing.
/// </summary>
public class TestRequestLowercasecommand : IRequest<int>
{
    public string? Value { get; set; }
}