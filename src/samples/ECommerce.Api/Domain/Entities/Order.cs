namespace ECommerce.Api.Domain.Entities;

/// <summary>
/// Represents an order in the e-commerce system.
/// </summary>
public class Order
{
    /// <summary>
    /// Gets or sets the unique identifier for the order.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Gets or sets the customer email address.
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the shipping address for the order.
    /// </summary>
    public string ShippingAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the current status of the order.
    /// </summary>
    public OrderStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the total amount of the order.
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the order was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the order was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the collection of items in this order.
    /// </summary>
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    /// <summary>
    /// Creates a new order with the specified customer details.
    /// </summary>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="customerEmail">The customer email address.</param>
    /// <param name="shippingAddress">The shipping address.</param>
    /// <returns>A new order instance.</returns>
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

    /// <summary>
    /// Adds an item to the order or updates the quantity if the item already exists.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="quantity">The quantity of the product.</param>
    /// <param name="unitPrice">The unit price of the product.</param>
    public void AddItem(int productId, int quantity, decimal unitPrice)
    {
        OrderItem? existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
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

    /// <summary>
    /// Updates the status of the order.
    /// </summary>
    /// <param name="status">The new order status.</param>
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