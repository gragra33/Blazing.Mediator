using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Queries;

/// <summary>
/// Query to retrieve order statistics for a specific date range.
/// </summary>
public class GetOrderStatisticsQuery : IRequest<OrderStatisticsDto>
{
    /// <summary>
    /// Gets or sets the start date for statistics calculation.
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// Gets or sets the end date for statistics calculation.
    /// </summary>
    public DateTime? ToDate { get; set; }
}