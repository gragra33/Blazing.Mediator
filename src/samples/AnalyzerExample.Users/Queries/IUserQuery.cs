using Blazing.Mediator;

namespace AnalyzerExample.Users.Queries;

/// <summary>
/// User-specific query interfaces
/// </summary>
public interface IUserQuery<TResponse> : IQuery<TResponse>
{
    bool IncludeInactive { get; set; }
}