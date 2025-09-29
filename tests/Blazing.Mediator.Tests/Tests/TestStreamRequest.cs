namespace Blazing.Mediator.Tests;

/// <summary>
/// Test stream request for testing streaming functionality.
/// </summary>
public class TestStreamRequest : IStreamRequest<string>
{
    public string? Value { get; set; }
}