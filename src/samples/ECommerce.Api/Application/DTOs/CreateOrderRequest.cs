namespace ECommerce.Api.Application.DTOs;

public class CreateOrderRequest
{
    public int CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public List<OrderItemRequest> Items { get; set; } = [];
}