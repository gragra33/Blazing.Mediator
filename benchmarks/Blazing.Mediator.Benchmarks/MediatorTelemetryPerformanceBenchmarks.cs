using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Blazing.Mediator.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blazing.Mediator.Benchmarks;

/// <summary>
///     Benchmarks to measure the performance impact of OpenTelemetry instrumentation on Mediator operations.
///     These benchmarks compare performance with telemetry enabled vs disabled to ensure minimal overhead.
/// </summary>
[SimpleJob]
[MemoryDiagnoser]
[MinColumn]
[MaxColumn]
[MeanColumn]
[MedianColumn]
public class MediatorTelemetryPerformanceBenchmarks
{
    private IMediator _mediatorWithoutTelemetry = null!;
    private IMediator _mediatorWithTelemetry = null!;
    private ServiceProvider _providerWithTelemetry = null!;
    private ServiceProvider _providerWithoutTelemetry = null!;
    private IServiceScope _scopeWithTelemetry = null!;
    private IServiceScope _scopeWithoutTelemetry = null!;

    private TestCommand _testCommand = null!;
    private TestNotification _testNotification = null!;
    private TestQuery _testQuery = null!;

    [GlobalSetup]
    public void Setup()
    {
        ServiceCollection servicesWithTelemetry = new();
        servicesWithTelemetry.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        servicesWithTelemetry.AddMediatorTelemetry();
        servicesWithTelemetry.AddMediator();
        servicesWithTelemetry.AddScoped<INotificationSubscriber<TestNotification>, TestNotificationSubscriber>();
        _providerWithTelemetry = servicesWithTelemetry.BuildServiceProvider();
        _scopeWithTelemetry = _providerWithTelemetry.CreateScope();
        _mediatorWithTelemetry = _scopeWithTelemetry.ServiceProvider.GetRequiredService<IMediator>();

        ServiceCollection servicesWithoutTelemetry = new();
        servicesWithoutTelemetry.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        servicesWithoutTelemetry.DisableMediatorTelemetry();
        servicesWithoutTelemetry.AddMediator();
        servicesWithoutTelemetry.AddScoped<INotificationSubscriber<TestNotification>, TestNotificationSubscriber>();
        _providerWithoutTelemetry = servicesWithoutTelemetry.BuildServiceProvider();
        _scopeWithoutTelemetry = _providerWithoutTelemetry.CreateScope();
        _mediatorWithoutTelemetry = _scopeWithoutTelemetry.ServiceProvider.GetRequiredService<IMediator>();

        INotificationSubscriber<TestNotification> subscriber =
            _scopeWithTelemetry.ServiceProvider.GetRequiredService<INotificationSubscriber<TestNotification>>();
        _mediatorWithTelemetry.Subscribe(subscriber);

        INotificationSubscriber<TestNotification> subscriberWithoutTelemetry =
            _scopeWithoutTelemetry.ServiceProvider.GetRequiredService<INotificationSubscriber<TestNotification>>();
        _mediatorWithoutTelemetry.Subscribe(subscriberWithoutTelemetry);

        _testCommand = new TestCommand { Value = "benchmark" };
        _testQuery = new TestQuery { Value = "benchmark" };
        _testNotification = new TestNotification { Message = "benchmark" };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _scopeWithTelemetry?.Dispose();
        _scopeWithoutTelemetry?.Dispose();
        _providerWithTelemetry?.Dispose();
        _providerWithoutTelemetry?.Dispose();
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
        Task[] tasks = new Task[batchSize];
        for (int i = 0; i < batchSize; i++) tasks[i] = _mediatorWithoutTelemetry.Send(_testCommand).AsTask();
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    [Arguments(10)]
    [Arguments(100)]
    [Arguments(1000)]
    public async Task SendCommand_Batch_WithTelemetry(int batchSize)
    {
        Task[] tasks = new Task[batchSize];
        for (int i = 0; i < batchSize; i++) tasks[i] = _mediatorWithTelemetry.Send(_testCommand).AsTask();
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    [Arguments(10)]
    [Arguments(100)]
    [Arguments(1000)]
    public async Task PublishNotification_Batch_WithoutTelemetry(int batchSize)
    {
        Task[] tasks = new Task[batchSize];
        for (int i = 0; i < batchSize; i++) tasks[i] = _mediatorWithoutTelemetry.Publish(_testNotification).AsTask();
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    [Arguments(10)]
    [Arguments(100)]
    [Arguments(1000)]
    public async Task PublishNotification_Batch_WithTelemetry(int batchSize)
    {
        Task[] tasks = new Task[batchSize];
        for (int i = 0; i < batchSize; i++) tasks[i] = _mediatorWithTelemetry.Publish(_testNotification).AsTask();
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
        public async ValueTask Handle(TestCommand request, CancellationToken cancellationToken)
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
        public async ValueTask<string> Handle(TestQuery request, CancellationToken cancellationToken)
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