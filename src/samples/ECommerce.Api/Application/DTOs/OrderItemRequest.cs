namespace ECommerce.Api.Application.DTOs;

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}