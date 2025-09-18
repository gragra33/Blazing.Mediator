using Microsoft.AspNetCore.Components;
using OpenTelemetryExample.Shared.Models;
using OpenTelemetryExample.Client.Components;

namespace OpenTelemetryExample.Client.Pages;

public partial class Telemetry : ComponentBase
{
    private bool? _apiHealthy;
    private bool _apiHealthLoading = true;
    private TelemetryHealthDto? _telemetryHealth;
    private bool _telemetryHealthLoading = true;
    private Timer? _refreshTimer;
        
    // Strongly-typed telemetry data
    private LiveMetricsDto? _liveMetrics;
    private RecentTracesDto? _recentTraces;
    private RecentActivitiesDto? _recentActivities;

    // Debug data for the debug component
    private DebugInformationCard.DebugData _debugData = new();

    private int _recentTracesRefreshTrigger;

    protected override async Task OnInitializedAsync()
    {
        await RefreshTelemetryManual(); // Use manual refresh for initial load
            
        // Set up auto-refresh every 30 seconds
        _refreshTimer = new Timer(async void (_) =>
        {
            await InvokeAsync(async () =>
            {
                await RefreshTelemetry(); // Auto-refresh excludes traces
                StateHasChanged();
            });
        }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    private async Task RefreshTelemetry()
    {
        _recentTracesRefreshTrigger++; // Force RecentTracesCard to refresh
        await Task.WhenAll(
            RefreshApiHealth(), 
            RefreshTelemetryHealth(), 
            RefreshLiveMetrics(),
            RefreshRecentActivities(),
            RefreshRecentTraces()
        );
        // Update debug data
        UpdateDebugData();
    }

    private async Task RefreshTelemetryManual()
    {
        await Task.WhenAll(
            RefreshApiHealth(), 
            RefreshTelemetryHealth(), 
            RefreshLiveMetrics(),
            RefreshRecentActivities(),
            RefreshRecentTraces()
        );
        // Update debug data
        UpdateDebugData();
        _recentTracesRefreshTrigger++; // Force RecentTracesCard to refresh
    }

    private async Task RefreshApiHealth()
    {
        _apiHealthLoading = true;
        try
        {
            _apiHealthy = await TelemetryService.CheckApiHealthAsync();
        }
        catch
        {
            _apiHealthy = false;
        }
        finally
        {
            _apiHealthLoading = false;
        }
    }

    private async Task RefreshTelemetryHealth()
    {
        _telemetryHealthLoading = true;
        try
        {
            _telemetryHealth = await TelemetryService.GetTelemetryHealthAsync();
        }
        catch
        {
            _telemetryHealth = null;
        }
        finally
        {
            _telemetryHealthLoading = false;
        }
    }

    private async Task RefreshLiveMetrics()
    {
        Console.WriteLine("[DEBUG] Starting RefreshLiveMetrics...");
        try
        {
            var result = await TelemetryService.GetLiveMetricsAsync();
            Console.WriteLine($"[DEBUG] GetLiveMetricsAsync returned: {(result != null ? "data" : "null")}");
                
            _liveMetrics = result;

            Console.WriteLine(_liveMetrics != null
                ? $"[DEBUG] Live metrics loaded successfully: RequestCount={_liveMetrics.Metrics.RequestCount}"
                : "[DEBUG] GetLiveMetricsAsync returned null");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to refresh live metrics: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            _liveMetrics = null;
        }
    }

    private async Task RefreshRecentTraces()
    {
        Console.WriteLine("[DEBUG] Starting RefreshRecentTraces...");
        try
        {
            var result = await TelemetryService.GetRecentTracesAsync();
            Console.WriteLine($"[DEBUG] GetRecentTracesAsync returned: {(result != null ? "data" : "null")}");
                
            _recentTraces = result;

            Console.WriteLine(_recentTraces != null
                ? $"[DEBUG] Recent traces loaded successfully: {_recentTraces.TotalTracesInTimeframe} traces"
                : "[DEBUG] GetRecentTracesAsync returned null");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to refresh recent traces: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            _recentTraces = null;
        }
    }

    private async Task RefreshRecentActivities()
    {
        Console.WriteLine("[DEBUG] Starting RefreshRecentActivities...");
        try
        {
            var result = await TelemetryService.GetRecentActivitiesAsync();
            Console.WriteLine($"[DEBUG] GetRecentActivitiesAsync returned: {(result != null ? "data" : "null")}");
                
            _recentActivities = result;

            Console.WriteLine(_recentActivities != null
                ? $"[DEBUG] Recent activities loaded successfully: {_recentActivities.Activities.Count} activities"
                : "[DEBUG] GetRecentActivitiesAsync returned null");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to refresh recent activities: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            _recentActivities = null;
        }
    }

    private void UpdateDebugData()
    {
        _debugData = new DebugInformationCard.DebugData
        {
            ApiHealthy = _apiHealthy,
            LiveMetrics = _liveMetrics,
            RecentTraces = _recentTraces,
            RecentActivities = _recentActivities,
            TelemetryHealth = _telemetryHealth
        };
    }

    private async Task OnTracesFiltersChanged()
    {
        UpdateDebugData();
        StateHasChanged();
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
    }
}