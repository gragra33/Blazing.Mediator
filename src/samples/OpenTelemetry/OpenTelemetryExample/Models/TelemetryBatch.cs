using OpenTelemetryExample.Domain.Entities;

namespace OpenTelemetryExample.Models;

/// <summary>
/// Represents a batch of telemetry data for efficient database operations.
/// </summary>
internal sealed class TelemetryBatch
{
    /// <summary>
    /// Gets the collection of telemetry traces in this batch.
    /// </summary>
    public List<TelemetryTrace> Traces { get; } = new();
    
    /// <summary>
    /// Gets the collection of telemetry activities in this batch.
    /// </summary>
    public List<TelemetryActivity> Activities { get; } = new();
    
    /// <summary>
    /// Gets the collection of telemetry metrics in this batch.
    /// </summary>
    public List<TelemetryMetric> Metrics { get; } = new();
}