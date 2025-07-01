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
}