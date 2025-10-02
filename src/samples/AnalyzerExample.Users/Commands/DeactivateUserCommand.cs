using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Users.Commands;

public class DeactivateUserCommand : IUserCommand, IAuditableCommand, ITransactionalCommand
{
    public int UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? ReactivationDate { get; set; }
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
    public bool RequiresTransaction => true;
}