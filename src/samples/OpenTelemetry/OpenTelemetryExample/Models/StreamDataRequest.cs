namespace OpenTelemetryExample.Models;

/// <summary>
/// Request model for stream-data endpoint.
/// </summary>
public class StreamDataRequest
{
    /// <summary>
    /// Gets or sets the number of data items to stream.
    /// </summary>
    public int Count { get; set; } = 50;

    /// <summary>
    /// Gets or sets the delay in milliseconds between each streamed item.
    /// </summary>
    public int DelayMs { get; set; } = 100;
}