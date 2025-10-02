using AnalyzerExample.Common.Domain;
using AnalyzerExample.Orders.Domain;
using AnalyzerExample.Orders.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for retrieving user orders
/// </summary>
public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, PagedResult<OrderSummaryDto>>
{
    public async Task<PagedResult<OrderSummaryDto>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database query
        await Task.Delay(35, cancellationToken);
        
        var orders = new List<OrderSummaryDto>
        {
            new OrderSummaryDto
            {
                Id = 1,
                OrderNumber = "ORD-001",
                Status = OrderStatus.Processing,
                TotalAmount = 150.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
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