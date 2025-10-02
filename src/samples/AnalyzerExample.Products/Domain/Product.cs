using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Products.Domain;

/// <summary>
/// Product domain models and related entities
/// </summary>
public class Product : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public bool InStock { get; set; }
    public int StockQuantity { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public List<ProductImage> Images { get; set; } = new();
    public List<ProductReview> Reviews { get; set; } = new();
}