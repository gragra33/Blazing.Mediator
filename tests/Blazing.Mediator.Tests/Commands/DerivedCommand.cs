namespace Blazing.Mediator.Tests;

/// <summary>
/// Derived command for testing inheritance scenarios.
/// </summary>
public record DerivedCommand : BaseCommand 
{ 
    /// <summary>
    /// Gets or initializes the derived value.
    /// </summary>
    public string DerivedValue { get; init; } = string.Empty; 
}