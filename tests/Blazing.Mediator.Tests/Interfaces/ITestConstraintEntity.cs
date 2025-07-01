namespace Blazing.Mediator.Tests;

/// <summary>
/// Interface for testing type constraints in generic commands.
/// </summary>
public interface ITestConstraintEntity 
{ 
    /// <summary>
    /// Gets the entity identifier.
    /// </summary>
    int Id { get; } 
    
    /// <summary>
    /// Gets the entity name.
    /// </summary>
    string Name { get; } 
}