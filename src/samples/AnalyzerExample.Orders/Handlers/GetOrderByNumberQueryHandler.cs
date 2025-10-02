using AnalyzerExample.Orders.Domain;
using AnalyzerExample.Orders.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for retrieving order by order number
/// </summary>
public class GetOrderByNumberQueryHandler : IRequestHandler<GetOrderByNumberQuery, OrderDetailDto?>
{
    public async Task<OrderDetailDto?> Handle(GetOrderByNumberQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database query
        await Task.Delay(30, cancellationToken);
        
        if (request.OrderNumber == "ORD-001")
        {
            return new OrderDetailDto
            {
                Id = 1,
                OrderNumber = request.OrderNumber,
                UserId = 123,
                Status = OrderStatus.Processing,
                Currency = "USD",
                Notes = "Sample order",
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { Id = 1, ProductId = 1, Quantity = 2 }
                },
                StatusHistory = new List<OrderStatusHistoryDto>()
            };
        }
        
        return null;
    }
}