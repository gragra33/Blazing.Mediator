using AnalyzerExample.Common.Domain;

namespace AnalyzerExample.Products.Domain;

public class ProductCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public List<Product> Products { get; set; } = new();
}