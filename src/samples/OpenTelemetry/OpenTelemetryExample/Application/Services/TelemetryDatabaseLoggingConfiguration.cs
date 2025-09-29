namespace OpenTelemetryExample.Application.Services;

/// <summary>
/// Configuration options for the TelemetryDatabaseLoggingProvider.
/// Follows Microsoft's recommended pattern for custom logging providers.
/// </summary>
public sealed class TelemetryDatabaseLoggingConfiguration
{
    /// <summary>
    /// Gets or sets the batch size for log processing.
    /// Default is 50 log entries per batch.
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets the processing interval in milliseconds.
    /// Default is 2000ms (2 seconds).
    /// </summary>
    public int ProcessingIntervalMs { get; set; } = 2000;

    /// <summary>
    /// Gets or sets the minimum log level to capture.
    /// Default is Information level.
    /// </summary>
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets whether to capture debug-level logs.
    /// Default is true for development scenarios.
    /// </summary>
    public bool CaptureDebugLogs { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the provider is enabled.
    /// Default is true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the patterns to skip to avoid recursive logging.
    /// These patterns are checked against both the message content and category name.
    /// </summary>
    public string[] SkipPatterns { get; set; } =
    [
        "TelemetryDatabaseLoggingProvider",
        "ProcessLogBatch",
        "Enqueued log:",
        "Capturing log:",
        "Successfully saved",
        "Processing batch of",
        "Microsoft.EntityFrameworkCore.Database.Command",
        "Microsoft.EntityFrameworkCore.Infrastructure",
        "Microsoft.EntityFrameworkCore.Query",
        "Microsoft.EntityFrameworkCore.Update",
        "Microsoft.EntityFrameworkCore.Database.Transaction",
        "Microsoft.EntityFrameworkCore.ChangeTracking",
        "Microsoft.EntityFrameworkCore.Model",
        "Microsoft.EntityFrameworkCore.Model.Validation",
        "POST /api/logs/test-logging",
        "POST /api/logs/test-database-logging"
    ];
}