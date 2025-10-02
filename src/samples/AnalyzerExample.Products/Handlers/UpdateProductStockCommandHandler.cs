using AnalyzerExample.Common.Domain;
using AnalyzerExample.Products.Commands;
using AnalyzerExample.Products.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

public class UpdateProductStockCommandHandler : ICommandHandler<UpdateProductStockCommand, OperationResult<int>>
{
    private readonly ILogger<UpdateProductStockCommandHandler> _logger;
    private readonly IMediator _mediator;

    public UpdateProductStockCommandHandler(ILogger<UpdateProductStockCommandHandler> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<OperationResult<int>> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("?? [Products] Updating stock for product {ProductId}: {QuantityChange}", 
            request.ProductId, request.QuantityChange);
        
        // Simulate database update
        await Task.Delay(100, cancellationToken);
        
        var oldQuantity = 100; // Simulated current stock
        var newQuantity = Math.Max(0, oldQuantity + request.QuantityChange);
        
        // Publish stock changed event
        await _mediator.Publish(new ProductStockChangedEvent
        {
            ProductId = request.ProductId,
            ProductName = $"Product {request.ProductId}",
            OldQuantity = oldQuantity,
            NewQuantity = newQuantity,
            QuantityChange = request.QuantityChange,
            Reason = request.Reason,
            ReferenceId = request.ReferenceId
        }, cancellationToken);
        
        // Check for low stock
        if (newQuantity <= 10)
        {
            await _mediator.Publish(new LowStockNotification
            {
                ProductId = request.ProductId,
                ProductName = $"Product {request.ProductId}",
                SKU = $"SKU-{request.ProductId:D6}",
                CurrentStock = newQuantity,
                Threshold = 10,
                Category = "Electronics"
            }, cancellationToken);
        }
        
        return OperationResult<int>.Success(newQuantity);
    }
}