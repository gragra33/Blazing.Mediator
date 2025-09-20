using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Blazing.Mediator.Benchmarks.OpenTelemetry;

/// <summary>
/// Performance benchmarks for OpenTelemetry instrumentation overhead in Blazing.Mediator.
/// Measures the performance impact of telemetry on Send, Publish, and streaming operations.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class MediatorTelemetryPerformanceBenchmarks
{
    private ServiceProvider _serviceProviderWithTelemetry = null!;
    private ServiceProvider _serviceProviderWithoutTelemetry = null!;
    private IMediator _mediatorWithTelemetry = null!;
    private IMediator _mediatorWithoutTelemetry = null!;

    private BenchmarkTestCommand _command = null!;
    private BenchmarkTestQuery _query = null!;
    private BenchmarkTestNotification _notification = null!;
    private BenchmarkTestStreamRequest _streamRequest = null!;

    private ActivityListener? _activityListener;

    [GlobalSetup]
    public void Setup()
    {
        // Create command and query instances
        _command = new BenchmarkTestCommand { Value = "benchmark test" };
        _query = new BenchmarkTestQuery { Value = "benchmark query" };
        _notification = new BenchmarkTestNotification { Message = "benchmark notification" };
        _streamRequest = new BenchmarkTestStreamRequest { Count = 10 };

        // Setup mediator WITH telemetry
        var servicesWithTelemetry = new ServiceCollection();
        servicesWithTelemetry.AddLogging();
        servicesWithTelemetry.AddMediator(); // Don't scan assemblies

        // Register handlers manually to avoid conflicts
        servicesWithTelemetry.AddScoped<IRequestHandler<BenchmarkTestCommand>, BenchmarkTestCommandHandler>();
        servicesWithTelemetry.AddScoped<IRequestHandler<BenchmarkTestQuery, string>, BenchmarkTestQueryHandler>();
        servicesWithTelemetry.AddScoped<INotificationSubscriber<BenchmarkTestNotification>, BenchmarkTestNotificationSubscriber>();
        servicesWithTelemetry.AddScoped<IStreamRequestHandler<BenchmarkTestStreamRequest, string>, BenchmarkTestStreamHandler>();

        _serviceProviderWithTelemetry = servicesWithTelemetry.BuildServiceProvider();
        _mediatorWithTelemetry = _serviceProviderWithTelemetry.GetRequiredService<IMediator>();

        // Setup mediator WITHOUT telemetry
        var servicesWithoutTelemetry = new ServiceCollection();
        servicesWithoutTelemetry.AddLogging();
        servicesWithoutTelemetry.AddMediator(); // Don't scan assemblies

        // Register handlers manually to avoid conflicts
        servicesWithoutTelemetry.AddScoped<IRequestHandler<BenchmarkTestCommand>, BenchmarkTestCommandHandler>();
        servicesWithoutTelemetry.AddScoped<IRequestHandler<BenchmarkTestQuery, string>, BenchmarkTestQueryHandler>();
        servicesWithoutTelemetry.AddScoped<INotificationSubscriber<BenchmarkTestNotification>, BenchmarkTestNotificationSubscriber>();
        servicesWithoutTelemetry.AddScoped<IStreamRequestHandler<BenchmarkTestStreamRequest, string>, BenchmarkTestStreamHandler>();

        _serviceProviderWithoutTelemetry = servicesWithoutTelemetry.BuildServiceProvider();
        _mediatorWithoutTelemetry = _serviceProviderWithoutTelemetry.GetRequiredService<IMediator>();

        // Disable telemetry for the without-telemetry mediator
        Mediator.TelemetryEnabled = false;

        // Setup activity listener to capture telemetry (for telemetry-enabled tests)
        _activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Blazing.Mediator",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = _ => { /* Capture activity */ },
            ActivityStopped = _ => { /* Activity completed */ }
        };
        ActivitySource.AddActivityListener(_activityListener);

        // Re-enable telemetry for telemetry-enabled tests
        Mediator.TelemetryEnabled = true;

        // Subscribe to notifications
        _mediatorWithTelemetry.Subscribe(_serviceProviderWithTelemetry.GetService<INotificationSubscriber<BenchmarkTestNotification>>()!);
        _mediatorWithoutTelemetry.Subscribe(_serviceProviderWithoutTelemetry.GetService<INotificationSubscriber<BenchmarkTestNotification>>()!);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _activityListener?.Dispose();
        _serviceProviderWithTelemetry?.Dispose();
        _serviceProviderWithoutTelemetry?.Dispose();
    }

    #region Send Command Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Send_Command")]
    public async Task Send_Command_WithoutTelemetry()
    {
        await _mediatorWithoutTelemetry.Send(_command);
    }

    [Benchmark]
    [BenchmarkCategory("Send_Command")]
    public async Task Send_Command_WithTelemetry()
    {
        await _mediatorWithTelemetry.Send(_command);
    }

    #endregion

    #region Send Query Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Send_Query")]
    public async Task<string> Send_Query_WithoutTelemetry()
    {
        return await _mediatorWithoutTelemetry.Send(_query);
    }

    [Benchmark]
    [BenchmarkCategory("Send_Query")]
    public async Task<string> Send_Query_WithTelemetry()
    {
        return await _mediatorWithTelemetry.Send(_query);
    }

    #endregion

    #region Publish Notification Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Publish_Notification")]
    public async Task Publish_Notification_WithoutTelemetry()
    {
        await _mediatorWithoutTelemetry.Publish(_notification);
    }

    [Benchmark]
    [BenchmarkCategory("Publish_Notification")]
    public async Task Publish_Notification_WithTelemetry()
    {
        await _mediatorWithTelemetry.Publish(_notification);
    }

    #endregion

    #region Stream Request Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Send_Stream")]
    public async Task<int> Send_Stream_WithoutTelemetry()
    {
        var count = 0;
        await foreach (var unused in _mediatorWithoutTelemetry.SendStream(_streamRequest))
        {
            count++;
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Send_Stream")]
    public async Task<int> Send_Stream_WithTelemetry()
    {
        var count = 0;
        await foreach (var unused in _mediatorWithTelemetry.SendStream(_streamRequest))
        {
            count++;
        }
        return count;
    }

    #endregion

    #region Bulk Operations Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Bulk_Operations")]
    public async Task Bulk_Commands_WithoutTelemetry()
    {
        for (int i = 0; i < 100; i++)
        {
            await _mediatorWithoutTelemetry.Send(new BenchmarkTestCommand { Value = $"bulk test {i}" });
        }
    }

    [Benchmark]
    [BenchmarkCategory("Bulk_Operations")]
    public async Task Bulk_Commands_WithTelemetry()
    {
        for (int i = 0; i < 100; i++)
        {
            await _mediatorWithTelemetry.Send(new BenchmarkTestCommand { Value = $"bulk test {i}" });
        }
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Bulk_Notifications")]
    public async Task Bulk_Notifications_WithoutTelemetry()
    {
        for (int i = 0; i < 100; i++)
        {
            await _mediatorWithoutTelemetry.Publish(new BenchmarkTestNotification { Message = $"bulk notification {i}" });
        }
    }

    [Benchmark]
    [BenchmarkCategory("Bulk_Notifications")]
    public async Task Bulk_Notifications_WithTelemetry()
    {
        for (int i = 0; i < 100; i++)
        {
            await _mediatorWithTelemetry.Publish(new BenchmarkTestNotification { Message = $"bulk notification {i}" });
        }
    }

    #endregion

    #region Memory Allocation Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Memory_Allocation")]
    public async Task Memory_Commands_WithoutTelemetry()
    {
        for (int i = 0; i < 50; i++)
        {
            var command = new BenchmarkTestCommand { Value = $"memory test {i}" };
            await _mediatorWithoutTelemetry.Send(command);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Memory_Allocation")]
    public async Task Memory_Commands_WithTelemetry()
    {
        for (int i = 0; i < 50; i++)
        {
            var command = new BenchmarkTestCommand { Value = $"memory test {i}" };
            await _mediatorWithTelemetry.Send(command);
        }
    }

    #endregion

    #region Test Classes

    public class BenchmarkTestCommand : IRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class BenchmarkTestCommandHandler : IRequestHandler<BenchmarkTestCommand>
    {
        public async Task Handle(BenchmarkTestCommand request, CancellationToken cancellationToken)
        {
            // Minimal work to simulate real handler
            await Task.Delay(1, cancellationToken);
        }
    }

    public class BenchmarkTestQuery : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class BenchmarkTestQueryHandler : IRequestHandler<BenchmarkTestQuery, string>
    {
        public async Task<string> Handle(BenchmarkTestQuery request, CancellationToken cancellationToken)
        {
            // Minimal work to simulate real handler
            await Task.Delay(1, cancellationToken);
            return $"Handled: {request.Value}";
        }
    }

    public class BenchmarkTestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    public class BenchmarkTestNotificationSubscriber : INotificationSubscriber<BenchmarkTestNotification>
    {
        public async Task OnNotification(BenchmarkTestNotification notification, CancellationToken cancellationToken = default)
        {
            // Minimal work to simulate real subscriber
            await Task.Delay(1, cancellationToken);
        }
    }

    public class BenchmarkTestStreamRequest : IStreamRequest<string>
    {
        public int Count { get; set; }
    }

    public class BenchmarkTestStreamHandler : IStreamRequestHandler<BenchmarkTestStreamRequest, string>
    {
        public async IAsyncEnumerable<string> Handle(BenchmarkTestStreamRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(1, cancellationToken); // Minimal delay
                yield return $"Item {i}";
            }
        }
    }

    #endregion
}