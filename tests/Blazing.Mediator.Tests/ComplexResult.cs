namespace Blazing.Mediator.Tests;

/// <summary>
/// Complex result structure used for testing mediator functionality with complex return types.
/// </summary>
public class ComplexResult
{
    /// <summary>
    /// Gets or sets the filtered data result.
    /// </summary>
    public string FilteredData { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the count of items in the result.
    /// </summary>
    public int Count { get; set; }
}