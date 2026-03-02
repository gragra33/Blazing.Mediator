namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for QueryNamedAsCommand.
/// </summary>
public class QueryNamedAsCommandHandler : IRequestHandler<QueryNamedAsCommand, string>
{
    public async ValueTask<string> Handle(QueryNamedAsCommand request, CancellationToken cancellationToken)
    {
        return $"QueryAsCommand: {request.Value}";
    }
}