namespace TypedMiddlewareExample.Queries;

/// <summary>
/// Query to get product information by product ID.
/// Uses custom IProductRequest interface to demonstrate type constraints.
/// </summary>
public class GetProductQuery : IProductRequest<string>
{
    /// <summary>
    /// Gets or sets the product ID to retrieve.
    /// </summary>
    public required string ProductId { get; set; }
}