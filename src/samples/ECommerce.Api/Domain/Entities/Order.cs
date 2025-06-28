namespace ECommerce.Api.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    // Domain methods
    public static Order Create(int customerId, string customerEmail, string shippingAddress)
    {
        return new Order
        {
            CustomerId = customerId,
            CustomerEmail = customerEmail,
            ShippingAddress = shippingAddress,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };
    }

    public void AddItem(int productId, int quantity, decimal unitPrice)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            Items.Add(new OrderItem
            {
                OrderId = Id,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            });
        }

        CalculateTotal();
    }

    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    private void CalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.Quantity * i.UnitPrice);
        UpdatedAt = DateTime.UtcNow;
    }
}