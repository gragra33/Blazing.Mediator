namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test query for testing purposes.
/// </summary>
public class TestQuery : IRequest<string>
{
    public string? Value { get; set; }
}