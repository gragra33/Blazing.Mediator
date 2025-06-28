using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Queries;

/// <summary>
/// Query to retrieve a product by its unique identifier.
/// This is a CQRS query that represents a read operation.
/// </summary>
// Product Queries
public class GetProductByIdQuery : IRequest<ProductDto>
{
    /// <summary>
    /// Gets or sets the unique identifier of the product to retrieve.
    /// </summary>
    public int ProductId { get; set; }
}

// Order Queries