namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Command test type implementing ICommand&lt;T&gt;.
/// </summary>
public class ReturningTestCommand : ICommand<int>
{
    public string Value { get; set; } = string.Empty;
}