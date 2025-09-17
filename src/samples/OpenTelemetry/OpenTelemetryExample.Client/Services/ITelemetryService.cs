using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Services;

public interface ITelemetryService
{
    Task<bool> CheckApiHealthAsync();
    Task<TelemetryHealthDto?> GetTelemetryHealthAsync();
    Task<object?> GetTelemetryMetricsAsync();
    Task<LiveMetricsDto?> GetLiveMetricsAsync();
    Task<RecentTracesDto?> GetRecentTracesAsync();
    Task<RecentActivitiesDto?> GetRecentActivitiesAsync();
    Task<bool> TestNotificationAsync();
    Task<bool> TestMiddlewareErrorAsync();
    Task<bool> TestMiddlewareValidationAsync();
    Task<object?> TestBasicConnectivityAsync();
}