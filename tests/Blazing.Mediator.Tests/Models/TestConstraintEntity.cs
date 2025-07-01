namespace Blazing.Mediator.Tests;

/// <summary>
/// Test entity implementation for constraint testing.
/// </summary>
public class TestConstraintEntity : ITestConstraintEntity 
{ 
    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    public int Id { get; set; } 
    
    /// <summary>
    /// Gets or sets the entity name.
    /// </summary>
    public string Name { get; set; } = string.Empty; 
}