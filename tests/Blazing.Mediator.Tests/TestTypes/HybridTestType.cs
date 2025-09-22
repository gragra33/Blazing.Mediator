namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Type that implements both query and command interfaces (edge case).
/// </summary>
public class HybridTestType : IQuery<string>, ICommand<int>
{
    public string QueryData { get; set; } = string.Empty;
    public int CommandData { get; set; }
}