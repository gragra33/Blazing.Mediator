using AnalyzerExample.Common.Domain;

namespace AnalyzerExample.Users.Domain;

public class UserAddress : BaseEntity, IAuditableEntity
{
    public int UserId { get; set; }
    public AddressType Type { get; set; }
    public string Street1 { get; set; } = string.Empty;
    public string? Street2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}