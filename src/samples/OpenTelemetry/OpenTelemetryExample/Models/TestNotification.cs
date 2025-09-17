using Blazing.Mediator;

namespace OpenTelemetryExample.Models;

/// <summary>
/// Test notification for telemetry testing.
/// </summary>
public sealed class TestNotification : INotification
{
    public string Message { get; set; } = string.Empty;
}