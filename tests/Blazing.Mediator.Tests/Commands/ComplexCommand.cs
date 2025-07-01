namespace Blazing.Mediator.Tests;

/// <summary>
/// Test command with complex data used for testing the mediator with complex object scenarios.
/// </summary>
public class ComplexCommand : IRequest
{
    /// <summary>
    /// Gets or sets the complex data associated with this command.
    /// </summary>
    public ComplexData Data { get; set; } = new();
}