using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Commands;

public class CancelOrderCommand : IRequest<OperationResult<bool>>
{
    public int OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
}