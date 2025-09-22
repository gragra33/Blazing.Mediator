namespace OpenTelemetryExample.Application.Services;

/// <summary>
/// Configuration options for telemetry batching in the OpenTelemetryActivityProcessor.
/// </summary>
public sealed class TelemetryBatchingOptions
{
    /// <summary>
    /// Gets or sets the batch size for streaming telemetry before forcing a flush.
    /// Default: 1000 items.
    /// </summary>
    public int StreamingBatchSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the timeout in milliseconds for streaming telemetry batches.
    /// Default: 5000ms (5 seconds).
    /// </summary>
    public int StreamingBatchTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the batch size for regular (non-streaming) telemetry before forcing a flush.
    /// Default: 100 items.
    /// </summary>
    public int RegularBatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the timeout in milliseconds for regular telemetry batches.
    /// Default: 1000ms (1 second).
    /// </summary>
    public int RegularBatchTimeoutMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the interval in milliseconds for the batch processing timer.
    /// Default: 1000ms (1 second).
    /// </summary>
    public int ProcessingIntervalMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets whether to enable detailed logging for batch processing.
    /// Default: false.
    /// </summary>
    public bool EnableDetailedLogging { get; set; }

    /// <summary>
    /// Validates the configuration and returns any validation errors.
    /// </summary>
    /// <returns>A list of validation error messages, or an empty list if valid.</returns>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (StreamingBatchSize <= 0)
            errors.Add("StreamingBatchSize must be greater than 0");

        if (StreamingBatchTimeoutMs <= 0)
            errors.Add("StreamingBatchTimeoutMs must be greater than 0");

        if (RegularBatchSize <= 0)
            errors.Add("RegularBatchSize must be greater than 0");

        if (RegularBatchTimeoutMs <= 0)
            errors.Add("RegularBatchTimeoutMs must be greater than 0");

        if (ProcessingIntervalMs <= 0)
            errors.Add("ProcessingIntervalMs must be greater than 0");

        if (ProcessingIntervalMs > Math.Min(StreamingBatchTimeoutMs, RegularBatchTimeoutMs))
            errors.Add("ProcessingIntervalMs should be less than or equal to the minimum batch timeout");

        return errors;
    }

    /// <summary>
    /// Creates a configuration optimized for high-throughput streaming scenarios.
    /// </summary>
    /// <returns>A TelemetryBatchingOptions instance optimized for streaming.</returns>
    public static TelemetryBatchingOptions ForStreaming()
    {
        return new TelemetryBatchingOptions
        {
            StreamingBatchSize = 2000,
            StreamingBatchTimeoutMs = 3000, // 3 seconds for faster processing
            RegularBatchSize = 50,
            RegularBatchTimeoutMs = 500,
            ProcessingIntervalMs = 500,
            EnableDetailedLogging = false
        };
    }

    /// <summary>
    /// Creates a configuration optimized for development and debugging.
    /// </summary>
    /// <returns>A TelemetryBatchingOptions instance optimized for development.</returns>
    public static TelemetryBatchingOptions ForDevelopment()
    {
        return new TelemetryBatchingOptions
        {
            StreamingBatchSize = 100,
            StreamingBatchTimeoutMs = 2000, // 2 seconds for quicker feedback
            RegularBatchSize = 20,
            RegularBatchTimeoutMs = 500,
            ProcessingIntervalMs = 500,
            EnableDetailedLogging = true
        };
    }

    /// <summary>
    /// Creates a configuration optimized for production environments.
    /// </summary>
    /// <returns>A TelemetryBatchingOptions instance optimized for production.</returns>
    public static TelemetryBatchingOptions ForProduction()
    {
        return new TelemetryBatchingOptions
        {
            StreamingBatchSize = 1000,
            StreamingBatchTimeoutMs = 5000,
            RegularBatchSize = 100,
            RegularBatchTimeoutMs = 1000,
            ProcessingIntervalMs = 1000,
            EnableDetailedLogging = false
        };
    }
}