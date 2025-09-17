using System.Net.Http.Json;
using OpenTelemetryExample.Client.Models;

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

            var jsonContent = await response.Content.ReadAsStringAsync();
            logger.LogDebug("[<-] Telemetry health response: {Json}", jsonContent);
            
            var telemetryHealth = await response.Content.ReadFromJsonAsync<TelemetryHealthDto>();
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

    public async Task<bool> TestNotificationAsync()
    {
        try
        {
            logger.LogDebug("[->] Calling POST /notifications/test");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await httpClient.PostAsync("notifications/test", null, cts.Token);
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
            logger.LogError(ex, "[!] Error calling POST /notifications/test");
            return false;
        }
    }

    public async Task<bool> TestMiddlewareErrorAsync()
    {
        try
        {
            logger.LogDebug("[->] Calling POST /middleware/test-error");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await httpClient.PostAsync("middleware/test-error", null, cts.Token);
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
            logger.LogDebug("[->] Calling POST /middleware/test-validation");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await httpClient.PostAsync("middleware/test-validation", null, cts.Token);
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
}