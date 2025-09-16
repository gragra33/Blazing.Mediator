using Blazing.Mediator;
using ECommerce.Api.Domain.Entities;

namespace ECommerce.Api.Application.Commands;

/// <summary>
/// Command to update the status of an existing order.
/// </summary>
public class UpdateOrderStatusCommand : IRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the order to update.
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Gets or sets the new status for the order.
    /// </summary>
    public OrderStatus Status { get; set; }
}