using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpenTelemetryExample.Shared.Models;
using OpenTelemetryExample.Client.Services;

namespace OpenTelemetryExample.Client.Components;

public partial class DebugInformationCard : ComponentBase
{
    [Inject] public ITelemetryService TelemetryService { get; set; } = null!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] public HttpClient HttpClient { get; set; } = null!;

    [Parameter] public DebugData DataSource { get; set; } = new();
    private bool _testing;

    public class DebugData
    {
        public bool? ApiHealthy { get; set; }
        public LiveMetricsDto? LiveMetrics { get; set; }
        public RecentTracesDto? RecentTraces { get; set; }
        public RecentActivitiesDto? RecentActivities { get; set; }
        public TelemetryHealthDto? TelemetryHealth { get; set; }
    }

    private int GetCommandsCount() => DataSource.LiveMetrics?.Commands.Count ?? 0;
    private int GetQueriesCount() => DataSource.LiveMetrics?.Queries.Count ?? 0;
    private int GetTracesCount() => DataSource.RecentTraces?.TotalTracesInTimeframe ?? 0;

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