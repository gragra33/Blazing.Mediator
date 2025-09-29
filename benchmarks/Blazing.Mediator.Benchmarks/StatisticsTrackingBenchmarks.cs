using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Benchmarks;

/// <summary>
/// Performance benchmarks for statistics tracking impact in Blazing.Mediator.
/// Measures the performance overhead of enabling/disabling statistics collection.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class StatisticsTrackingBenchmarks
{
    private IMediator _mediatorWithoutStats = null!;
    private IMediator _mediatorWithStats = null!;

    private StatisticsTestCommand _command = null!;
    private StatisticsTestQuery _query = null!;
    private StatisticsTestNotification _notification = null!;
    private StatisticsTestStreamRequest _streamRequest = null!;

    [GlobalSetup]
    public void Setup()
    {
        _command = new StatisticsTestCommand { Value = "stats test" };
        _query = new StatisticsTestQuery { Value = "stats query" };
        _notification = new StatisticsTestNotification { Message = "stats notification" };
        _streamRequest = new StatisticsTestStreamRequest { Count = 10 };

        // Setup mediator WITHOUT statistics tracking
        var servicesWithoutStats = new ServiceCollection();
        servicesWithoutStats.AddMediator(typeof(StatisticsTrackingBenchmarks).Assembly);
        var providerWithoutStats = servicesWithoutStats.BuildServiceProvider();
        _mediatorWithoutStats = providerWithoutStats.GetRequiredService<IMediator>();

        // Setup mediator WITH statistics tracking
        var servicesWithStats = new ServiceCollection();
        servicesWithStats.AddMediator(config =>
        {
            config.WithStatisticsTracking();
        }, typeof(StatisticsTrackingBenchmarks).Assembly);

        var providerWithStats = servicesWithStats.BuildServiceProvider();
        _mediatorWithStats = providerWithStats.GetRequiredService<IMediator>();

        // Subscribe to notifications
        var notificationSubscriberWithoutStats = new StatisticsTestNotificationSubscriber();
        _mediatorWithoutStats.Subscribe(notificationSubscriberWithoutStats);

        var notificationSubscriberWithStats = new StatisticsTestNotificationSubscriber();
        _mediatorWithStats.Subscribe(notificationSubscriberWithStats);
    }

    #region Command Statistics Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Statistics_Commands")]
    public async Task Command_WithoutStatistics()
    {
        await _mediatorWithoutStats.Send(_command);
    }

    [Benchmark]
    [BenchmarkCategory("Statistics_Commands")]
    public async Task Command_WithStatistics()
    {
        await _mediatorWithStats.Send(_command);
    }

    #endregion

    #region Query Statistics Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Statistics_Queries")]
    public async Task<string> Query_WithoutStatistics()
    {
        return await _mediatorWithoutStats.Send(_query);
    }

    [Benchmark]
    [BenchmarkCategory("Statistics_Queries")]
    public async Task<string> Query_WithStatistics()
    {
        return await _mediatorWithStats.Send(_query);
    }

    #endregion

    #region Notification Statistics Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Statistics_Notifications")]
    public async Task Notification_WithoutStatistics()
    {
        await _mediatorWithoutStats.Publish(_notification);
    }

    [Benchmark]
    [BenchmarkCategory("Statistics_Notifications")]
    public async Task Notification_WithStatistics()
    {
        await _mediatorWithStats.Publish(_notification);
    }

    #endregion

    #region Stream Statistics Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Statistics_Streams")]
    public async Task<int> Stream_WithoutStatistics()
    {
        var count = 0;
        await foreach (var item in _mediatorWithoutStats.SendStream(_streamRequest))
        {
            count++;
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Statistics_Streams")]
    public async Task<int> Stream_WithStatistics()
    {
        var count = 0;
        await foreach (var item in _mediatorWithStats.SendStream(_streamRequest))
        {
            count++;
        }
        return count;
    }

    #endregion

    #region Bulk Operations Statistics Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Statistics_Bulk")]
    public async Task BulkCommands_WithoutStatistics()
    {
        for (int i = 0; i < 100; i++)
        {
            await _mediatorWithoutStats.Send(new StatisticsTestCommand { Value = $"bulk {i}" });
        }
    }

    [Benchmark]
    [BenchmarkCategory("Statistics_Bulk")]
    public async Task BulkCommands_WithStatistics()
    {
        for (int i = 0; i < 100; i++)
        {
            await _mediatorWithStats.Send(new StatisticsTestCommand { Value = $"bulk {i}" });
        }
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Statistics_Bulk_Notifications")]
    public async Task BulkNotifications_WithoutStatistics()
    {
        for (int i = 0; i < 100; i++)
        {
            await _mediatorWithoutStats.Publish(new StatisticsTestNotification { Message = $"bulk {i}" });
        }
    }

    [Benchmark]
    [BenchmarkCategory("Statistics_Bulk_Notifications")]
    public async Task BulkNotifications_WithStatistics()
    {
        for (int i = 0; i < 100; i++)
        {
            await _mediatorWithStats.Publish(new StatisticsTestNotification { Message = $"bulk {i}" });
        }
    }

    #endregion

    #region Memory Allocation Statistics Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Statistics_Memory")]
    public async Task MemoryAllocation_WithoutStatistics()
    {
        for (int i = 0; i < 50; i++)
        {
            var command = new StatisticsTestCommand { Value = $"memory test {i}" };
            await _mediatorWithoutStats.Send(command);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Statistics_Memory")]
    public async Task MemoryAllocation_WithStatistics()
    {
        for (int i = 0; i < 50; i++)
        {
            var command = new StatisticsTestCommand { Value = $"memory test {i}" };
            await _mediatorWithStats.Send(command);
        }
    }

    #endregion

    #region Test Classes and Handlers

    public class StatisticsTestCommand : IRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class StatisticsTestCommandHandler : IRequestHandler<StatisticsTestCommand>
    {
        public async Task Handle(StatisticsTestCommand request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
        }
    }

    public class StatisticsTestQuery : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class StatisticsTestQueryHandler : IRequestHandler<StatisticsTestQuery, string>
    {
        public async Task<string> Handle(StatisticsTestQuery request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            return $"Processed: {request.Value}";
        }
    }

    public class StatisticsTestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    public class StatisticsTestNotificationSubscriber : INotificationSubscriber<StatisticsTestNotification>
    {
        public async Task OnNotification(StatisticsTestNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
        }
    }

    public class StatisticsTestStreamRequest : IStreamRequest<string>
    {
        public int Count { get; set; }
    }

    public class StatisticsTestStreamHandler : IStreamRequestHandler<StatisticsTestStreamRequest, string>
    {
        public async IAsyncEnumerable<string> Handle(StatisticsTestStreamRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(1, cancellationToken);
                yield return $"Item {i}";
            }
        }
    }

    #endregion
}