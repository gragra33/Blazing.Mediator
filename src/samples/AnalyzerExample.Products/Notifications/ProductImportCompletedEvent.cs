using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Products.Notifications;

public class ProductImportCompletedEvent : IIntegrationEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source => "ProductService";
    public string ImportId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public List<int> ImportedProductIds { get; set; } = new();
    public string ImportedBy { get; set; } = string.Empty;
}