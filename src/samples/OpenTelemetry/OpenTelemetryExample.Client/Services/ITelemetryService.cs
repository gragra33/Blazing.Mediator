using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Services;

public interface ITelemetryService
{
    Task<bool> CheckApiHealthAsync();
    Task<TelemetryHealthDto?> GetTelemetryHealthAsync();
    Task<object?> GetTelemetryMetricsAsync();
    Task<LiveMetricsDto?> GetLiveMetricsAsync();
    Task<RecentTracesDto?> GetRecentTracesAsync(int maxRecords = 10, bool filterMediatorOnly = false, bool filterExampleAppOnly = false, int timeWindowMinutes = 30, int page = 1, int pageSize = 10);
    Task<GroupedTracesDto?> GetGroupedTracesAsync(int maxRecords = 10, bool filterMediatorOnly = false, bool filterExampleAppOnly = false, int timeWindowMinutes = 30, bool hidePackets = false, int page = 1, int pageSize = 10);
    Task<RecentActivitiesDto?> GetRecentActivitiesAsync();
    Task<RecentLogsDto?> GetRecentLogsAsync(int timeWindowMinutes = 30, bool appOnly = false, bool mediatorOnly = false, bool errorsOnly = false, string? minLogLevel = null, string? searchText = null, int page = 1, int pageSize = 20);
    Task<LogDto?> GetLogByIdAsync(int id);
    Task<LogSummary?> GetLogsSummaryAsync(int timeWindowMinutes = 30);
    Task<bool> TestNotificationAsync();
    Task<bool> TestMiddlewareErrorAsync();
    Task<bool> TestMiddlewareValidationAsync();
    Task<object?> TestBasicConnectivityAsync();
}