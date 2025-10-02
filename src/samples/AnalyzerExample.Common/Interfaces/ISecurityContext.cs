namespace AnalyzerExample.Common.Interfaces;

public interface ISecurityContext
{
    string? UserId { get; }
    string? UserName { get; }
    IEnumerable<string> Roles { get; }
    bool IsInRole(string role);
}