using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Commands;

public class AddProductReviewCommand : ICommand<OperationResult<int>>, IAuditableCommand<OperationResult<int>>
{
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsVerifiedPurchase { get; set; }
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
}