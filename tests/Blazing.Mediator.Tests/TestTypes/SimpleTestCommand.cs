namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Simple command test type implementing ICommand.
/// </summary>
public class SimpleTestCommand : ICommand
{
    public string Data { get; set; } = string.Empty;
}