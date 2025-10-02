namespace AnalyzerExample.Products.Commands;

public class ImportResult
{
    public int TotalRecords { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<int> ImportedProductIds { get; set; } = new();
}