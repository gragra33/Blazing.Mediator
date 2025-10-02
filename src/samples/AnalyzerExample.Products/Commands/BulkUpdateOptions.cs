namespace AnalyzerExample.Products.Commands;

public class BulkUpdateOptions
{
    public string? Category { get; set; }
    public decimal? PriceMultiplier { get; set; }
    public bool? InStock { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
}