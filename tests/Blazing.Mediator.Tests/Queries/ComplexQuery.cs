namespace Blazing.Mediator.Tests;

/// <summary>
/// Test query with complex parameters used for testing the mediator functionality with complex return types.
/// </summary>
public class ComplexQuery : IRequest<ComplexResult>
{
    /// <summary>
    /// Gets or sets the filter criteria for the query.
    /// </summary>
    public string Filter { get; set; } = string.Empty;
}