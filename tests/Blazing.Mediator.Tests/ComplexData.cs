namespace Blazing.Mediator.Tests;

/// <summary>
/// Complex data structure used for testing mediator functionality with complex objects.
/// </summary>
public class ComplexData
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the data.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the array of items associated with this data.
    /// </summary>
    public string[] Items { get; set; } = [];
}