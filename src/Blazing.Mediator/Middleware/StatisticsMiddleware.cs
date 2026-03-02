using System.Diagnostics;
using Blazing.Mediator.Statistics;

namespace Blazing.Mediator.Middleware;

/// <summary>
/// Optional statistics and performance-counter middleware for request-with-response handlers.
/// Increments query/command counters and optionally records execution time and memory on
/// <see cref="MediatorStatistics"/> when registered.
/// <para>
/// Register via the generated <c>AddMediator(MediatorConfiguration config)</c> when
/// <c>config.StatisticsOptions != null</c>. When not registered, zero overhead is incurred.
/// </para>
/// </summary>
/// <typeparam name="TRequest">The request type (query or command with response).</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class StatisticsMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly string _requestName = typeof(TRequest).Name;
    private static readonly bool _isQuery = typeof(IQuery<TResponse>).IsAssignableFrom(typeof(TRequest))
        || typeof(TRequest).Name.EndsWith("Query", StringComparison.OrdinalIgnoreCase);

    private readonly MediatorStatistics _statistics;
    private readonly StatisticsOptions _options;

    /// <summary>
    /// Initialises a new <see cref="StatisticsMiddleware{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="statistics">The statistics service to record into.</param>
    /// <param name="options">Statistics options controlling which counters are active.</param>
    public StatisticsMiddleware(MediatorStatistics statistics, StatisticsOptions options)
    {
        _statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public async ValueTask<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_options.EnableRequestMetrics)
        {
            if (_isQuery)
                _statistics.IncrementQuery(_requestName);
            else
                _statistics.IncrementCommand(_requestName);
        }

        if (!_options.EnablePerformanceCounters)
            return await next().ConfigureAwait(false);

        var sw = Stopwatch.StartNew();
        long startMemory = _options.EnablePerformanceCounters ? GC.GetTotalMemory(false) : 0;
        bool succeeded = false;

        try
        {
            var result = await next().ConfigureAwait(false);
            succeeded = true;
            return result;
        }
        finally
        {
            sw.Stop();
            if (_options.EnablePerformanceCounters)
            {
                _statistics.RecordExecutionTime(_requestName, sw.ElapsedMilliseconds, succeeded);

                if (startMemory > 0)
                {
                    var delta = GC.GetTotalMemory(false) - startMemory;
                    if (delta > 0) _statistics.RecordMemoryAllocation(delta);
                }
            }

            if (_options.EnableDetailedAnalysis)
            {
                _statistics.RecordExecutionPattern(_requestName, DateTime.UtcNow);
            }
        }
    }
}

/// <summary>
/// Optional statistics middleware for void-command handlers.
/// Records the same counters as <see cref="StatisticsMiddleware{TRequest, TResponse}"/>
/// but for commands that return no value.
/// </summary>
/// <typeparam name="TRequest">The void command type.</typeparam>
public sealed class StatisticsMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    private static readonly string _requestName = typeof(TRequest).Name;

    private readonly MediatorStatistics _statistics;
    private readonly StatisticsOptions _options;

    /// <summary>
    /// Initialises a new <see cref="StatisticsMiddleware{TRequest}"/>.
    /// </summary>
    /// <param name="statistics">The statistics service to record into.</param>
    /// <param name="options">Statistics options controlling which counters are active.</param>
    public StatisticsMiddleware(MediatorStatistics statistics, StatisticsOptions options)
    {
        _statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public async ValueTask HandleAsync(
        TRequest request,
        RequestHandlerDelegate next,
        CancellationToken cancellationToken)
    {
        if (_options.EnableRequestMetrics)
            _statistics.IncrementCommand(_requestName);

        if (!_options.EnablePerformanceCounters)
        {
            await next().ConfigureAwait(false);
            return;
        }

        var sw = Stopwatch.StartNew();
        long startMemory = GC.GetTotalMemory(false);
        bool succeeded = false;

        try
        {
            await next().ConfigureAwait(false);
            succeeded = true;
        }
        finally
        {
            sw.Stop();
            _statistics.RecordExecutionTime(_requestName, sw.ElapsedMilliseconds, succeeded);

            var delta = GC.GetTotalMemory(false) - startMemory;
            if (delta > 0) _statistics.RecordMemoryAllocation(delta);

            if (_options.EnableDetailedAnalysis)
                _statistics.RecordExecutionPattern(_requestName, DateTime.UtcNow);
        }
    }
}
