namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test request with "Command" suffix for name-based detection.
/// </summary>
public class TestRequestNamedCommand : IRequest<bool>
{
    public string? Value { get; set; }
}