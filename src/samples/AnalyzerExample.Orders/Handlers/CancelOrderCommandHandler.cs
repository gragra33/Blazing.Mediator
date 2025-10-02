using AnalyzerExample.Orders.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for cancelling orders
/// </summary>
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate order cancellation
        await Task.Delay(70, cancellationToken);
        
        // Order cancelled successfully
    }
}