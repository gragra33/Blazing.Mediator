namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for QueryNamedAsCommand.
/// </summary>
public class QueryNamedAsCommandHandler : IRequestHandler<QueryNamedAsCommand, string>
{
    public Task<string> Handle(QueryNamedAsCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"QueryAsCommand: {request.Value}");
    }
}