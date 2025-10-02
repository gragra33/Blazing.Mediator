using Blazing.Mediator;

namespace AnalyzerExample.Users.Commands;

/// <summary>
/// User-specific command interfaces
/// </summary>
public interface IUserCommand : ICommand
{
    int UserId { get; set; }
}