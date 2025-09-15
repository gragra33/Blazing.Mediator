namespace Blazing.Mediator.Tests;

/// <summary>
/// Base command for testing inheritance scenarios in mediator tests.
/// </summary>
public record BaseCommand : IRequest 
{ 
    /// <summary>
    /// Gets or initializes the base value for the command.
    /// </summary>
    public string BaseValue { get; init; } = string.Empty; 
}