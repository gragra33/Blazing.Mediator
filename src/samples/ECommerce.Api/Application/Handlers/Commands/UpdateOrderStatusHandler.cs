using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Infrastructure.Data;

namespace ECommerce.Api.Application.Handlers.Commands;

public class UpdateOrderStatusHandler(ECommerceDbContext context) : IRequestHandler<UpdateOrderStatusCommand>
{
    public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken = default)
    {
        var order = await context.Orders.FindAsync(request.OrderId);
        if (order == null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");

        order.UpdateStatus(request.Status);
        await context.SaveChangesAsync(cancellationToken);
    }
}