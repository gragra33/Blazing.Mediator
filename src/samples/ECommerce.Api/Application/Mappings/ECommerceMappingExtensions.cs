using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Domain.Entities;

namespace ECommerce.Api.Application.Mappings;

public static class ECommerceMappingExtensions
{
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

    public static List<ProductDto> ToDto(this IEnumerable<Product> products)
    {
        return products.Select(p => p.ToDto()).ToList();
    }

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

    public static List<OrderDto> ToDto(this IEnumerable<Order> orders)
    {
        return orders.Select(o => o.ToDto()).ToList();
    }

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

    public static List<OrderItemDto> ToDto(this IEnumerable<OrderItem> orderItems)
    {
        return orderItems.Select(oi => oi.ToDto()).ToList();
    }

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
