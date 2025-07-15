using Blazing.Mediator;

namespace ECommerce.Api.Application.Notifications;

/// <summary>
/// Notification published when a new order is created in the e-commerce system.
/// This allows multiple services to react to order creation events.
/// </summary>
public class OrderCreatedNotification : INotification
{
    /// <summary>
    /// Gets the ID of the newly created order.
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// Gets the customer ID who placed the order.
    /// </summary>
    public int CustomerId { get; }

    /// <summary>
    /// Gets the customer's email address.
    /// </summary>
    public string CustomerEmail { get; }

    /// <summary>
    /// Gets the total amount of the order.
    /// </summary>
    public decimal TotalAmount { get; }

    /// <summary>
    /// Gets the items in the order.
    /// </summary>
    public List<OrderItemNotification> Items { get; }

    /// <summary>
    /// Gets the timestamp when the order was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the OrderCreatedNotification.
    /// </summary>
    /// <param name="orderId">The ID of the newly created order.</param>
    /// <param name="customerId">The customer ID who placed the order.</param>
    /// <param name="customerEmail">The customer's email address.</param>
    /// <param name="totalAmount">The total amount of the order.</param>
    /// <param name="items">The items in the order.</param>
    public OrderCreatedNotification(int orderId, int customerId, string customerEmail, decimal totalAmount, List<OrderItemNotification> items)
    {
        OrderId = orderId;
        CustomerId = customerId;
        CustomerEmail = customerEmail;
        TotalAmount = totalAmount;
        Items = items;
        CreatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Represents an order item in the notification.
/// </summary>
public class OrderItemNotification
{
    /// <summary>
    /// Gets the product ID.
    /// </summary>
    public int ProductId { get; }

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public string ProductName { get; }

    /// <summary>
    /// Gets the quantity ordered.
    /// </summary>
    public int Quantity { get; }

    /// <summary>
    /// Gets the unit price.
    /// </summary>
    public decimal UnitPrice { get; }

    /// <summary>
    /// Initializes a new instance of the OrderItemNotification.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="productName">The product name.</param>
    /// <param name="quantity">The quantity ordered.</param>
    /// <param name="unitPrice">The unit price.</param>
    public OrderItemNotification(int productId, string productName, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
