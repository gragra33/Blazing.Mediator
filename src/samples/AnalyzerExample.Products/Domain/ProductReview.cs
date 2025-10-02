using AnalyzerExample.Common.Domain;

namespace AnalyzerExample.Products.Domain;

public class ProductReview : BaseEntity, IAuditableEntity
{
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5 stars
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsVerifiedPurchase { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}