using AnalyzerExample.Common.Domain;

namespace AnalyzerExample.Users.Commands;

public class VerifyUserEmailCommand : IUserCommand<OperationResult>
{
    public int UserId { get; set; }
    public string VerificationToken { get; set; } = string.Empty;
    public DateTime TokenExpiry { get; set; }
}