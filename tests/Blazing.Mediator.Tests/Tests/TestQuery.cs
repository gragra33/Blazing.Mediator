namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Statistics test query (temporary name to avoid source generator collision until Phase H folder reorganisation).
/// </summary>
public class TestsTestQuery : IRequest<string>
{
    public string? Value { get; set; }
}