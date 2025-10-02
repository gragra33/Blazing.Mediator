using Blazing.Mediator;

namespace AnalyzerExample.Common.Interfaces;

public interface IAuditableCommand<out TResponse> : ICommand<TResponse>
{
    string? AuditUserId { get; set; }
    string? AuditReason { get; set; }
}

public interface ITransactionalCommand<out TResponse> : ICommand<TResponse>
{
    bool RequiresTransaction { get; }
}