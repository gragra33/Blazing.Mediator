using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Domain.Entities;

namespace ECommerce.Api.Application.Mappings;

/// <summary>
/// Extension methods for mapping between domain entities and DTOs in the e-commerce application.
/// </summary>
public static class ECommerceMappingExtensions
{
    /// <summary>
    /// Converts a Product entity to a ProductDto.
    /// </summary>
    /// <param name="product">The product entity to convert.</param>
    /// <returns>A ProductDto containing the product information.</returns>
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };
    }

    /// <summary>
    /// Converts a collection of Product entities to a list of ProductDto objects.
    /// </summary>
    /// <param name="products">The collection of product entities to convert.</param>
    /// <returns>A list of ProductDto objects.</returns>
    public static List<ProductDto> ToDto(this IEnumerable<Product> products)
    {
        return products.Select(p => p.ToDto()).ToList();
    }

    /// <summary>
    /// Converts an Order entity to an OrderDto.
    /// </summary>
    /// <param name="order">The order entity to convert.</param>
    /// <returns>An OrderDto containing the order information with its items.</returns>
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerEmail = order.CustomerEmail,
            ShippingAddress = order.ShippingAddress,
            Status = order.Status,
            StatusName = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.Items.ToDto()
        };
    }

    /// <summary>
    /// Converts a collection of Order entities to a list of OrderDto objects.
    /// </summary>
    /// <param name="orders">The collection of order entities to convert.</param>
    /// <returns>A list of OrderDto objects.</returns>
    public static List<OrderDto> ToDto(this IEnumerable<Order> orders)
    {
        return orders.Select(o => o.ToDto()).ToList();
    }

    /// <summary>
    /// Converts an OrderItem entity to an OrderItemDto.
    /// </summary>
    /// <param name="orderItem">The order item entity to convert.</param>
    /// <returns>An OrderItemDto containing the order item information.</returns>
    public static OrderItemDto ToDto(this OrderItem orderItem)
    {
        return new OrderItemDto
        {
            Id = orderItem.Id,
            ProductId = orderItem.ProductId,
            ProductName = orderItem.Product?.Name ?? string.Empty,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.UnitPrice,
            TotalPrice = orderItem.TotalPrice
        };
    }

    /// <summary>
    /// Converts a collection of OrderItem entities to a list of OrderItemDto objects.
    /// </summary>
    /// <param name="orderItems">The collection of order item entities to convert.</param>
    /// <returns>A list of OrderItemDto objects.</returns>
    public static List<OrderItemDto> ToDto(this IEnumerable<OrderItem> orderItems)
    {
        return orderItems.Select(oi => oi.ToDto()).ToList();
    }

    /// <summary>
    /// Converts a collection of Product entities to a paginated ProductDto result.
    /// </summary>
    /// <param name="products">The collection of product entities to convert.</param>
    /// <param name="totalCount">The total count of items across all pages.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated result containing ProductDto objects.</returns>
    public static PagedResult<ProductDto> ToPagedDto(this IEnumerable<Product> products, int totalCount, int page, int pageSize)
    {
        return new PagedResult<ProductDto>
        {
            Items = products.ToDto(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Converts a collection of Order entities to a paginated OrderDto result.
    /// </summary>
    /// <param name="orders">The collection of order entities to convert.</param>
    /// <param name="totalCount">The total count of items across all pages.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated result containing OrderDto objects.</returns>
    public static PagedResult<OrderDto> ToPagedDto(this IEnumerable<Order> orders, int totalCount, int page, int pageSize)
    {
        return new PagedResult<OrderDto>
        {
            Items = orders.ToDto(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
