namespace ECommerce.Api.Domain.Entities;

/// <summary>
/// Represents the status of an order in the system.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order is pending confirmation.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Order has been confirmed.
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Order is being processed.
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Order has been shipped.
    /// </summary>
    Shipped = 3,

    /// <summary>
    /// Order has been delivered.
    /// </summary>
    Delivered = 4,

    /// <summary>
    /// Order has been cancelled.
    /// </summary>
    Cancelled = 5
}