using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Commands;

public class ImportProductsCommand : ICommand<OperationResult<ImportResult>>, IAuditableCommand<OperationResult<ImportResult>>, ITransactionalCommand<OperationResult<ImportResult>>
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public ImportFormat Format { get; set; } = ImportFormat.CSV;
    public bool OverwriteExisting { get; set; } = false;
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
    public bool RequiresTransaction => true;
}