using Blazing.Mediator;

/// <summary>
/// Conditional query for testing middleware execution conditions.
/// </summary>
public record ConditionalQuery : IRequest<string>
{ 
    /// <summary>
    /// Gets or initializes a value indicating whether middleware should execute.
    /// </summary>
    public bool ShouldExecuteMiddleware { get; init; }
    
    /// <summary>
    /// Gets or initializes the query value.
    /// </summary>
    public string Value { get; init; } = string.Empty;
}