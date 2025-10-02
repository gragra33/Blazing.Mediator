namespace AnalyzerExample.Products.Domain;

public class ProductSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool InStock { get; set; }
    public int StockQuantity { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string? PrimaryImageUrl { get; set; }
}