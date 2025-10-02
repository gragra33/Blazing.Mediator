using AnalyzerExample.Common.Domain;
using AnalyzerExample.Orders.Domain;
using AnalyzerExample.Orders.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for searching orders with various criteria
/// </summary>
public class SearchOrdersQueryHandler : IRequestHandler<SearchOrdersQuery, PagedResult<OrderSummaryDto>>
{
    public async Task<PagedResult<OrderSummaryDto>> Handle(SearchOrdersQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database search
        await Task.Delay(80, cancellationToken);
        
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
            Items = orders,
            TotalCount = orders.Count,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}