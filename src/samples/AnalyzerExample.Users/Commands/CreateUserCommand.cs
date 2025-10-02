using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Users.Commands;

/// <summary>
/// User commands demonstrating various patterns
/// </summary>
public class CreateUserCommand : IAuditableCommand<OperationResult<int>>, ITransactionalCommand<OperationResult<int>>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? InitialRole { get; set; }
    public string? AuditUserId { get; set; }
    public string? AuditReason { get; set; }
    public bool RequiresTransaction => true;
}