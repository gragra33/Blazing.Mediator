namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test request with lowercase "query" suffix for case-insensitive testing.
/// </summary>
public class TestRequestLowercasequery : IRequest<string>
{
    public string? Value { get; set; }
}