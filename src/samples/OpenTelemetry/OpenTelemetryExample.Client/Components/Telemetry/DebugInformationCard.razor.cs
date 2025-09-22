using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpenTelemetryExample.Client.Services;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Components.Telemetry;

/// <summary>
/// A Blazor component that displays debug and telemetry information, and provides tools for testing API endpoints and connectivity.
/// </summary>
public partial class DebugInformationCard : ComponentBase
{
    /// <summary>
    /// Gets or sets the telemetry service used for connectivity and health checks.
    /// </summary>
    [Inject] public ITelemetryService TelemetryService { get; set; } = null!;

    /// <summary>
    /// Gets or sets the JavaScript runtime for invoking JS interop calls.
    /// </summary>
    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;

    /// <summary>
    /// Gets or sets the HTTP client for making API requests.
    /// </summary>
    [Inject] public HttpClient HttpClient { get; set; } = null!;

    /// <summary>
    /// Gets or sets the data source containing debug and telemetry information.
    /// </summary>
    [Parameter] public DebugData DataSource { get; set; } = new();

    private bool _testing;

    /// <summary>
    /// Represents the data model for debug and telemetry information.
    /// </summary>
    public class DebugData
    {
        /// <summary>
        /// Gets or sets a value indicating whether the API is healthy.
        /// </summary>
        public bool? ApiHealthy { get; set; }

        /// <summary>
        /// Gets or sets the live metrics data.
        /// </summary>
        public LiveMetricsDto? LiveMetrics { get; set; }

        /// <summary>
        /// Gets or sets the recent traces data.
        /// </summary>
        public RecentTracesDto? RecentTraces { get; set; }

        /// <summary>
        /// Gets or sets the recent activities data.
        /// </summary>
        public RecentActivitiesDto? RecentActivities { get; set; }

        /// <summary>
        /// Gets or sets the telemetry health data.
        /// </summary>
        public TelemetryHealthDto? TelemetryHealth { get; set; }
    }

    /// <summary>
    /// Gets the count of command metrics from the live metrics data.
    /// </summary>
    /// <returns>The number of commands, or 0 if not available.</returns>
    private int GetCommandsCount() => DataSource.LiveMetrics?.Commands.Count ?? 0;

    /// <summary>
    /// Gets the count of query metrics from the live metrics data.
    /// </summary>
    /// <returns>The number of queries, or 0 if not available.</returns>
    private int GetQueriesCount() => DataSource.LiveMetrics?.Queries.Count ?? 0;

    /// <summary>
    /// Gets the count of traces from the recent traces data.
    /// </summary>
    /// <returns>The number of traces in the timeframe, or 0 if not available.</returns>
    private int GetTracesCount() => DataSource.RecentTraces?.TotalTracesInTimeframe ?? 0;

    /// <summary>
    /// Shows a modal dialog with the raw telemetry and debug data in JSON format.
    /// </summary>
    private async Task ShowRawDataModal()
    {
        try
        {
            var rawData = new
            {
                DataSource.LiveMetrics,
                DataSource.RecentTraces,
                DataSource.RecentActivities,
                DataSource.ApiHealthy,
                DataSource.TelemetryHealth
            };
            var jsonString = System.Text.Json.JsonSerializer.Serialize(rawData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await JSRuntime.InvokeVoidAsync("alert", $"Raw Data:\n\n{jsonString}");
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Error showing raw data: {ex.Message}");
        }
    }

    /// <summary>
    /// Tests several API endpoints and displays the results in a modal dialog.
    /// </summary>
    private async Task TestApiEndpoints()
    {
        _testing = true;
        var results = new List<string>();
        try
        {
            var endpoints = new[]
            {
                ("Health", "/health"),
                ("Telemetry Health", "/telemetry/health"),
                ("Telemetry Metrics", "/telemetry/metrics"),
                ("Live Metrics", "/telemetry/live-metrics"),
                ("Recent Traces", "/telemetry/traces"),
                ("Recent Activities", "/telemetry/activities")
            };
            foreach (var (name, endpoint) in endpoints)
            {
                try
                {
                    var response = await HttpClient.GetAsync(endpoint);
                    results.Add($"{name}: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    results.Add($"{name}: ERROR - {ex.Message}");
                }
            }
            await JSRuntime.InvokeVoidAsync("alert", $"API Endpoint Test Results:\n\n{string.Join("\n", results)}");
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Error testing endpoints: {ex.Message}");
        }
        finally
        {
            _testing = false;
        }
    }

    /// <summary>
    /// Makes a direct API call to the health endpoint and displays the result.
    /// </summary>
    private async Task TestDirectApiCall()
    {
        _testing = true;
        try
        {
            var response = await HttpClient.GetAsync("/health");
            var content = await response.Content.ReadAsStringAsync();
            await JSRuntime.InvokeVoidAsync("alert", $"Direct API Call Result:\n\nStatus: {response.StatusCode}\nContent: {content}");
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Error making direct API call: {ex.Message}");
        }
        finally
        {
            _testing = false;
        }
    }

    /// <summary>
    /// Tests basic connectivity to the telemetry service and displays the result.
    /// </summary>
    private async Task TestBasicConnectivity()
    {
        _testing = true;
        try
        {
            var result = await TelemetryService.TestBasicConnectivityAsync();
            var jsonString = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await JSRuntime.InvokeVoidAsync("alert", $"Basic Connectivity Test Result:\n\n{jsonString}");
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Error testing basic connectivity: {ex.Message}");
        }
        finally
        {
            _testing = false;
        }
    }
}
