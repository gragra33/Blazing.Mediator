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
}