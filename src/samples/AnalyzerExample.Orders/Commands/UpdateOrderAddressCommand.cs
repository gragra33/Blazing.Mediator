using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Orders.Domain;

namespace AnalyzerExample.Orders.Commands;

public class UpdateOrderAddressCommand : IOrderCommand<OperationResult>, IAuditableCommand<OperationResult>
{
    public int OrderId { get; set; }
    public OrderAddressDto? ShippingAddress { get; set; }
    public OrderAddressDto? BillingAddress { get; set; }
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
}