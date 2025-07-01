using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Queries;

/// <summary>
/// Query to retrieve a specific order by its unique identifier.
/// </summary>
public class GetOrderByIdQuery : IRequest<OrderDto>
{
    /// <summary>
    /// Gets or sets the unique identifier of the order to retrieve.
    /// </summary>
    public int OrderId { get; set; }
}