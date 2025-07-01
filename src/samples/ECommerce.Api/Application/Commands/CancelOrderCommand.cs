using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Commands;

/// <summary>
/// Command to cancel an existing order.
/// </summary>
public class CancelOrderCommand : IRequest<OperationResult<bool>>
{
    /// <summary>
    /// Gets or sets the identifier of the order to cancel.
    /// </summary>
    public int OrderId { get; set; }
    
    /// <summary>
    /// Gets or sets the reason for cancelling the order.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}