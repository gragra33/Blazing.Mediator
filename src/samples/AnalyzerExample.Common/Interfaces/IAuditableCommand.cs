using Blazing.Mediator;

namespace AnalyzerExample.Common.Interfaces;

/// <summary>
/// Common domain interfaces for cross-cutting concerns
/// </summary>
public interface IAuditableCommand : ICommand
{
    string? AuditUserId { get; set; }
    string? AuditReason { get; set; }
}