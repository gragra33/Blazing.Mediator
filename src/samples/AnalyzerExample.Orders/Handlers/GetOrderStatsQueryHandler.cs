using AnalyzerExample.Orders.Domain;
using AnalyzerExample.Orders.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for retrieving order statistics
/// </summary>
public class GetOrderStatsQueryHandler : IRequestHandler<GetOrderStatsQuery, OrderStatsDto>
{
    public async Task<OrderStatsDto> Handle(GetOrderStatsQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database query
        await Task.Delay(60, cancellationToken);
        
        // Return a sample order statistics entry
        return new OrderStatsDto
        {
            OrderId = 1,
            OrderNumber = "ORD-001",
            TotalAmount = 150.00m,
            ItemCount = 3,
            CurrentStatus = OrderStatus.Processing,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            ShippedAt = DateTime.UtcNow.AddDays(-2),
            DaysToShip = 3,
            DaysToDeliver = 0,
            StatusTransitions = new List<OrderStatusHistoryDto>
            {
                new OrderStatusHistoryDto
                {
                    Id = 1,
                    FromStatus = OrderStatus.Pending,
                    ToStatus = OrderStatus.Processing,
                    ChangedAt = DateTime.UtcNow.AddDays(-5),
                    ChangedBy = "System"
                },
                new OrderStatusHistoryDto
                {
                    Id = 2,
                    FromStatus = OrderStatus.Processing,
                    ToStatus = OrderStatus.Shipped,
                    ChangedAt = DateTime.UtcNow.AddDays(-2),
                    ChangedBy = "Admin"
                }
            }
        };
    }
}