using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Users.Commands;

public class ResetUserPasswordCommand : IUserCommand<OperationResult>, IAuditableCommand<OperationResult>
{
    public int UserId { get; set; }
    public string NewPasswordHash { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public DateTime TokenExpiry { get; set; }
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
}