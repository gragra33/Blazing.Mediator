using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Users.Commands;

public class UpdateUserCommand : IUserCommand<OperationResult>, IAuditableCommand<OperationResult>, ITransactionalCommand<OperationResult>
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
    public bool RequiresTransaction => true;
}