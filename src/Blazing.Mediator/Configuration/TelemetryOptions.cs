namespace Blazing.Mediator.Configuration;

/// <summary>
/// Configuration options for Blazing.Mediator OpenTelemetry integration.
/// Provides granular control over what telemetry data is collected and how it is configured.
/// </summary>
public class TelemetryOptions : IEnvironmentConfigurationOptions<TelemetryOptions>
{
    #region Core Telemetry Settings

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
    /// Gets or sets whether to enable health check metrics. Default is true.
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;

    #endregion

    #region Exception and Error Handling

    /// <summary>
    /// Gets or sets the maximum length for exception messages in telemetry. Default is 200.
    /// </summary>
    public int MaxExceptionMessageLength { get; set; } = 200;

    /// <summary>
    /// Gets or sets the maximum number of stack trace lines to include. Default is 3.
    /// </summary>
    public int MaxStackTraceLines { get; set; } = 3;

    #endregion

    #region Sensitive Data Protection

    /// <summary>
    /// Gets or sets the list of sensitive data patterns to filter from telemetry.
    /// Default includes common patterns like "password", "token", "secret", etc.
    /// </summary>
    public List<string> SensitiveDataPatterns { get; set; } =
        ["password", "token", "secret", "key", "auth", "credential", "connection"];

    #endregion

    #region Streaming Telemetry Settings

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

    #endregion

    #region Streaming Performance Thresholds

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

    #endregion

    #region Notification Telemetry Settings

