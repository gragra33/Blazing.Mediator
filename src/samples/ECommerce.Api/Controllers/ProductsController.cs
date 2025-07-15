using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Exceptions;
using ECommerce.Api.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

/// <summary>
/// Controller for managing product operations in the e-commerce system.
/// Implements CQRS pattern using the Blazing.Mediator library.
/// </summary>
/// <param name="mediator">The mediator instance for handling commands and queries.</param>
[ApiController]
[Route("api/[controller]")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>The product details if found.</returns>
    /// <response code="200">Returns the product details.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        GetProductByIdQuery query = new() { ProductId = id };
        ProductDto product = await mediator.Send(query);
        return Ok(product);
    }

    /// <summary>
    /// Retrieves a paginated list of products with optional filtering.
    /// </summary>
    /// <param name="page">The page number (default: 1).</param>
    /// <param name="pageSize">The number of items per page (default: 10).</param>
    /// <param name="searchTerm">Optional search term to filter products.</param>
    /// <param name="inStockOnly">If true, returns only products in stock.</param>
    /// <param name="activeOnly">If true, returns only active products (default: true).</param>
    /// <returns>A paginated list of products.</returns>
    /// <response code="200">Returns the paginated list of products.</response>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = "",
        [FromQuery] bool inStockOnly = false,
        [FromQuery] bool activeOnly = true)
    {
        GetProductsQuery query = new()
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            InStockOnly = inStockOnly,
            ActiveOnly = activeOnly
        };

        PagedResult<ProductDto> result = await mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves products with low stock levels.
    /// </summary>
    /// <param name="threshold">The stock threshold below which products are considered low stock (default: 10).</param>
    /// <returns>A list of products with low stock.</returns>
    /// <response code="200">Returns the list of low stock products.</response>
    [HttpGet("low-stock")]
    public async Task<ActionResult<List<ProductDto>>> GetLowStockProducts([FromQuery] int threshold = 10)
    {
        GetLowStockProductsQuery query = new() { Threshold = threshold };
        List<ProductDto> products = await mediator.Send(query);
        return Ok(products);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="command">The product creation command.</param>
    /// <returns>The ID of the created product.</returns>
    /// <response code="201">Returns the ID of the created product.</response>
    /// <response code="400">If the product data is invalid.</response>
    [HttpPost]
    public async Task<ActionResult<int>> CreateProduct([FromBody] CreateProductCommand command)
    {
        try
        {
            int productId = await mediator.Send(command);
            return CreatedAtAction(nameof(GetProduct), new { id = productId }, productId);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => e.ErrorMessage));
        }
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="command">The product update command.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the product was updated successfully.</response>
    /// <response code="400">If the product data is invalid.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProduct(int id, [FromBody] UpdateProductCommand command)
    {
        try
        {
            command.ProductId = id;
            await mediator.Send(command);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Updates the stock quantity for a product.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="command">The stock update command.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the stock was updated successfully.</response>
    /// <response code="400">If the stock data is invalid.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpPut("{id}/stock")]
    public async Task<ActionResult> UpdateProductStock(int id, [FromBody] UpdateProductStockCommand command)
    {
        try
        {
            command.ProductId = id;
            await mediator.Send(command);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Deactivates a product, making it unavailable for purchase.
    /// </summary>
    /// <param name="id">The unique identifier of the product to deactivate.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the product was deactivated successfully.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpPost("{id}/deactivate")]
    public async Task<ActionResult> DeactivateProduct(int id)
    {
        DeactivateProductCommand command = new() { ProductId = id };
        await mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Reduces product stock to simulate low stock scenarios and trigger notifications.
    /// This demonstrates the inventory management notification system.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="quantity">The amount to reduce stock by (default: 5).</param>
    /// <returns>ActionResult indicating the operation result.</returns>
    /// <response code="200">If the stock was reduced successfully.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpPost("{id}/reduce-stock")]
    public async Task<ActionResult> ReduceStock(int id, [FromQuery] int quantity = 5)
    {
        try
        {
            // Get current stock
            var product = await mediator.Send(new GetProductByIdQuery { ProductId = id });
            if (product == null)
                return NotFound($"Product with ID {id} not found");

            // Calculate new stock (ensure it doesn't go negative)
            var newStock = Math.Max(0, product.StockQuantity - quantity);
            
            // Update stock
            await mediator.Send(new UpdateProductStockCommand 
            { 
                ProductId = id, 
                StockQuantity = newStock 
            });

            return Ok(new { 
                message = $"Stock reduced by {quantity} units", 
                productId = id,
                productName = product.Name,
                previousStock = product.StockQuantity,
                newStock = newStock,
                notificationTrigger = newStock <= 10 ? "Low Stock Notification Sent" : 
                                     newStock == 0 ? "Out of Stock Notification Sent" : "No Notification"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Simulates a bulk order that will trigger multiple stock notifications.
    /// This demonstrates the notification system for inventory management.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="orderQuantity">The quantity to simulate ordering (default: 15).</param>
    /// <returns>ActionResult indicating the operation result.</returns>
    /// <response code="200">If the bulk order simulation was successful.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpPost("{id}/simulate-bulk-order")]
    public async Task<ActionResult> SimulateBulkOrder(int id, [FromQuery] int orderQuantity = 15)
    {
        try
        {
            // Get current product
            var product = await mediator.Send(new GetProductByIdQuery { ProductId = id });
            if (product == null)
                return NotFound($"Product with ID {id} not found");

            // Create a mock order to trigger stock notifications
            var mockOrder = new CreateOrderCommand
            {
                CustomerId = 999,
                CustomerEmail = "demo@notifications.com",
                ShippingAddress = "123 Mock St, Test City, TS 12345",
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        ProductId = id,
                        Quantity = orderQuantity
                    }
                }
            };

            var result = await mediator.Send(mockOrder);
            
            if (result.Success)
            {
                return Ok(new { 
                    message = $"Bulk order simulation completed",
                    orderId = result.Data,
                    productId = id,
                    productName = product.Name,
                    orderQuantity = orderQuantity,
                    notificationsSent = new[] { "Order Created", "Email Confirmation", "Inventory Tracking", "Low Stock Alert (if triggered)" }
                });
            }
            else
            {
                return BadRequest(new { error = "Order simulation failed", message = result.Message });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}