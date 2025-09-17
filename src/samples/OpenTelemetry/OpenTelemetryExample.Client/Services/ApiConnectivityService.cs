namespace OpenTelemetryExample.Client.Services;

/// <summary>
/// Implementation of API connectivity service for debugging connection issues.
/// </summary>
public sealed class ApiConnectivityService(HttpClient httpClient, ILogger<ApiConnectivityService> logger)
    : IApiConnectivityService
{
    /// <summary>
    /// Tests basic connectivity to the API server.
    /// </summary>
    public async Task<bool> TestApiConnectivityAsync()
    {
        try
        {
            logger.LogInformation("[->] Testing API connectivity to: {BaseAddress}", httpClient.BaseAddress);
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            // Try a simple GET request to the health endpoint
            var response = await httpClient.GetAsync("health", cts.Token);
            
            var isConnected = response.IsSuccessStatusCode;
            logger.LogInformation("[<-] API connectivity test result: {IsConnected} (Status: {StatusCode})", 
                isConnected, response.StatusCode);
            
            return isConnected;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[!] API connectivity test timed out");
            return false;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "[!] API connectivity test failed with HTTP error");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] API connectivity test failed with unexpected error");
            return false;
        }
    }

    /// <summary>
    /// Gets the current API status with detailed information.
    /// </summary>
    public async Task<string> GetApiStatusAsync()
    {
        try
        {
            var isConnected = await TestApiConnectivityAsync();
            
            if (isConnected)
            {
                // Try to get more detailed status
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    var response = await httpClient.GetAsync("debug/mediator", cts.Token);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync(cts.Token);
                        return $"API is accessible and responsive. Debug info: {content}";
                    }

                    return $"API is accessible but debug endpoint returned: {response.StatusCode}";
                }
                catch
                {
                    return "API is accessible but debug endpoint is not available";
                }
            }

            return $"API is not accessible at: {httpClient.BaseAddress}";
        }
        catch (Exception ex)
        {
            return $"Failed to get API status: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets comprehensive connection diagnostics for troubleshooting.
    /// </summary>
    public async Task<Dictionary<string, object>> GetConnectionDiagnosticsAsync()
    {
        var diagnostics = new Dictionary<string, object>
        {
            ["BaseAddress"] = httpClient.BaseAddress?.ToString() ?? "Not set",
            ["Timeout"] = httpClient.Timeout.ToString(),
            ["Timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
        };

        try
        {
            // Test basic connectivity
            var connectivityResult = await TestApiConnectivityAsync();
            diagnostics["BasicConnectivity"] = connectivityResult;

            // Test specific endpoints
            var endpoints = new[] { "health", "debug/mediator", "telemetry/health", "api/users" };
            var endpointResults = new Dictionary<string, object>();

            foreach (var endpoint in endpoints)
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    var response = await httpClient.GetAsync(endpoint, cts.Token);
                    
                    endpointResults[endpoint] = new
                    {
                        StatusCode = (int)response.StatusCode,
                        StatusText = response.StatusCode.ToString(),
                        IsSuccess = response.IsSuccessStatusCode
                    };
                }
                catch (Exception ex)
                {
                    endpointResults[endpoint] = new
                    {
                        Error = ex.GetType().Name, ex.Message
                    };
                }
            }

            diagnostics["EndpointTests"] = endpointResults;
        }
        catch (Exception ex)
        {
            diagnostics["DiagnosticsError"] = ex.Message;
        }

        return diagnostics;
    }
}