using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Queries;

/// <summary>
/// Query to retrieve all orders for a specific customer with optional date filtering.
/// </summary>
public class GetCustomerOrdersQuery : IRequest<List<OrderDto>>
{
    /// <summary>
    /// Gets or sets the unique identifier of the customer whose orders to retrieve.
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Gets or sets the start date to filter orders from this date onwards.
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// Gets or sets the end date to filter orders up to this date.
    /// </summary>
    public DateTime? ToDate { get; set; }
}