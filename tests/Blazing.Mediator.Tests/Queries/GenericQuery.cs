namespace Blazing.Mediator.Tests;

/// <summary>
/// Generic test query for testing generic type handling in the mediator.
/// Used to verify that generic queries are properly handled and registered.
/// </summary>
/// <typeparam name="T">The type of data contained in the query.</typeparam>
public class GenericQuery<T> : IRequest<string>
{
    /// <summary>
    /// Gets or sets the data payload of the generic query.
    /// </summary>
    public T Data { get; set; } = default!;
}