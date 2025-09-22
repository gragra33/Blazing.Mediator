namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Simple query test type implementing IQuery&lt;T&gt;.
/// </summary>
public class SimpleTestQuery : IQuery<string>
{
    public string SearchTerm { get; set; } = string.Empty;
}