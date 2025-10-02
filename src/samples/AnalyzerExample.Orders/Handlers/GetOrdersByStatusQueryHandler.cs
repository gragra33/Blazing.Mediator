using AnalyzerExample.Orders.Domain;
using AnalyzerExample.Orders.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for retrieving orders by status
/// </summary>
public class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, List<OrderSummaryDto>>
{
    public async Task<List<OrderSummaryDto>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database query
        await Task.Delay(50, cancellationToken);
        
        return new List<OrderSummaryDto>
        {
            new OrderSummaryDto
            {
                Id = 1,
                OrderNumber = "ORD-001",
                Status = request.Status,
                TotalAmount = 150.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new OrderSummaryDto
            {
                Id = 2, 
                OrderNumber = "ORD-002",
                Status = request.Status,
                TotalAmount = 75.50m,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };
    }
}