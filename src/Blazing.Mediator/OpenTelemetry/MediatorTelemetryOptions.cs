namespace Blazing.Mediator.OpenTelemetry;

/// <summary>
/// Configuration options for Blazing.Mediator OpenTelemetry integration.
/// </summary>
public sealed class MediatorTelemetryOptions
{
    /// <summary>
    /// Gets or sets whether telemetry is enabled. Default is true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to capture middleware execution details. Default is true.
    /// </summary>
    public bool CaptureMiddlewareDetails { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to capture handler information. Default is true.
    /// </summary>
    public bool CaptureHandlerDetails { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to capture exception details. Default is true.
    /// </summary>
    public bool CaptureExceptionDetails { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of sensitive data patterns to filter from telemetry.
    /// Default includes common patterns like "password", "token", "secret", etc.
    /// </summary>
    public List<string> SensitiveDataPatterns { get; set; } =
        ["password", "token", "secret", "key", "auth", "credential", "connection"];

    /// <summary>
    /// Gets or sets whether to enable health check metrics. Default is true.
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum length for exception messages in telemetry. Default is 200.
    /// </summary>
    public int MaxExceptionMessageLength { get; set; } = 200;

    /// <summary>
    /// Gets or sets the maximum number of stack trace lines to include. Default is 3.
    /// </summary>
    public int MaxStackTraceLines { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to enable packet-level telemetry for streaming operations.
    /// When enabled, creates child spans for each packet which provides detailed visibility but may impact performance.
    /// Default is false for performance reasons.
    /// </summary>
    public bool PacketLevelTelemetryEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the packet telemetry batching interval.
    /// Packets will be batched into events every N packets to reduce telemetry overhead.
    /// Set to 1 to disable batching (create event for every packet).
    /// Default is 10 for optimal performance/visibility balance.
    /// </summary>
    public int PacketTelemetryBatchSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to enable enhanced streaming metrics including jitter, throughput analysis, and performance classification.
    /// Default is true.
    /// </summary>
    public bool EnableStreamingMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to capture packet size information when possible.
    /// This may have performance implications for high-frequency streams.
    /// Default is false.
    /// </summary>
    public bool CapturePacketSize { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to enable detailed streaming performance classification (excellent/good/fair/poor).
    /// Default is true.
    /// </summary>
    public bool EnableStreamingPerformanceClassification { get; set; } = true;

    /// <summary>
    /// Gets or sets the threshold for considering streaming performance as "excellent" (jitter as percentage of average inter-packet time).
    /// Default is 0.1 (10%).
    /// </summary>
    public double ExcellentPerformanceThreshold { get; set; } = 0.1;

    /// <summary>
    /// Gets or sets the threshold for considering streaming performance as "good" (jitter as percentage of average inter-packet time).
    /// Default is 0.3 (30%).
    /// </summary>
    public double GoodPerformanceThreshold { get; set; } = 0.3;

    /// <summary>
    /// Gets or sets the threshold for considering streaming performance as "fair" (jitter as percentage of average inter-packet time).
    /// Values above this threshold are considered "poor".
    /// Default is 0.5 (50%).
    /// </summary>
    public double FairPerformanceThreshold { get; set; } = 0.5;
}