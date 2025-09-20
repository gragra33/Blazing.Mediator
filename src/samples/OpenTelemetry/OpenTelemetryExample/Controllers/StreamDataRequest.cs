namespace OpenTelemetryExample.Controllers;

/// <summary>
/// Request model for stream-data endpoint.
/// </summary>
public class StreamDataRequest
{
    public int Count { get; set; } = 50;
    public int DelayMs { get; set; } = 100;
}