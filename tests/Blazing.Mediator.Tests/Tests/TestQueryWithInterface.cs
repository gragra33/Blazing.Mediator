namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test query that explicitly implements IQuery&lt;T&gt; interface.
/// </summary>
public class TestQueryWithInterface : IQuery<string>
{
    public string? Value { get; set; }
}