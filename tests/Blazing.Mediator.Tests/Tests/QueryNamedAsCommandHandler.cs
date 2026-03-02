namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for QueryNamedAsCommand.
/// </summary>
public class QueryNamedAsCommandHandler : IRequestHandler<QueryNamedAsCommand, string>
{
    public ValueTask<string> Handle(QueryNamedAsCommand request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"QueryAsCommand: {request.Value}");
    }
}