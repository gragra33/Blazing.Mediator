namespace Blazing.Mediator.Benchmarks;

public class Ping : IRequest
{
    public string Message { get; set; } = string.Empty;
}

public class PingHandler : IRequestHandler<Ping>
{
    public ValueTask Handle(Ping request, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}