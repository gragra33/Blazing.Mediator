using AnalyzerExample.Common.Domain;
using AnalyzerExample.Products.Commands;
using AnalyzerExample.Products.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Product command handlers
/// </summary>
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OperationResult<int>>
{
    private readonly ILogger<CreateProductCommandHandler> _logger;
    private readonly IMediator _mediator;

    public CreateProductCommandHandler(ILogger<CreateProductCommandHandler> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<OperationResult<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("?? [Products] Creating product: {ProductName}", request.Name);
        
        // Simulate validation
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return OperationResult<int>.Failure("Product name is required");
        }

        if (request.Price <= 0)
        {
            return OperationResult<int>.Failure("Product price must be greater than zero");
        }

        // Simulate database save
        await Task.Delay(200, cancellationToken);
        
        var productId = Random.Shared.Next(1000, 9999);
        
        // Publish domain event
        await _mediator.Publish(new ProductCreatedEvent
        {
            ProductId = productId,
            ProductName = request.Name,
            SKU = request.SKU,
            Price = request.Price,
            Category = request.Category,
            CreatedBy = request.AuditUserId ?? "System"
        }, cancellationToken);
        
        _logger.LogInformation("?? [Products] Product created with ID: {ProductId}", productId);
        
        return OperationResult<int>.Success(productId);
    }
}