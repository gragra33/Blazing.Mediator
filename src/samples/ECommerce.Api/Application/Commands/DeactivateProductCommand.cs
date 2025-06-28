using Blazing.Mediator;

namespace ECommerce.Api.Application.Commands;

/// <summary>
/// Command to deactivate a product, making it unavailable for purchase.
/// This is a CQRS command that represents a write operation.
/// </summary>
public class DeactivateProductCommand : IRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the product to deactivate.
    /// </summary>
    public int ProductId { get; set; }
}