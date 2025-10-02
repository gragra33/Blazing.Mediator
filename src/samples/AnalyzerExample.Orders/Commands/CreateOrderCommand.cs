using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Orders.Domain;

namespace AnalyzerExample.Orders.Commands;

/// <summary>
/// Order commands demonstrating various patterns
/// </summary>
public class CreateOrderCommand : IAuditableCommand<OperationResult<int>>, ITransactionalCommand<OperationResult<int>>
{
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public List<CreateOrderItem> Items { get; set; } = new();
    public OrderAddressDto ShippingAddress { get; set; } = new();
    public OrderAddressDto BillingAddress { get; set; } = new();
    public string? Notes { get; set; }
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
    public bool RequiresTransaction => true;
}