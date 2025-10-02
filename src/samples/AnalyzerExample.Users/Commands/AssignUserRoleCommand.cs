using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Users.Commands;

public class AssignUserRoleCommand : IUserCommand<OperationResult>, IAuditableCommand<OperationResult>
{
    public int UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
}