    /// <summary>
    /// Gets or sets whether to capture detailed notification handler information. Default is true.
    /// </summary>
    public bool CaptureNotificationHandlerDetails { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to create child spans for individual notification handlers. Default is true.
    /// </summary>
    public bool CreateHandlerChildSpans { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to capture notification subscriber counts. Default is true.
    /// </summary>
    public bool CaptureSubscriberMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to capture notification middleware execution details. Default is true.
    /// </summary>
    public bool CaptureNotificationMiddlewareDetails { get; set; } = true;

    #endregion

    #region Properties and Validation

    /// <summary>
    /// Gets whether any telemetry tracking is effectively enabled.
    /// </summary>
    public bool IsEnabled => Enabled && (CaptureMiddlewareDetails || CaptureHandlerDetails || 
                                       CaptureNotificationHandlerDetails || EnableStreamingMetrics || EnableHealthChecks);

    /// <summary>
    /// Gets whether any notification-specific telemetry is enabled.
    /// </summary>
    public bool IsNotificationTelemetryEnabled => Enabled && (CaptureNotificationHandlerDetails || 
                                                             CreateHandlerChildSpans || CaptureSubscriberMetrics || 
                                                             CaptureNotificationMiddlewareDetails);

    /// <summary>
    /// Gets whether any streaming telemetry is enabled.
    /// </summary>
    public bool IsStreamingTelemetryEnabled => Enabled && (EnableStreamingMetrics || PacketLevelTelemetryEnabled);

    /// <summary>
    /// Validates the current configuration and returns any validation errors.
    /// </summary>
    /// <returns>A list of validation error messages, or an empty list if valid.</returns>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (MaxExceptionMessageLength < 0)
        {
            errors.Add("MaxExceptionMessageLength cannot be negative.");
        }

        if (MaxStackTraceLines < 0)
        {
            errors.Add("MaxStackTraceLines cannot be negative.");
        }

        if (PacketTelemetryBatchSize < 1)
        {
            errors.Add("PacketTelemetryBatchSize must be at least 1.");
        }

        if (ExcellentPerformanceThreshold < 0 || ExcellentPerformanceThreshold > 1)
        {
            errors.Add("ExcellentPerformanceThreshold must be between 0 and 1.");
        }

        if (GoodPerformanceThreshold < 0 || GoodPerformanceThreshold > 1)
        {
            errors.Add("GoodPerformanceThreshold must be between 0 and 1.");
        }

        if (FairPerformanceThreshold < 0 || FairPerformanceThreshold > 1)
        {
            errors.Add("FairPerformanceThreshold must be between 0 and 1.");
        }

        if (ExcellentPerformanceThreshold >= GoodPerformanceThreshold)
        {
            errors.Add("ExcellentPerformanceThreshold must be less than GoodPerformanceThreshold.");
        }

        if (GoodPerformanceThreshold >= FairPerformanceThreshold)
        {
            errors.Add("GoodPerformanceThreshold must be less than FairPerformanceThreshold.");
        }

        if (SensitiveDataPatterns == null)
        {
            errors.Add("SensitiveDataPatterns cannot be null.");
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
            throw new ArgumentException($"Invalid TelemetryOptions configuration: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Creates a copy of the current options with all the same values.
    /// </summary>
    /// <returns>A new TelemetryOptions instance with the same configuration.</returns>
    public TelemetryOptions Clone()
    {
        return new TelemetryOptions
        {
            Enabled = Enabled,
            CaptureMiddlewareDetails = CaptureMiddlewareDetails,
            CaptureHandlerDetails = CaptureHandlerDetails,
            CaptureExceptionDetails = CaptureExceptionDetails,
            EnableHealthChecks = EnableHealthChecks,
            MaxExceptionMessageLength = MaxExceptionMessageLength,
            MaxStackTraceLines = MaxStackTraceLines,
            SensitiveDataPatterns = new List<string>(SensitiveDataPatterns),
            PacketLevelTelemetryEnabled = PacketLevelTelemetryEnabled,
            PacketTelemetryBatchSize = PacketTelemetryBatchSize,
            EnableStreamingMetrics = EnableStreamingMetrics,
            CapturePacketSize = CapturePacketSize,
            EnableStreamingPerformanceClassification = EnableStreamingPerformanceClassification,
            ExcellentPerformanceThreshold = ExcellentPerformanceThreshold,
            GoodPerformanceThreshold = GoodPerformanceThreshold,
            FairPerformanceThreshold = FairPerformanceThreshold,
            CaptureNotificationHandlerDetails = CaptureNotificationHandlerDetails,
            CreateHandlerChildSpans = CreateHandlerChildSpans,
            CaptureSubscriberMetrics = CaptureSubscriberMetrics,
            CaptureNotificationMiddlewareDetails = CaptureNotificationMiddlewareDetails
        };
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Creates a default configuration suitable for development environments.
    /// Enables comprehensive telemetry with detailed information for debugging.
    /// </summary>
    public static TelemetryOptions Development()
    {
        return new TelemetryOptions
        {
            Enabled = true,
            CaptureMiddlewareDetails = true,
            CaptureHandlerDetails = true,
            CaptureExceptionDetails = true,
            EnableHealthChecks = true,
            MaxExceptionMessageLength = 500,
            MaxStackTraceLines = 10,
            PacketLevelTelemetryEnabled = true,
            PacketTelemetryBatchSize = 5,
            EnableStreamingMetrics = true,
            CapturePacketSize = true,
            EnableStreamingPerformanceClassification = true,
            CaptureNotificationHandlerDetails = true,
            CreateHandlerChildSpans = true,
            CaptureSubscriberMetrics = true,
            CaptureNotificationMiddlewareDetails = true,
            SensitiveDataPatterns = ["password", "token", "secret", "key", "auth", "credential", "connection"]
        };
    }

    /// <summary>
    /// Creates a configuration suitable for production environments.
    /// Enables essential telemetry with optimized performance settings.
    /// </summary>
    public static TelemetryOptions Production()
    {
        return new TelemetryOptions
        {
            Enabled = true,
            CaptureMiddlewareDetails = false,
            CaptureHandlerDetails = true,
            CaptureExceptionDetails = true,
            EnableHealthChecks = true,
            MaxExceptionMessageLength = 200,
            MaxStackTraceLines = 3,
            PacketLevelTelemetryEnabled = false,
            PacketTelemetryBatchSize = 20,
            EnableStreamingMetrics = true,
            CapturePacketSize = false,
            EnableStreamingPerformanceClassification = false,
            CaptureNotificationHandlerDetails = true,
            CreateHandlerChildSpans = false,
            CaptureSubscriberMetrics = false,
            CaptureNotificationMiddlewareDetails = false,
            SensitiveDataPatterns = ["password", "token", "secret", "key", "auth", "credential", "connection", "api_key", "bearer", "oauth"]
        };
    }

    /// <summary>
    /// Creates a configuration with all telemetry disabled.
    /// Useful for high-performance scenarios where telemetry is not needed.
    /// </summary>
    public static TelemetryOptions Disabled()
    {
        return new TelemetryOptions
        {
            Enabled = false,
            CaptureMiddlewareDetails = false,
            CaptureHandlerDetails = false,
            CaptureExceptionDetails = false,
            EnableHealthChecks = false,
            PacketLevelTelemetryEnabled = false,
            EnableStreamingMetrics = false,
            CapturePacketSize = false,
            EnableStreamingPerformanceClassification = false,
            CaptureNotificationHandlerDetails = false,
            CreateHandlerChildSpans = false,
            CaptureSubscriberMetrics = false,
            CaptureNotificationMiddlewareDetails = false,
            SensitiveDataPatterns = []
        };
    }

    /// <summary>
    /// Creates a minimal configuration with only essential telemetry enabled.
    /// Suitable for lightweight monitoring scenarios.
    /// </summary>
    public static TelemetryOptions Minimal()
    {
        return new TelemetryOptions
        {
            Enabled = true,
            CaptureMiddlewareDetails = false,
            CaptureHandlerDetails = false,
            CaptureExceptionDetails = true,
            EnableHealthChecks = true,
            MaxExceptionMessageLength = 100,
            MaxStackTraceLines = 2,
            PacketLevelTelemetryEnabled = false,
            PacketTelemetryBatchSize = 50,
            EnableStreamingMetrics = false,
            CapturePacketSize = false,
            EnableStreamingPerformanceClassification = false,
            CaptureNotificationHandlerDetails = false,
            CreateHandlerChildSpans = false,
            CaptureSubscriberMetrics = false,
            CaptureNotificationMiddlewareDetails = false,
            SensitiveDataPatterns = ["password", "token", "secret", "key"]
        };
    }

    /// <summary>
    /// Creates a configuration optimized for notification telemetry only.
    /// Enables comprehensive notification tracking while disabling other telemetry features.
    /// </summary>
    public static TelemetryOptions NotificationOnly()
    {
        return new TelemetryOptions
        {
            Enabled = true,
            CaptureMiddlewareDetails = false,
            CaptureHandlerDetails = false,
            CaptureExceptionDetails = true,
            EnableHealthChecks = false,
            PacketLevelTelemetryEnabled = false,
            EnableStreamingMetrics = false,
            CaptureNotificationHandlerDetails = true,
            CreateHandlerChildSpans = true,
            CaptureSubscriberMetrics = true,
            CaptureNotificationMiddlewareDetails = true,
            SensitiveDataPatterns = ["password", "token", "secret", "key", "auth"]
        };
    }

    /// <summary>
    /// Creates a configuration optimized for streaming telemetry only.
    /// Enables comprehensive streaming monitoring while disabling other telemetry features.
    /// </summary>
    public static TelemetryOptions StreamingOnly()
    {
        return new TelemetryOptions
        {
            Enabled = true,
            CaptureMiddlewareDetails = false,
            CaptureHandlerDetails = false,
            CaptureExceptionDetails = true,
            EnableHealthChecks = false,
            PacketLevelTelemetryEnabled = true,
            PacketTelemetryBatchSize = 10,
            EnableStreamingMetrics = true,
            CapturePacketSize = true,
            EnableStreamingPerformanceClassification = true,
            CaptureNotificationHandlerDetails = false,
            CreateHandlerChildSpans = false,
            CaptureSubscriberMetrics = false,
            CaptureNotificationMiddlewareDetails = false,
            SensitiveDataPatterns = ["password", "token", "secret"]
        };
    }

    #endregion
}