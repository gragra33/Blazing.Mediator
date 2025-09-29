namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Query that implements IQuery&lt;T&gt; but has "Command" in name to test precedence.
/// </summary>
public class QueryNamedAsCommand : IQuery<string>
{
    public string? Value { get; set; }
}