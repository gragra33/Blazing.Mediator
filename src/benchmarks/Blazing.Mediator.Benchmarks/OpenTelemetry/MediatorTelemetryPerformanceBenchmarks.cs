using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Blazing.Mediator;
using Blazing.Mediator.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blazing.Mediator.Benchmarks.OpenTelemetry;

/// <summary>
/// Benchmarks to measure the performance impact of OpenTelemetry instrumentation on Mediator operations.
/// These benchmarks compare performance with telemetry enabled vs disabled to ensure minimal overhead.
/// </summary>
[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class MediatorTelemetryPerformanceBenchmarks
{
    private IServiceProvider _serviceProviderWithTelemetry = null!;
    private IServiceProvider _serviceProviderWithoutTelemetry = null!;
    private IMediator _mediatorWithTelemetry = null!;
    private IMediator _mediatorWithoutTelemetry = null!;
    
    private TestCommand _testCommand = null!;
    private TestQuery _testQuery = null!;
    private TestNotification _testNotification = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Setup with telemetry enabled
        var servicesWithTelemetry = new ServiceCollection();
        servicesWithTelemetry.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        servicesWithTelemetry.AddMediatorTelemetry();
        servicesWithTelemetry.AddMediator(typeof(MediatorTelemetryPerformanceBenchmarks).Assembly);
        servicesWithTelemetry.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();
        servicesWithTelemetry.AddScoped<IRequestHandler<TestQuery, string>, TestQueryHandler>();
        servicesWithTelemetry.AddScoped<INotificationSubscriber<TestNotification>, TestNotificationSubscriber>();
        
        _serviceProviderWithTelemetry = servicesWithTelemetry.BuildServiceProvider();
        _mediatorWithTelemetry = _serviceProviderWithTelemetry.GetRequiredService<IMediator>();

        // Setup without telemetry
        var servicesWithoutTelemetry = new ServiceCollection();
        servicesWithoutTelemetry.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        servicesWithoutTelemetry.DisableMediatorTelemetry();
        servicesWithoutTelemetry.AddMediator(typeof(MediatorTelemetryPerformanceBenchmarks).Assembly);
        servicesWithoutTelemetry.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();
        servicesWithoutTelemetry.AddScoped<IRequestHandler<TestQuery, string>, TestQueryHandler>();
        servicesWithoutTelemetry.AddScoped<INotificationSubscriber<TestNotification>, TestNotificationSubscriber>();
        
        _serviceProviderWithoutTelemetry = servicesWithoutTelemetry.BuildServiceProvider();
        _mediatorWithoutTelemetry = _serviceProviderWithoutTelemetry.GetRequiredService<IMediator>();

        // Subscribe to notifications
        var subscriber = _serviceProviderWithTelemetry.GetRequiredService<INotificationSubscriber<TestNotification>>();
        _mediatorWithTelemetry.Subscribe<TestNotification>(subscriber);
        
        var subscriberWithoutTelemetry = _serviceProviderWithoutTelemetry.GetRequiredService<INotificationSubscriber<TestNotification>>();
        _mediatorWithoutTelemetry.Subscribe<TestNotification>(subscriberWithoutTelemetry);

        // Initialize test objects
        _testCommand = new TestCommand { Value = "benchmark" };
        _testQuery = new TestQuery { Value = "benchmark" };
        _testNotification = new TestNotification { Message = "benchmark" };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProviderWithTelemetry?.Dispose();
        _serviceProviderWithoutTelemetry?.Dispose();
    }

    #region Command Benchmarks

    [Benchmark(Baseline = true)]
    public async Task SendCommand_WithoutTelemetry()
    {
        await _mediatorWithoutTelemetry.Send(_testCommand);
    }

    [Benchmark]
    public async Task SendCommand_WithTelemetry()
    {
        await _mediatorWithTelemetry.Send(_testCommand);
    }

    #endregion

    #region Query Benchmarks

    [Benchmark]
    public async Task<string> SendQuery_WithoutTelemetry()
    {
        return await _mediatorWithoutTelemetry.Send(_testQuery);
    }

    [Benchmark]
    public async Task<string> SendQuery_WithTelemetry()
    {
        return await _mediatorWithTelemetry.Send(_testQuery);
    }

    #endregion

    #region Notification Benchmarks

    [Benchmark]
    public async Task PublishNotification_WithoutTelemetry()
    {
        await _mediatorWithoutTelemetry.Publish(_testNotification);
    }

    [Benchmark]
    public async Task PublishNotification_WithTelemetry()
    {
        await _mediatorWithTelemetry.Publish(_testNotification);
    }

    #endregion

    #region Batch Operation Benchmarks

    [Benchmark]
    [Arguments(10)]
    [Arguments(100)]
    [Arguments(1000)]
    public async Task SendCommand_Batch_WithoutTelemetry(int batchSize)
    {
        var tasks = new Task[batchSize];
        for (int i = 0; i < batchSize; i++)
        {
            tasks[i] = _mediatorWithoutTelemetry.Send(_testCommand);
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    [Arguments(10)]
    [Arguments(100)]
    [Arguments(1000)]
    public async Task SendCommand_Batch_WithTelemetry(int batchSize)
    {
        var tasks = new Task[batchSize];
        for (int i = 0; i < batchSize; i++)
        {
            tasks[i] = _mediatorWithTelemetry.Send(_testCommand);
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    [Arguments(10)]
    [Arguments(100)]
    [Arguments(1000)]
    public async Task PublishNotification_Batch_WithoutTelemetry(int batchSize)
    {
        var tasks = new Task[batchSize];
        for (int i = 0; i < batchSize; i++)
        {
            tasks[i] = _mediatorWithoutTelemetry.Publish(_testNotification);
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    [Arguments(10)]
    [Arguments(100)]
    [Arguments(1000)]
    public async Task PublishNotification_Batch_WithTelemetry(int batchSize)
    {
        var tasks = new Task[batchSize];
        for (int i = 0; i < batchSize; i++)
        {
            tasks[i] = _mediatorWithTelemetry.Publish(_testNotification);
        }
        await Task.WhenAll(tasks);
    }

    #endregion

    #region Test Classes

    public class TestCommand : IRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestCommandHandler : IRequestHandler<TestCommand>
    {
        public async Task Handle(TestCommand request, CancellationToken cancellationToken)
        {
            // Minimal work to focus on telemetry overhead
            await Task.Yield();
        }
    }

    public class TestQuery : IQuery<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestQueryHandler : IRequestHandler<TestQuery, string>
    {
        public async Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            // Minimal work to focus on telemetry overhead
            await Task.Yield();
            return $"Result: {request.Value}";
        }
    }

    public class TestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    public class TestNotificationSubscriber : INotificationSubscriber<TestNotification>
    {
        public async Task OnNotification(TestNotification notification, CancellationToken cancellationToken = default)
        {
            // Minimal work to focus on telemetry overhead
            await Task.Yield();
        }
    }

    #endregion
}