using AnalyzerExample.Common.Domain;
using AnalyzerExample.Orders.Domain;
using AnalyzerExample.Orders.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for retrieving all orders with pagination
/// </summary>
public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderSummaryDto>>
{
    public async Task<PagedResult<OrderSummaryDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database query
        await Task.Delay(40, cancellationToken);
        
        var orders = new List<OrderSummaryDto>
        {
            new OrderSummaryDto
            {
                Id = 1,
                OrderNumber = "ORD-001",
                Status = OrderStatus.Processing,
                TotalAmount = 150.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new OrderSummaryDto
            {
                Id = 2,
                OrderNumber = "ORD-002", 
                Status = OrderStatus.Processing,
                TotalAmount = 75.50m,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };
        
        return new PagedResult<OrderSummaryDto>
        {
            Items = orders.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList(),
            TotalCount = orders.Count,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}