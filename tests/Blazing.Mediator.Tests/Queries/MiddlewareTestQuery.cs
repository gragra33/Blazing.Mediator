using Blazing.Mediator;

/// <summary>
/// Test query for middleware testing.
/// </summary>
public record MiddlewareTestQuery : IRequest<string> 
{ 
    /// <summary>
    /// Gets or initializes the query value.
    /// </summary>
    public string Value { get; init; } = string.Empty; 
}