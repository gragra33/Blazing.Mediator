namespace ECommerce.Api.Application.Commands;

/// <summary>
/// Response model for process order operations.
/// </summary>
public class ProcessOrderResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the processed order.
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Gets or sets the order number generated for the processed order.
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total amount of the processed order.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the order was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}