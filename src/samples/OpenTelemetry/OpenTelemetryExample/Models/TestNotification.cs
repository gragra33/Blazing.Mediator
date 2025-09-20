using Blazing.Mediator;

namespace OpenTelemetryExample.Models;

/// <summary>
/// Test notification for telemetry testing.
/// </summary>
public sealed class TestNotification : INotification
{
    /// <summary>
    /// Gets or sets the test message for telemetry testing.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}