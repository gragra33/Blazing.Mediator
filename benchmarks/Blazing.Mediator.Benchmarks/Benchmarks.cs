using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Benchmarks;

[DotTraceDiagnoser]
[MemoryDiagnoser]
public class Benchmarks
{
    private IMediator _mediator = null!;
    private readonly Ping _request = new() { Message = "Hello World" };
    private readonly Pinged _notification = new();

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();

        services.AddSingleton(TextWriter.Null);

        services.AddMediator(typeof(Ping).Assembly);

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public Task SendingRequests()
    {
        return _mediator.Send(_request);
    }

    [Benchmark]
    public Task PublishingNotifications()
    {
        return _mediator.Publish(_notification);
    }
}
