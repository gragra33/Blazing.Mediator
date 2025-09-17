using OpenTelemetryExample.Client.Models;

namespace OpenTelemetryExample.Client.Services;

public interface ITelemetryService
{
    Task<bool> CheckApiHealthAsync();
    Task<TelemetryHealthDto?> GetTelemetryHealthAsync();
    Task<object?> GetTelemetryMetricsAsync();
    Task<bool> TestNotificationAsync();
    Task<bool> TestMiddlewareErrorAsync();
    Task<bool> TestMiddlewareValidationAsync();
}