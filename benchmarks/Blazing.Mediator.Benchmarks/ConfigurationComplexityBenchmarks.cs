using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Benchmarks;

/// <summary>
/// Performance benchmarks for configuration complexity impact in Blazing.Mediator.
/// Measures the performance impact of different real-world configuration scenarios.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class ConfigurationComplexityBenchmarks
{
    private IMediator _mediatorMinimalConfig = null!;
    private IMediator _mediatorStandardConfig = null!;
    private IMediator _mediatorFullConfig = null!;
    private IMediator _mediatorEnterpriseConfig = null!;

    private ConfigTestCommand _command = null!;
    private ConfigTestQuery _query = null!;
    private ConfigTestNotification _notification = null!;
    private ConfigTestStreamRequest _streamRequest = null!;

    [GlobalSetup]
    public void Setup()
    {
        _command = new ConfigTestCommand { Value = "config test" };
        _query = new ConfigTestQuery { Value = "config query" };
        _notification = new ConfigTestNotification { Message = "config notification" };
        _streamRequest = new ConfigTestStreamRequest { Count = 10 };

        SetupMinimalConfiguration();
        SetupStandardConfiguration();
        SetupFullConfiguration();
        SetupEnterpriseConfiguration();

        // Subscribe to notifications for all mediators
        _mediatorMinimalConfig.Subscribe(new ConfigTestNotificationSubscriber());
        _mediatorStandardConfig.Subscribe(new ConfigTestNotificationSubscriber());
        _mediatorFullConfig.Subscribe(new ConfigTestNotificationSubscriber());
        _mediatorEnterpriseConfig.Subscribe(new ConfigTestNotificationSubscriber());
    }

    private void SetupMinimalConfiguration()
    {
        // Minimal configuration (basic setup) - Zero configuration approach
        var services = new ServiceCollection();
        services.AddMediator(typeof(ConfigurationComplexityBenchmarks).Assembly);
        var provider = services.BuildServiceProvider();
        _mediatorMinimalConfig = provider.GetRequiredService<IMediator>();
    }

    private void SetupStandardConfiguration()
    {
        // Standard configuration (middleware + stats) - Typical production setup
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking();
        }, typeof(ConfigurationComplexityBenchmarks).Assembly);

        // Add standard middleware
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(LoggingMiddleware<,>));
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(ValidationMiddleware<,>));

        var provider = services.BuildServiceProvider();
        _mediatorStandardConfig = provider.GetRequiredService<IMediator>();
    }

    private void SetupFullConfiguration()
    {
        // Full configuration (all features enabled) - Feature-rich setup
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking();
        }, typeof(ConfigurationComplexityBenchmarks).Assembly);

        // Add comprehensive middleware pipeline
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(LoggingMiddleware<,>));
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(ValidationMiddleware<,>));
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(PerformanceMiddleware<,>));
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(CachingMiddleware<,>));

        var provider = services.BuildServiceProvider();
        _mediatorFullConfig = provider.GetRequiredService<IMediator>();
    }

    private void SetupEnterpriseConfiguration()
    {
        // Enterprise configuration (complex middleware chain) - Maximum feature setup
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking();
        }, typeof(ConfigurationComplexityBenchmarks).Assembly);

        // Add enterprise-grade middleware chain
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(SecurityMiddleware<,>));
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(LoggingMiddleware<,>));
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(ValidationMiddleware<,>));
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(AuthorizationMiddleware<,>));
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(PerformanceMiddleware<,>));
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(CachingMiddleware<,>));
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(ErrorHandlingMiddleware<,>));
        services.AddScoped(typeof(IRequestMiddleware<,>), typeof(AuditMiddleware<,>));

        var provider = services.BuildServiceProvider();
        _mediatorEnterpriseConfig = provider.GetRequiredService<IMediator>();
    }

    #region Command Configuration Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Configuration_Commands")]
    public async Task CommandMinimalConfiguration()
    {
        await _mediatorMinimalConfig.Send(_command).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Commands")]
    public async Task CommandStandardConfiguration()
    {
        await _mediatorStandardConfig.Send(_command).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Commands")]
    public async Task CommandFullConfiguration()
    {
        await _mediatorFullConfig.Send(_command).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Commands")]
    public async Task CommandEnterpriseConfiguration()
    {
        await _mediatorEnterpriseConfig.Send(_command).ConfigureAwait(false);
    }

    #endregion

    #region Query Configuration Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Configuration_Queries")]
    public async Task<string> QueryMinimalConfiguration()
    {
        return await _mediatorMinimalConfig.Send(_query).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Queries")]
    public async Task<string> QueryStandardConfiguration()
    {
        return await _mediatorStandardConfig.Send(_query).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Queries")]
    public async Task<string> QueryFullConfiguration()
    {
        return await _mediatorFullConfig.Send(_query).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Queries")]
    public async Task<string> QueryEnterpriseConfiguration()
    {
        return await _mediatorEnterpriseConfig.Send(_query).ConfigureAwait(false);
    }

    #endregion

    #region Notification Configuration Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Configuration_Notifications")]
    public async Task NotificationMinimalConfiguration()
    {
        await _mediatorMinimalConfig.Publish(_notification).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Notifications")]
    public async Task NotificationStandardConfiguration()
    {
        await _mediatorStandardConfig.Publish(_notification).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Notifications")]
    public async Task NotificationFullConfiguration()
    {
        await _mediatorFullConfig.Publish(_notification).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Notifications")]
    public async Task NotificationEnterpriseConfiguration()
    {
        await _mediatorEnterpriseConfig.Publish(_notification).ConfigureAwait(false);
    }

    #endregion

    #region Stream Configuration Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Configuration_Streams")]
    public async Task<int> StreamMinimalConfiguration()
    {
        var count = 0;
        await foreach (var item in _mediatorMinimalConfig.SendStream(_streamRequest).ConfigureAwait(false))
        {
            count++;
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Streams")]
    public async Task<int> StreamStandardConfiguration()
    {
        var count = 0;
        await foreach (var item in _mediatorStandardConfig.SendStream(_streamRequest).ConfigureAwait(false))
        {
            count++;
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Streams")]
    public async Task<int> StreamFullConfiguration()
    {
        var count = 0;
        await foreach (var item in _mediatorFullConfig.SendStream(_streamRequest).ConfigureAwait(false))
        {
            count++;
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Streams")]
    public async Task<int> StreamEnterpriseConfiguration()
    {
        var count = 0;
        await foreach (var item in _mediatorEnterpriseConfig.SendStream(_streamRequest).ConfigureAwait(false))
        {
            count++;
        }
        return count;
    }

    #endregion

    #region Bulk Operations Configuration Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Configuration_Bulk")]
    public async Task BulkCommandsMinimalConfiguration()
    {
        for (int i = 0; i < 50; i++)
        {
            await _mediatorMinimalConfig.Send(new ConfigTestCommand { Value = $"bulk {i}" }).ConfigureAwait(false);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Bulk")]
    public async Task BulkCommandsStandardConfiguration()
    {
        for (int i = 0; i < 50; i++)
        {
            await _mediatorStandardConfig.Send(new ConfigTestCommand { Value = $"bulk {i}" }).ConfigureAwait(false);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Bulk")]
    public async Task BulkCommandsFullConfiguration()
    {
        for (int i = 0; i < 50; i++)
        {
            await _mediatorFullConfig.Send(new ConfigTestCommand { Value = $"bulk {i}" }).ConfigureAwait(false);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Bulk")]
    public async Task BulkCommandsEnterpriseConfiguration()
    {
        for (int i = 0; i < 50; i++)
        {
            await _mediatorEnterpriseConfig.Send(new ConfigTestCommand { Value = $"bulk {i}" }).ConfigureAwait(false);
        }
    }

    #endregion

    #region Test Classes and Handlers

    internal class ConfigTestCommand : IRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    internal class ConfigTestCommandHandler : IRequestHandler<ConfigTestCommand>
    {
        public async Task Handle(ConfigTestCommand request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
        }
    }

    internal class ConfigTestQuery : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    internal class ConfigTestQueryHandler : IRequestHandler<ConfigTestQuery, string>
    {
        public async Task<string> Handle(ConfigTestQuery request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            return $"Processed: {request.Value}";
        }
    }

    internal class ConfigTestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    internal class ConfigTestNotificationSubscriber : INotificationSubscriber<ConfigTestNotification>
    {
        public async Task OnNotification(ConfigTestNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
        }
    }

    internal class ConfigTestStreamRequest : IStreamRequest<string>
    {
        public int Count { get; set; }
    }

    internal class ConfigTestStreamHandler : IStreamRequestHandler<ConfigTestStreamRequest, string>
    {
        public async IAsyncEnumerable<string> Handle(ConfigTestStreamRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                yield return $"Item {i}";
            }
        }
    }

    #endregion

    #region Middleware Implementations

    internal class LoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal logging overhead
            var result = await next().ConfigureAwait(false);
            return result;
        }
    }

    internal class ValidationMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal validation overhead
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            return await next().ConfigureAwait(false);
        }
    }

    internal class PerformanceMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal performance tracking overhead
            var result = await next().ConfigureAwait(false);
            return result;
        }
    }

    internal class CachingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal caching logic overhead
            var result = await next().ConfigureAwait(false);
            return result;
        }
    }

    internal class SecurityMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal security check overhead
            var result = await next().ConfigureAwait(false);
            return result;
        }
    }

    internal class AuthorizationMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal authorization check overhead
            var result = await next().ConfigureAwait(false);
            return result;
        }
    }

    internal class ErrorHandlingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal error handling overhead
            try
            {
                return await next().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                GC.KeepAlive(e); // Satisfy S2737: add logic to catch clause
                throw;
            }
        }
    }

    internal class AuditMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal audit logging overhead
            var result = await next().ConfigureAwait(false);
            return result;
        }
    }

    #endregion
}
