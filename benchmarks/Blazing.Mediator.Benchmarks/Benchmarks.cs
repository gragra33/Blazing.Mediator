using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Blazing.Mediator.Configuration;

namespace Blazing.Mediator.Benchmarks;

[DotTraceDiagnoser]
[MemoryDiagnoser]
public class Benchmarks
{
    private IMediator _mediator = null!;
    private IMediator _mediatorWithTelemetry = null!;
    private IMediator _mediatorSubscriberOnly = null!;
    private IMediator _mediatorSubscriberWithTelemetry = null!;
    private readonly Ping _request = new() { Message = "Hello World" };
    private readonly Pinged _notification = new();
    private PingedSubscriber _subscriber = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Setup mediator without telemetry (baseline) - with handlers
        var services = new ServiceCollection();
        services.AddSingleton(TextWriter.Null);
        services.AddScoped<IRequestHandler<Ping>, PingHandler>();
        services.AddScoped<INotificationHandler<Pinged>, PingedHandler>();

        services.AddMediator(config =>
        {
            config.WithoutTelemetry();
        });

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();

        // Setup mediator with minimal telemetry - with handlers
        var servicesWithTelemetry = new ServiceCollection();
        servicesWithTelemetry.AddSingleton(TextWriter.Null);
        servicesWithTelemetry.AddScoped<IRequestHandler<Ping>, PingHandler>();
        servicesWithTelemetry.AddScoped<INotificationHandler<Pinged>, PingedHandler>();

        servicesWithTelemetry.AddMediator(config =>
        {
            config.WithTelemetry(TelemetryOptions.Minimal());
        });

        var providerWithTelemetry = servicesWithTelemetry.BuildServiceProvider();
        _mediatorWithTelemetry = providerWithTelemetry.GetRequiredService<IMediator>();

        // Setup mediator for subscriber-only tests (no notification handlers)
        var servicesSubscriberOnly = new ServiceCollection();
        servicesSubscriberOnly.AddSingleton(TextWriter.Null);
        servicesSubscriberOnly.AddScoped<IRequestHandler<Ping>, PingHandler>(); // Only Ping handler

        servicesSubscriberOnly.AddMediator(config =>
        {
            config.WithoutTelemetry();
        });

        var providerSubscriberOnly = servicesSubscriberOnly.BuildServiceProvider();
        _mediatorSubscriberOnly = providerSubscriberOnly.GetRequiredService<IMediator>();

        // Setup mediator for subscriber-only tests with telemetry
        var servicesSubscriberWithTelemetry = new ServiceCollection();
        servicesSubscriberWithTelemetry.AddSingleton(TextWriter.Null);
        servicesSubscriberWithTelemetry.AddScoped<IRequestHandler<Ping>, PingHandler>(); // Only Ping handler

        servicesSubscriberWithTelemetry.AddMediator(config =>
        {
            config.WithTelemetry(TelemetryOptions.Minimal());
        });

        var providerSubscriberWithTelemetry = servicesSubscriberWithTelemetry.BuildServiceProvider();
        _mediatorSubscriberWithTelemetry = providerSubscriberWithTelemetry.GetRequiredService<IMediator>();

        // Setup subscriber for subscriber-only notification benchmarks
        _subscriber = new PingedSubscriber();
        _mediatorSubscriberOnly.Subscribe(_subscriber);
        _mediatorSubscriberWithTelemetry.Subscribe(_subscriber);
    }

    [Benchmark]
    public Task SendRequests()
    {
        return _mediator.Send(_request);
    }

    [Benchmark]
    public Task SendRequestsWithTelemetry()
    {
        return _mediatorWithTelemetry.Send(_request);
    }

    [Benchmark]
    public Task PublishToHandlers()
    {
        return _mediator.Publish(_notification);
    }

    [Benchmark]
    public Task PublishToHandlersWithTelem()
    {
        return _mediatorWithTelemetry.Publish(_notification);
    }

    [Benchmark]
    public Task PublishToSubscribers()
    {
        return _mediatorSubscriberOnly.Publish(_notification);
    }

    [Benchmark]
    public Task PublishToSubscribersTelem()
    {
        return _mediatorSubscriberWithTelemetry.Publish(_notification);
    }
}
