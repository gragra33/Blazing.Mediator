using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Blazing.Mediator.Abstractions;

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
        services.AddScoped<IRequestHandler<ConfigTestCommand>, ConfigTestCommandHandler>();
        services.AddScoped<IRequestHandler<ConfigTestQuery, string>, ConfigTestQueryHandler>();
        services.AddScoped<INotificationSubscriber<ConfigTestNotification>, ConfigTestNotificationSubscriber>();
        services.AddScoped<IStreamRequestHandler<ConfigTestStreamRequest, string>, ConfigTestStreamHandler>();
        
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
        services.AddScoped<IRequestHandler<ConfigTestCommand>, ConfigTestCommandHandler>();
        services.AddScoped<IRequestHandler<ConfigTestQuery, string>, ConfigTestQueryHandler>();
        services.AddScoped<INotificationSubscriber<ConfigTestNotification>, ConfigTestNotificationSubscriber>();
        services.AddScoped<IStreamRequestHandler<ConfigTestStreamRequest, string>, ConfigTestStreamHandler>();
        
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
        services.AddScoped<IRequestHandler<ConfigTestCommand>, ConfigTestCommandHandler>();
        services.AddScoped<IRequestHandler<ConfigTestQuery, string>, ConfigTestQueryHandler>();
        services.AddScoped<INotificationSubscriber<ConfigTestNotification>, ConfigTestNotificationSubscriber>();
        services.AddScoped<IStreamRequestHandler<ConfigTestStreamRequest, string>, ConfigTestStreamHandler>();
        
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
    public async Task Command_MinimalConfiguration()
    {
        await _mediatorMinimalConfig.Send(_command);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Commands")]
    public async Task Command_StandardConfiguration()
    {
        await _mediatorStandardConfig.Send(_command);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Commands")]
    public async Task Command_FullConfiguration()
    {
        await _mediatorFullConfig.Send(_command);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Commands")]
    public async Task Command_EnterpriseConfiguration()
    {
        await _mediatorEnterpriseConfig.Send(_command);
    }

    #endregion

    #region Query Configuration Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Configuration_Queries")]
    public async Task<string> Query_MinimalConfiguration()
    {
        return await _mediatorMinimalConfig.Send(_query);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Queries")]
    public async Task<string> Query_StandardConfiguration()
    {
        return await _mediatorStandardConfig.Send(_query);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Queries")]
    public async Task<string> Query_FullConfiguration()
    {
        return await _mediatorFullConfig.Send(_query);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Queries")]
    public async Task<string> Query_EnterpriseConfiguration()
    {
        return await _mediatorEnterpriseConfig.Send(_query);
    }

    #endregion

    #region Notification Configuration Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Configuration_Notifications")]
    public async Task Notification_MinimalConfiguration()
    {
        await _mediatorMinimalConfig.Publish(_notification);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Notifications")]
    public async Task Notification_StandardConfiguration()
    {
        await _mediatorStandardConfig.Publish(_notification);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Notifications")]
    public async Task Notification_FullConfiguration()
    {
        await _mediatorFullConfig.Publish(_notification);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Notifications")]
    public async Task Notification_EnterpriseConfiguration()
    {
        await _mediatorEnterpriseConfig.Publish(_notification);
    }

    #endregion

    #region Stream Configuration Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Configuration_Streams")]
    public async Task<int> Stream_MinimalConfiguration()
    {
        var count = 0;
        await foreach (var item in _mediatorMinimalConfig.SendStream(_streamRequest))
        {
            count++;
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Streams")]
    public async Task<int> Stream_StandardConfiguration()
    {
        var count = 0;
        await foreach (var item in _mediatorStandardConfig.SendStream(_streamRequest))
        {
            count++;
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Streams")]
    public async Task<int> Stream_FullConfiguration()
    {
        var count = 0;
        await foreach (var item in _mediatorFullConfig.SendStream(_streamRequest))
        {
            count++;
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Streams")]
    public async Task<int> Stream_EnterpriseConfiguration()
    {
        var count = 0;
        await foreach (var item in _mediatorEnterpriseConfig.SendStream(_streamRequest))
        {
            count++;
        }
        return count;
    }

    #endregion

    #region Bulk Operations Configuration Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Configuration_Bulk")]
    public async Task BulkCommands_MinimalConfiguration()
    {
        for (int i = 0; i < 50; i++)
        {
            await _mediatorMinimalConfig.Send(new ConfigTestCommand { Value = $"bulk {i}" });
        }
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Bulk")]
    public async Task BulkCommands_StandardConfiguration()
    {
        for (int i = 0; i < 50; i++)
        {
            await _mediatorStandardConfig.Send(new ConfigTestCommand { Value = $"bulk {i}" });
        }
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Bulk")]
    public async Task BulkCommands_FullConfiguration()
    {
        for (int i = 0; i < 50; i++)
        {
            await _mediatorFullConfig.Send(new ConfigTestCommand { Value = $"bulk {i}" });
        }
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Bulk")]
    public async Task BulkCommands_EnterpriseConfiguration()
    {
        for (int i = 0; i < 50; i++)
        {
            await _mediatorEnterpriseConfig.Send(new ConfigTestCommand { Value = $"bulk {i}" });
        }
    }

    #endregion

    #region Test Classes and Handlers

    public class ConfigTestCommand : IRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class ConfigTestCommandHandler : IRequestHandler<ConfigTestCommand>
    {
        public async Task Handle(ConfigTestCommand request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
        }
    }

    public class ConfigTestQuery : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class ConfigTestQueryHandler : IRequestHandler<ConfigTestQuery, string>
    {
        public async Task<string> Handle(ConfigTestQuery request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            return $"Processed: {request.Value}";
        }
    }

    public class ConfigTestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ConfigTestNotificationSubscriber : INotificationSubscriber<ConfigTestNotification>
    {
        public async Task OnNotification(ConfigTestNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
        }
    }

    public class ConfigTestStreamRequest : IStreamRequest<string>
    {
        public int Count { get; set; }
    }

    public class ConfigTestStreamHandler : IStreamRequestHandler<ConfigTestStreamRequest, string>
    {
        public async IAsyncEnumerable<string> Handle(ConfigTestStreamRequest request,
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

    #region Middleware Implementations

    public class LoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal logging overhead
            var result = await next();
            return result;
        }
    }

    public class ValidationMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal validation overhead
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            
            return await next();
        }
    }

    public class PerformanceMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal performance tracking overhead
            var result = await next();
            return result;
        }
    }

    public class CachingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal caching logic overhead
            var result = await next();
            return result;
        }
    }

    public class SecurityMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal security check overhead
            var result = await next();
            return result;
        }
    }

    public class AuthorizationMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal authorization check overhead
            var result = await next();
            return result;
        }
    }

    public class ErrorHandlingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal error handling overhead
            try
            {
                return await next();
            }
            catch (Exception)
            {
                // Handle error - rethrow for benchmarking purposes
                throw;
            }
        }
    }

    public class AuditMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Minimal audit logging overhead
            var result = await next();
            return result;
        }
    }

    #endregion
}