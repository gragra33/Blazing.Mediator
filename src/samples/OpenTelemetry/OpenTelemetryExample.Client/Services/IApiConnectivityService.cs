namespace OpenTelemetryExample.Client.Services;

/// <summary>
/// Service for checking API connectivity and debugging connection issues.
/// </summary>
public interface IApiConnectivityService
{
    Task<bool> TestApiConnectivityAsync();
    Task<string> GetApiStatusAsync();
    Task<Dictionary<string, object>> GetConnectionDiagnosticsAsync();
}