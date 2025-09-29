namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test command that explicitly implements ICommand&lt;T&gt; interface.
/// </summary>
public class TestCommandWithInterface : ICommand<int>
{
    public string? Value { get; set; }
}