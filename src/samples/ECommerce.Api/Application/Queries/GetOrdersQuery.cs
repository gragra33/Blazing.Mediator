using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Domain.Entities;

namespace ECommerce.Api.Application.Queries;

/// <summary>
/// Query to retrieve a paginated list of orders with optional filtering.
/// </summary>
public class GetOrdersQuery : IRequest<PagedResult<OrderDto>>
{
    /// <summary>
    /// Gets or sets the page number for pagination (default: 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of items per page (default: 10).
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the customer ID to filter orders by specific customer.
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the order status to filter orders by status.
    /// </summary>
    public OrderStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the start date to filter orders from this date onwards.
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Gets or sets the end date to filter orders up to this date.
    /// </summary>
    public DateTime? ToDate { get; set; }
}