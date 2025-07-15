using Blazing.Mediator;
using ECommerce.Api.Domain.Entities;

namespace ECommerce.Api.Application.Notifications;

/// <summary>
/// Notification published when an order status changes.
/// This allows services to react to order lifecycle events.
/// </summary>
public class OrderStatusChangedNotification : INotification
{
    /// <summary>
    /// Gets the ID of the order whose status changed.
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
    /// Gets the previous order status.
    /// </summary>
    public OrderStatus PreviousStatus { get; }

    /// <summary>
    /// Gets the new order status.
    /// </summary>
    public OrderStatus NewStatus { get; }

    /// <summary>
    /// Gets the total amount of the order.
    /// </summary>
    public decimal TotalAmount { get; }

    /// <summary>
    /// Gets the timestamp when the status changed.
    /// </summary>
    public DateTime ChangedAt { get; }

    /// <summary>
    /// Initializes a new instance of the OrderStatusChangedNotification.
    /// </summary>
    /// <param name="orderId">The ID of the order whose status changed.</param>
    /// <param name="customerId">The customer ID who placed the order.</param>
    /// <param name="customerEmail">The customer's email address.</param>
    /// <param name="previousStatus">The previous order status.</param>
    /// <param name="newStatus">The new order status.</param>
    /// <param name="totalAmount">The total amount of the order.</param>
    public OrderStatusChangedNotification(int orderId, int customerId, string customerEmail, OrderStatus previousStatus, OrderStatus newStatus, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        CustomerEmail = customerEmail;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        TotalAmount = totalAmount;
        ChangedAt = DateTime.UtcNow;
    }
}
