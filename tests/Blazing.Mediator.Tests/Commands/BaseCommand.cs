namespace Blazing.Mediator.Tests;

/// <summary>
/// Base command for testing inheritance scenarios.
/// </summary>
public record BaseCommand : IRequest 
{ 
    /// <summary>
    /// Gets or initializes the base value.
    /// </summary>
    public string BaseValue { get; init; } = string.Empty; 
}