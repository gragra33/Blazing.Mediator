using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Blazing.Mediator.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Benchmarks;

/// <summary>
///     Performance benchmarks for middleware pipeline configurations in Blazing.Mediator.
///     Measures the performance impact of different middleware pipeline setups.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class MiddlewarePipelineBenchmarks
{
    private MiddlewareTestCommand _command = null!;
    private IMediator _mediatorConditionalMiddleware = null!;
    private IMediator _mediatorEnterpriseConfig = null!;
    private IMediator _mediatorFullConfig = null!;

    // Configuration complexity benchmarks
    private IMediator _mediatorMinimalConfig = null!;
    private IMediator _mediatorMultipleMiddleware = null!;
    private IMediator _mediatorNoMiddleware = null!;
    private IMediator _mediatorSingleMiddleware = null!;
    private IMediator _mediatorStandardConfig = null!;
    private MiddlewareTestQuery _query = null!;

    [GlobalSetup]
    public void Setup()
    {
        _command = new MiddlewareTestCommand { Value = "middleware test" };
        _query = new MiddlewareTestQuery { Value = "middleware query" };

        // Setup mediator with NO middleware
        ServiceCollection servicesNoMiddleware = new();
        servicesNoMiddleware.AddMediator();
        ServiceProvider providerNoMiddleware = servicesNoMiddleware.BuildServiceProvider();
        _mediatorNoMiddleware = providerNoMiddleware.GetRequiredService<IMediator>();

        // Setup mediator with SINGLE middleware
        ServiceCollection servicesSingleMiddleware = new();
        servicesSingleMiddleware.AddMediator();
        servicesSingleMiddleware.AddScoped(typeof(IRequestMiddleware<,>), typeof(LoggingMiddleware<,>));
        ServiceProvider providerSingleMiddleware = servicesSingleMiddleware.BuildServiceProvider();
        _mediatorSingleMiddleware = providerSingleMiddleware.GetRequiredService<IMediator>();

        // Setup mediator with MULTIPLE middleware (3 middlewares)
        ServiceCollection servicesMultipleMiddleware = new();
        servicesMultipleMiddleware.AddMediator();
        servicesMultipleMiddleware.AddScoped(typeof(IRequestMiddleware<,>), typeof(LoggingMiddleware<,>));
        servicesMultipleMiddleware.AddScoped(typeof(IRequestMiddleware<,>), typeof(ValidationMiddleware<,>));
        servicesMultipleMiddleware.AddScoped(typeof(IRequestMiddleware<,>), typeof(PerformanceMiddleware<,>));
        ServiceProvider providerMultipleMiddleware = servicesMultipleMiddleware.BuildServiceProvider();
        _mediatorMultipleMiddleware = providerMultipleMiddleware.GetRequiredService<IMediator>();

        // Setup mediator with CONDITIONAL middleware
        ServiceCollection servicesConditionalMiddleware = new();
        servicesConditionalMiddleware.AddMediator();
        servicesConditionalMiddleware.AddScoped(typeof(IRequestMiddleware<,>), typeof(ConditionalLoggingMiddleware<,>));
        ServiceProvider providerConditionalMiddleware = servicesConditionalMiddleware.BuildServiceProvider();
        _mediatorConditionalMiddleware = providerConditionalMiddleware.GetRequiredService<IMediator>();

        // Configuration Complexity Impact benchmarks
        SetupConfigurationComplexityBenchmarks();
    }

    private void SetupConfigurationComplexityBenchmarks()
    {
        // Minimal configuration (basic setup)
        ServiceCollection servicesMinimal = new();
        servicesMinimal.AddMediator();
        ServiceProvider providerMinimal = servicesMinimal.BuildServiceProvider();
        _mediatorMinimalConfig = providerMinimal.GetRequiredService<IMediator>();

        // Standard configuration (middleware + stats)
        ServiceCollection servicesStandard = new();
        MediatorConfiguration standardConfig = new();
        standardConfig.WithStatisticsTracking();
        servicesStandard.AddMediator(standardConfig);
        servicesStandard.AddScoped(typeof(IRequestMiddleware<,>), typeof(LoggingMiddleware<,>));
        ServiceProvider providerStandard = servicesStandard.BuildServiceProvider();
        _mediatorStandardConfig = providerStandard.GetRequiredService<IMediator>();

        // Full configuration (all features enabled)
        ServiceCollection servicesFull = new();
        MediatorConfiguration fullConfig = new();
        fullConfig.WithStatisticsTracking();
        servicesFull.AddMediator(fullConfig);
        servicesFull.AddScoped(typeof(IRequestMiddleware<,>), typeof(LoggingMiddleware<,>));
        servicesFull.AddScoped(typeof(IRequestMiddleware<,>), typeof(ValidationMiddleware<,>));
        ServiceProvider providerFull = servicesFull.BuildServiceProvider();
        _mediatorFullConfig = providerFull.GetRequiredService<IMediator>();

        // Enterprise configuration (complex middleware chain)
        ServiceCollection servicesEnterprise = new();
        MediatorConfiguration enterpriseConfig = new();
        enterpriseConfig.WithStatisticsTracking();
        servicesEnterprise.AddMediator(enterpriseConfig);
        servicesEnterprise.AddScoped(typeof(IRequestMiddleware<,>), typeof(LoggingMiddleware<,>));
        servicesEnterprise.AddScoped(typeof(IRequestMiddleware<,>), typeof(ValidationMiddleware<,>));
        servicesEnterprise.AddScoped(typeof(IRequestMiddleware<,>), typeof(PerformanceMiddleware<,>));
        servicesEnterprise.AddScoped(typeof(IRequestMiddleware<,>), typeof(ConditionalLoggingMiddleware<,>));
        ServiceProvider providerEnterprise = servicesEnterprise.BuildServiceProvider();
        _mediatorEnterpriseConfig = providerEnterprise.GetRequiredService<IMediator>();
    }

    #region Middleware Pipeline Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Middleware_Pipeline")]
    public async Task CommandNoMiddleware()
    {
        await _mediatorNoMiddleware.Send(_command).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Middleware_Pipeline")]
    public async Task CommandSingleMiddleware()
    {
        await _mediatorSingleMiddleware.Send(_command).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Middleware_Pipeline")]
    public async Task CommandMultipleMiddleware()
    {
        await _mediatorMultipleMiddleware.Send(_command).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Middleware_Pipeline")]
    public async Task CommandConditionalMiddleware()
    {
        await _mediatorConditionalMiddleware.Send(_command).ConfigureAwait(false);
    }

    #endregion

    #region Query Middleware Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Query_Middleware")]
    public async Task<string> QueryNoMiddleware()
    {
        return await _mediatorNoMiddleware.Send(_query).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Query_Middleware")]
    public async Task<string> QuerySingleMiddleware()
    {
        return await _mediatorSingleMiddleware.Send(_query).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Query_Middleware")]
    public async Task<string> QueryMultipleMiddleware()
    {
        return await _mediatorMultipleMiddleware.Send(_query).ConfigureAwait(false);
    }

    #endregion

    #region Configuration Complexity Impact Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Configuration_Complexity")]
    public async Task Command_MinimalConfiguration()
    {
        await _mediatorMinimalConfig.Send(_command).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Complexity")]
    public async Task Command_StandardConfiguration()
    {
        await _mediatorStandardConfig.Send(_command).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Complexity")]
    public async Task Command_FullConfiguration()
    {
        await _mediatorFullConfig.Send(_command).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Complexity")]
    public async Task Command_EnterpriseConfiguration()
    {
        await _mediatorEnterpriseConfig.Send(_command).ConfigureAwait(false);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Configuration_Query")]
    public async Task<string> Query_MinimalConfiguration()
    {
        return await _mediatorMinimalConfig.Send(_query).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Query")]
    public async Task<string> Query_StandardConfiguration()
    {
        return await _mediatorStandardConfig.Send(_query).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Query")]
    public async Task<string> Query_FullConfiguration()
    {
        return await _mediatorFullConfig.Send(_query).ConfigureAwait(false);
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Query")]
    public async Task<string> Query_EnterpriseConfiguration()
    {
        return await _mediatorEnterpriseConfig.Send(_query).ConfigureAwait(false);
    }

    #endregion

    #region Test Classes and Handlers

    public class MiddlewareTestCommand : IRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class MiddlewareTestCommandHandler : IRequestHandler<MiddlewareTestCommand>
    {
        public async ValueTask Handle(MiddlewareTestCommand request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
        }
    }

    public class MiddlewareTestQuery : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class MiddlewareTestQueryHandler : IRequestHandler<MiddlewareTestQuery, string>
    {
        public async ValueTask<string> Handle(MiddlewareTestQuery request,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            return $"Processed: {request.Value}";
        }
    }

    #endregion

    #region Middleware Implementations

    public class LoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Minimal logging overhead
            TResponse result = await next().ConfigureAwait(false);
            return result;
        }
    }

    public class ValidationMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Minimal validation overhead
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            return await next().ConfigureAwait(false);
        }
    }

    public class PerformanceMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Minimal performance tracking overhead
            TResponse result = await next().ConfigureAwait(false);
            return result;
        }
    }

    public class ConditionalLoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Only logs commands
            if (request.GetType().Name.Contains("Command", StringComparison.OrdinalIgnoreCase))
            {
                // Log the command
            }

            TResponse result = await next().ConfigureAwait(false);
            return result;
        }
    }

    #endregion
}