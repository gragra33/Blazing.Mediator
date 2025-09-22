namespace Blazing.Mediator.Configuration;

/// <summary>
/// Configuration options for mediator statistics tracking.
/// Provides granular control over what statistics are collected and how they are managed.
/// </summary>
public sealed class StatisticsOptions
{
    /// <summary>
    /// Gets or sets whether to track request metrics (queries and commands).
    /// When enabled, tracks execution counts and basic performance data for all requests.
    /// Default: true.
    /// </summary>
    public bool EnableRequestMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to track notification metrics.
    /// When enabled, tracks publication counts and subscriber execution data.
    /// Default: true.
    /// </summary>
    public bool EnableNotificationMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to track middleware metrics.
    /// When enabled, tracks middleware execution times and pipeline performance.
    /// This can add overhead, so consider disabling in high-performance scenarios.
    /// Default: false.
    /// </summary>
    public bool EnableMiddlewareMetrics { get; set; }

    /// <summary>
    /// Gets or sets whether to enable advanced performance counters.
    /// When enabled, tracks detailed performance metrics including memory usage,
    /// throughput rates, and execution time percentiles. Has higher overhead.
    /// Default: false.
    /// </summary>
    public bool EnablePerformanceCounters { get; set; }

    /// <summary>
    /// Gets or sets the period for which metrics data is retained in memory.
    /// After this period, old metrics data is automatically cleaned up to prevent memory leaks.
    /// Set to TimeSpan.Zero to disable automatic cleanup (not recommended for long-running applications).
    /// Default: 24 hours.
    /// </summary>
    public TimeSpan MetricsRetentionPeriod { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Gets or sets whether to enable detailed analysis features.
    /// When enabled, provides comprehensive analysis including execution patterns,
    /// frequency analysis, and performance insights. Requires additional memory.
    /// Default: false.
    /// </summary>
    public bool EnableDetailedAnalysis { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of unique request types to track.
    /// Prevents unbounded memory growth when tracking many different request types.
    /// Set to 0 for unlimited tracking (not recommended).
    /// Default: 1000.
    /// </summary>
    public int MaxTrackedRequestTypes { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the interval for automatic metrics cleanup.
    /// Determines how frequently the cleanup process runs to remove expired metrics.
    /// Should be less than or equal to MetricsRetentionPeriod.
    /// Default: 1 hour.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets whether any statistics tracking is enabled.
    /// </summary>
    public bool IsEnabled => EnableRequestMetrics || EnableNotificationMetrics || EnableMiddlewareMetrics || EnablePerformanceCounters;

    /// <summary>
    /// Validates the current configuration and returns any validation errors.
    /// </summary>
    /// <returns>A list of validation error messages, or an empty list if valid.</returns>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (MetricsRetentionPeriod < TimeSpan.Zero)
        {
            errors.Add("MetricsRetentionPeriod cannot be negative.");
        }

        if (CleanupInterval <= TimeSpan.Zero)
        {
            errors.Add("CleanupInterval must be greater than zero.");
        }

        if (MetricsRetentionPeriod > TimeSpan.Zero && CleanupInterval > MetricsRetentionPeriod)
        {
            errors.Add("CleanupInterval should not be greater than MetricsRetentionPeriod.");
        }

        if (MaxTrackedRequestTypes < 0)
        {
            errors.Add("MaxTrackedRequestTypes cannot be negative.");
        }

        return errors;
    }

    /// <summary>
    /// Validates the configuration and throws an exception if invalid.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the configuration is invalid.</exception>
    public void ValidateAndThrow()
    {
        var errors = Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException($"Invalid StatisticsOptions configuration: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Creates a copy of the current options with all the same values.
    /// </summary>
    /// <returns>A new StatisticsOptions instance with the same configuration.</returns>
    public StatisticsOptions Clone()
    {
        return new StatisticsOptions
        {
            EnableRequestMetrics = EnableRequestMetrics,
            EnableNotificationMetrics = EnableNotificationMetrics,
            EnableMiddlewareMetrics = EnableMiddlewareMetrics,
            EnablePerformanceCounters = EnablePerformanceCounters,
            MetricsRetentionPeriod = MetricsRetentionPeriod,
            EnableDetailedAnalysis = EnableDetailedAnalysis,
            MaxTrackedRequestTypes = MaxTrackedRequestTypes,
            CleanupInterval = CleanupInterval
        };
    }

    /// <summary>
    /// Gets whether any statistics tracking is enabled.
    /// </summary>
    public bool IsAnyTrackingEnabled =>
        EnableRequestMetrics || EnableNotificationMetrics || EnableMiddlewareMetrics || EnablePerformanceCounters;

    /// <summary>
    /// Creates a default configuration suitable for development environments.
    /// Enables basic tracking with reasonable defaults.
    /// </summary>
    public static StatisticsOptions Development()
    {
        return new StatisticsOptions
        {
            EnableRequestMetrics = true,
            EnableNotificationMetrics = true,
            EnableMiddlewareMetrics = true,
            EnablePerformanceCounters = false,
            EnableDetailedAnalysis = true,
            MetricsRetentionPeriod = TimeSpan.FromHours(1),
            CleanupInterval = TimeSpan.FromMinutes(15)
        };
    }

    /// <summary>
    /// Creates a configuration suitable for production environments.
    /// Enables essential tracking with minimal overhead.
    /// </summary>
    public static StatisticsOptions Production()
    {
        return new StatisticsOptions
        {
            EnableRequestMetrics = true,
            EnableNotificationMetrics = true,
            EnableMiddlewareMetrics = false,
            EnablePerformanceCounters = false,
            EnableDetailedAnalysis = false,
            MetricsRetentionPeriod = TimeSpan.FromHours(24),
            CleanupInterval = TimeSpan.FromHours(4)
        };
    }

    /// <summary>
    /// Creates a configuration with all tracking disabled.
    /// Useful for high-performance scenarios where statistics are not needed.
    /// </summary>
    public static StatisticsOptions Disabled()
    {
        return new StatisticsOptions
        {
            EnableRequestMetrics = false,
            EnableNotificationMetrics = false,
            EnableMiddlewareMetrics = false,
            EnablePerformanceCounters = false,
            EnableDetailedAnalysis = false
        };
    }
}