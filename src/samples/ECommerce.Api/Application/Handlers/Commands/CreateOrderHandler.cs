using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Application.Handlers.Commands;

/// <summary>
/// Handler for creating a new order with validation and stock checking.
/// </summary>
/// <param name="context">The database context for accessing order and product data.</param>
/// <param name="validator">The validator for validating the create order command.</param>
public class CreateOrderHandler(ECommerceDbContext context, IValidator<CreateOrderCommand> validator)
    : IRequestHandler<CreateOrderCommand, OperationResult<int>>
{
    /// <summary>
    /// Handles the create order command by validating the request, checking stock availability, and creating the order.
    /// </summary>
    /// <param name="request">The command containing order creation details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result with the created order ID if successful.</returns>
    public async Task<OperationResult<int>> Handle(CreateOrderCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return OperationResult<int>.ErrorResult(
                    "Validation failed",
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            // Check product availability
            List<int> productIds = request.Items.Select(i => i.ProductId).ToList();
            List<Product> products = await context.Products
                .Where(p => productIds.Contains(p.Id) && p.IsActive)
                .ToListAsync(cancellationToken);

            foreach (OrderItemRequest? item in request.Items)
            {
                Product? product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                {
                    return OperationResult<int>.ErrorResult($"Product with ID {item.ProductId} not found or inactive");
                }

                if (!product.HasSufficientStock(item.Quantity))
                {
                    return OperationResult<int>.ErrorResult($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {item.Quantity}");
                }
            }

            // Create order
            Order order = Order.Create(request.CustomerId, request.CustomerEmail, request.ShippingAddress);

            foreach (OrderItemRequest? item in request.Items)
            {
                Product product = products.First(p => p.Id == item.ProductId);
                order.AddItem(item.ProductId, item.Quantity, product.Price);
                product.ReserveStock(item.Quantity);
            }

            context.Orders.Add(order);
            await context.SaveChangesAsync(cancellationToken);

            return OperationResult<int>.SuccessResult(order.Id, "Order created successfully");
        }
        catch (Exception ex)
        {
            return OperationResult<int>.ErrorResult($"Error creating order: {ex.Message}");
        }
    }
}