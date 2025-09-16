namespace Blazing.Mediator.Tests;

/// <summary>
/// Derived command for testing inheritance scenarios in mediator tests.
/// </summary>
public record DerivedCommand : BaseCommand
{
    /// <summary>
    /// Gets or sets the derived-specific value for the command.
    /// </summary>
    public string DerivedValue { get; init; } = string.Empty;
}