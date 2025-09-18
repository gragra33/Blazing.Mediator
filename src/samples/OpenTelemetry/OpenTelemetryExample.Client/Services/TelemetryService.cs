using System.Net.Http.Json;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Services;

public sealed class TelemetryService(HttpClient httpClient, ILogger<TelemetryService> logger) : ITelemetryService
{
    public async Task<bool> CheckApiHealthAsync()
    {
        try
        {
            logger.LogDebug("[->] Calling GET /health");
            logger.LogDebug("[->] Using base address: {BaseAddress}", httpClient.BaseAddress);
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await httpClient.GetAsync("health", cts.Token);
            var isHealthy = response.IsSuccessStatusCode;
            
            logger.LogDebug("[<-] Health check result: {IsHealthy} (Status: {StatusCode})", 
                isHealthy, response.StatusCode);
            
            return isHealthy;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[!] Health check timed out - API server may not be running");
            return false;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "[!] HTTP error calling GET /health - API server may not be running at {BaseAddress}", 
                httpClient.BaseAddress);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] Unexpected error calling GET /health");
            return false;
        }
    }

    public async Task<TelemetryHealthDto?> GetTelemetryHealthAsync()
    {
        try
        {
            logger.LogDebug("[->] Calling GET /telemetry/health");
            logger.LogDebug("[->] Using base address: {BaseAddress}", httpClient.BaseAddress);
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await httpClient.GetAsync("telemetry/health", cts.Token);
            
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("[!] Telemetry health endpoint returned {StatusCode}", response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cts.Token);
            logger.LogDebug("[<-] Telemetry health response: {Json}", jsonContent);
            
            var telemetryHealth = await response.Content.ReadFromJsonAsync<TelemetryHealthDto>(cancellationToken: cts.Token);
            logger.LogDebug("[<-] Telemetry health: IsHealthy={IsHealthy}, IsEnabled={IsEnabled}", 
                telemetryHealth?.IsHealthy ?? false, telemetryHealth?.IsEnabled ?? false);
            
            return telemetryHealth;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[!] Telemetry health check timed out - API server may not be running");
            return null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "[!] HTTP error calling GET /telemetry/health - API server may not be running at {BaseAddress}", 
                httpClient.BaseAddress);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] Unexpected error calling GET /telemetry/health");
            return null;
        }
    }

    public async Task<object?> GetTelemetryMetricsAsync()
    {
        try
        {
            logger.LogDebug("[->] Calling GET /telemetry/metrics");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await httpClient.GetFromJsonAsync<object>("telemetry/metrics", cts.Token);
            logger.LogDebug("[<-] Retrieved telemetry metrics");
            return response;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[!] Telemetry metrics request timed out");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] Error calling GET /telemetry/metrics");
            return null;
        }
    }

    public async Task<LiveMetricsDto?> GetLiveMetricsAsync()
    {
        try
        {
            logger.LogDebug("[->] Calling GET /telemetry/live-metrics");
            logger.LogDebug("[->] Using base address: {BaseAddress}", httpClient.BaseAddress);
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await httpClient.GetAsync("telemetry/live-metrics", cts.Token);
            
            logger.LogDebug("[<-] Live metrics response status: {StatusCode}", response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                logger.LogWarning("[!] Live metrics endpoint returned {StatusCode}: {ErrorContent}", 
                    response.StatusCode, errorContent);
                return null;
            }
            
            // Return strongly-typed LiveMetricsDto
            var result = await response.Content.ReadFromJsonAsync<LiveMetricsDto>(cancellationToken: cts.Token);
            logger.LogDebug("[<-] Retrieved live telemetry metrics successfully");
            return result;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[!] Live telemetry metrics request timed out");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] Error calling GET /telemetry/live-metrics");
            return null;
        }
    }

    public async Task<RecentTracesDto?> GetRecentTracesAsync(int maxRecords = 10, bool filterMediatorOnly = false, bool filterExampleAppOnly = false, int timeWindowMinutes = 30)
    {
        try
        {
            var queryParams = new List<string>();
            
            // Always include maxRecords parameter 
            queryParams.Add($"maxRecords={Math.Max(maxRecords, 10)}");
            
            if (filterMediatorOnly)
                queryParams.Add("blazingMediatorOnly=true");
                
            if (filterExampleAppOnly)
                queryParams.Add("exampleAppOnly=true");
                
            if (timeWindowMinutes != 30)
                queryParams.Add($"timeWindowMinutes={timeWindowMinutes}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"telemetry/traces{queryString}";
            
            logger.LogDebug("[->] Calling GET /{Endpoint}", endpoint);
            logger.LogDebug("[->] Using base address: {BaseAddress}", httpClient.BaseAddress);
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await httpClient.GetAsync(endpoint, cts.Token);
            
            logger.LogDebug("[<-] Recent traces response status: {StatusCode}", response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                logger.LogWarning("[!] Recent traces endpoint returned {StatusCode}: {ErrorContent}", 
                    response.StatusCode, errorContent);
                return null;
            }
            
            // Return strongly-typed RecentTracesDto
            var result = await response.Content.ReadFromJsonAsync<RecentTracesDto>(cancellationToken: cts.Token);
            logger.LogDebug("[<-] Retrieved recent traces successfully (maxRecords: {MaxRecords}, filter: {Filter})", 
                maxRecords, filterMediatorOnly);
            return result;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[!] Recent traces request timed out");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] Error calling GET /telemetry/traces");
            return null;
        }
    }

    public async Task<RecentActivitiesDto?> GetRecentActivitiesAsync()
    {
        try
        {
            logger.LogDebug("[->] Calling GET /telemetry/activities");
            logger.LogDebug("[->] Using base address: {BaseAddress}", httpClient.BaseAddress);
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await httpClient.GetAsync("telemetry/activities", cts.Token);
            
            logger.LogDebug("[<-] Recent activities response status: {StatusCode}", response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                logger.LogWarning("[!] Recent activities endpoint returned {StatusCode}: {ErrorContent}", 
                    response.StatusCode, errorContent);
                return null;
            }
            
            // Return strongly-typed RecentActivitiesDto
            var result = await response.Content.ReadFromJsonAsync<RecentActivitiesDto>(cancellationToken: cts.Token);
            logger.LogDebug("[<-] Retrieved recent activities successfully");
            return result;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[!] Recent activities request timed out");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] Error calling GET /telemetry/activities");
            return null;
        }
    }

    public async Task<bool> TestNotificationAsync()
    {
        try
        {
            logger.LogDebug("[->] Calling POST /testing/notifications");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await httpClient.PostAsync("testing/notifications", null, cts.Token);
            var success = response.IsSuccessStatusCode;
            logger.LogDebug("[<-] Test notification result: {Success}", success);
            return success;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[!] Test notification request timed out");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] Error calling POST /testing/notifications");
            return false;
        }
    }

    public async Task<bool> TestMiddlewareErrorAsync()
    {
        try
        {
            logger.LogDebug("[->] Calling POST /testing/middleware/error");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await httpClient.PostAsync("testing/middleware/error", null, cts.Token);
            logger.LogDebug("[<-] Test middleware error completed - this should generate telemetry");
            return false; // This should always fail, but that's expected
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[!] Test middleware error request timed out");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "[<-] Middleware error test threw exception (expected) - telemetry generated");
            return true; // Exception expected, telemetry should be generated
        }
    }

    public async Task<bool> TestMiddlewareValidationAsync()
    {
        try
        {
            logger.LogDebug("[->] Calling POST /testing/middleware/validation");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await httpClient.PostAsync("testing/middleware/validation", null, cts.Token);
            logger.LogDebug("[<-] Test middleware validation completed - this should generate telemetry");
            return false; // This should always fail validation, but that's expected
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[!] Test middleware validation request timed out");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "[<-] Middleware validation test threw exception (expected) - telemetry generated");
            return true; // Exception expected, telemetry should be generated
        }
    }

    public async Task<object?> TestBasicConnectivityAsync()
    {
        try
        {
            logger.LogDebug("[->] Testing basic connectivity with GET /telemetry/test");
            logger.LogDebug("[->] Using base address: {BaseAddress}", httpClient.BaseAddress);
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await httpClient.GetAsync("telemetry/test", cts.Token);
            
            logger.LogDebug("[<-] Basic connectivity test response status: {StatusCode}", response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                logger.LogWarning("[!] Basic connectivity test returned {StatusCode}: {ErrorContent}", 
                    response.StatusCode, errorContent);
                return new { Success = false, response.StatusCode, Error = errorContent };
            }
            
            var result = await response.Content.ReadFromJsonAsync<object>(cancellationToken: cts.Token);
            logger.LogInformation("[+] Basic connectivity test successful!");
            return result;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[!] Basic connectivity test timed out");
            return new { Success = false, Error = "Request timed out" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] Error in basic connectivity test");
            return new { Success = false, Error = ex.Message };
        }
    }
}