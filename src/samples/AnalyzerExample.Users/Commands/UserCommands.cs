using Blazing.Mediator;

namespace AnalyzerExample.Users.Commands;

public interface IUserCommand<TResponse> : ICommand<TResponse>
{
    int UserId { get; set; }
}