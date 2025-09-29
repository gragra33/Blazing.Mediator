namespace OpenTelemetryExample.Hubs;

/// <summary>
/// Request model for SignalR streaming.
/// </summary>
public class StreamingRequest
{
    /// <summary>
    /// Gets or sets the total number of items to stream.
    /// </summary>
    public int Count { get; set; } = 50;

    /// <summary>
    /// Gets or sets the delay in milliseconds between streamed items.
    /// </summary>
    public int DelayMs { get; set; } = 100;

    /// <summary>
    /// Gets or sets the number of items to send in each batch.
    /// </summary>
    public int BatchSize { get; set; } = 5;
}