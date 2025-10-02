namespace AnalyzerExample.Products.Queries;

public class ProductStatsDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public int CurrentStock { get; set; }
    public DateTime LastSaleDate { get; set; }
    public DateTime LastRestockDate { get; set; }
}