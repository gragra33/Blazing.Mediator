namespace TypedMiddlewareExample.Queries;

/// <summary>
/// Query to get product information by product ID.
/// </summary>
public class GetProductQuery : IQuery<string>
{
    /// <summary>
    /// Gets or sets the product ID to retrieve.
    /// </summary>
    public required string ProductId { get; set; }
}