using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using Blazing.Mediator;

namespace AnalyzerExample.Users.Commands;

public class BulkUpdateUsersCommand : ICommand<OperationResult<int>>, IAuditableCommand<OperationResult<int>>, ITransactionalCommand<OperationResult<int>>
{
    public List<int> UserIds { get; set; } = new();
    public BulkUserUpdateOptions Updates { get; set; } = new();
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
    public bool RequiresTransaction => true;
}