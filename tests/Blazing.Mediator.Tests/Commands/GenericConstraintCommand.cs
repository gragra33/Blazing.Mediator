namespace Blazing.Mediator.Tests;

/// <summary>
/// Generic command with type constraints for testing constraint validation.
/// Used to verify that generic commands with type constraints are handled correctly.
/// </summary>
/// <typeparam name="T">The type parameter that must be a class implementing ITestConstraintEntity.</typeparam>
public record GenericConstraintCommand<T> : IRequest where T : class, ITestConstraintEntity 
{ 
    /// <summary>
    /// Gets or initializes the data of type T.
    /// </summary>
    public T Data { get; init; } = default!; 
}