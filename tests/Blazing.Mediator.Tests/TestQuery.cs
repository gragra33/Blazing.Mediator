namespace Blazing.Mediator.Tests;

/// <summary>
/// Test query used for unit testing the mediator functionality.
/// Represents a query that returns a string value.
/// </summary>
public class TestQuery : IRequest<string>
{
    /// <summary>
    /// Gets or sets the test value for the query.
    /// </summary>
    public int Value { get; set; }
}