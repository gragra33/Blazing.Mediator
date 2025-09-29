namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test request with "Query" suffix for name-based detection.
/// </summary>
public class TestRequestNamedQuery : IRequest<string>
{
    public string? Value { get; set; }
}