using AnalyzerExample.Common.Domain;

namespace AnalyzerExample.Orders.Domain;

public class OrderBillingAddress : BaseEntity
{
    public int OrderId { get; set; }
    public string BillingName { get; set; } = string.Empty;
    public string Street1 { get; set; } = string.Empty;
    public string? Street2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}