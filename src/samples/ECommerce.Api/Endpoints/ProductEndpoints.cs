using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Exceptions;
using ECommerce.Api.Application.Queries;

namespace ECommerce.Api.Endpoints;

/// <summary>
/// Handles product endpoints following single responsibility principle.
/// </summary>
public static class ProductEndpoints
{
    /// <summary>
    /// Maps product endpoints to the route group.
    /// </summary>
    /// <param name="group">The route group builder.</param>
    /// <returns>The route group builder for method chaining.</returns>
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
    {
        group.MapGetProductById();
        group.MapGetProducts();
        group.MapGetLowStockProducts();
        group.MapCreateProduct();
        group.MapUpdateProduct();
        group.MapDeactivateProduct();
        group.MapUpdateProductStock();

        return group;
    }

    private static void MapGetProductById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:int}", async (int id, IMediator mediator) =>
            {
                var query = new GetProductByIdQuery { ProductId = id };
                var product = await mediator.Send(query);
                return Results.Ok(product);
            })
            .WithName("GetProduct")
            .WithSummary("Get product by ID")
            .WithDescription("Retrieves a product by its unique identifier")
            .Produces<ProductDto>()
            .Produces(404);
    }

    private static void MapGetProducts(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
                IMediator mediator,
                int page = 1,
                int pageSize = 10,
                string searchTerm = "",
                bool inStockOnly = false,
                bool activeOnly = true) =>
            {
                var query = new GetProductsQuery
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    InStockOnly = inStockOnly,
                    ActiveOnly = activeOnly
                };

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetProducts")
            .WithSummary("Get paginated products")
            .WithDescription("Retrieves a paginated list of products with optional filtering")
            .Produces<PagedResult<ProductDto>>();
    }

    private static void MapGetLowStockProducts(this RouteGroupBuilder group)
    {
        group.MapGet("/low-stock", async (IMediator mediator, int threshold = 10) =>
            {
                var query = new GetLowStockProductsQuery { Threshold = threshold };
                var products = await mediator.Send(query);
                return Results.Ok(products);
            })
            .WithName("GetLowStockProducts")
            .WithSummary("Get products with low stock")
            .WithDescription("Retrieves products with low stock levels")
            .Produces<List<ProductDto>>();
    }

    private static void MapCreateProduct(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateProductCommand command, IMediator mediator) =>
            {
                try
                {
                    var productId = await mediator.Send(command);
                    return Results.Created($"/api/products/{productId}", productId);
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(ex.Errors.Select(e => e.ErrorMessage));
                }
            })
            .WithName("CreateProduct")
            .WithSummary("Create product")
            .WithDescription("Creates a new product")
            .Accepts<CreateProductCommand>("application/json")
            .Produces<int>(201)
            .Produces(400);
    }

    private static void MapUpdateProduct(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:int}", async (int id, UpdateProductCommand command, IMediator mediator) =>
            {
                try
                {
                    command.ProductId = id;
                    await mediator.Send(command);
                    return Results.NoContent();
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(ex.Errors.Select(e => e.ErrorMessage));
                }
                catch (InvalidOperationException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            })
            .WithName("UpdateProduct")
            .WithSummary("Update product")
            .WithDescription("Updates an existing product")
            .Accepts<UpdateProductCommand>("application/json")
            .Produces(204)
            .Produces(400)
            .Produces(404);
    }

    private static void MapDeactivateProduct(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:int}/deactivate", async (int id, IMediator mediator) =>
            {
                var command = new DeactivateProductCommand { ProductId = id };
                await mediator.Send(command);
                return Results.NoContent();
            })
            .WithName("DeactivateProduct")
            .WithSummary("Deactivate product")
            .WithDescription("Deactivates a product, making it unavailable for purchase")
            .Produces(204)
            .Produces(404);
    }

    private static void MapUpdateProductStock(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:int}/stock", async (int id, UpdateProductStockCommand command, IMediator mediator) =>
            {
                try
                {
                    command.ProductId = id;
                    await mediator.Send(command);
                    return Results.NoContent();
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(ex.Errors.Select(e => e.ErrorMessage));
                }
                catch (InvalidOperationException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            })
            .WithName("UpdateProductStock")
            .WithSummary("Update product stock")
            .WithDescription("Updates the stock quantity of a product")
            .Accepts<UpdateProductStockCommand>("application/json")
            .Produces(204)
            .Produces(400)
            .Produces(404);
    }
}
