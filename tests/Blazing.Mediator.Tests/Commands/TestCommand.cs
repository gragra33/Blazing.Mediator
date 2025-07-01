namespace Blazing.Mediator.Tests;

/// <summary>
/// Test command used for unit testing the mediator functionality.
/// Represents a command that doesn't return a value.
/// </summary>
public class TestCommand : IRequest
{
    /// <summary>
    /// Gets or sets the test value for the command.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}