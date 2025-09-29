namespace OpenTelemetryExample.Domain.Entities;

/// <summary>
/// Entity representing a telemetry metric entry.
/// </summary>
public sealed class TelemetryMetric
{
    /// <summary>
    /// Gets or sets the unique identifier for the telemetry metric entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the type of the request (e.g., HTTP, gRPC).
    /// </summary>
    public string RequestType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the request.
    /// </summary>
    public string RequestName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category of the request (e.g., Command or Query).
    /// </summary>
    public string Category { get; set; } = string.Empty; // Command or Query

    /// <summary>
    /// Gets or sets the duration of the request in milliseconds.
    /// </summary>
    public double Duration { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the request was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the error message if the request failed; otherwise, null.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the metric was recorded.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets additional tags associated with the metric.
    /// </summary>
    public Dictionary<string, object> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the name of the handler that processed the request, if applicable.
    /// </summary>
    public string? HandlerName { get; set; }
}