using Microsoft.AspNetCore.Components;
using OpenTelemetryExample.Client.Components.Telemetry;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Pages;

/// <summary>
/// Main telemetry page that displays comprehensive OpenTelemetry monitoring data including
/// health status, live metrics, recent traces, recent logs, and activities.
/// </summary>
public partial class Telemetry : ComponentBase
{
    private bool? _apiHealthy;
    private bool _apiHealthLoading = true;
    private TelemetryHealthDto? _telemetryHealth;
    private bool _telemetryHealthLoading = true;
    private Timer? _refreshTimer;
    private bool _autoRefreshEnabled = true; // Auto-refresh enabled by default
    private const int _refreshIntervalSeconds = 30;
    private int _remainingSeconds = 30;
    private Timer? _countdownTimer;

    // Strongly-typed telemetry data
    private LiveMetricsDto? _liveMetrics;
    private RecentActivitiesDto? _recentActivities;

    // Debug data for the debug component
    private DebugInformationCard.DebugData _debugData = new();

    private int _recentTracesRefreshTrigger;
    private int _recentLogsRefreshTrigger;

    /// <summary>
    /// Initializes the telemetry page and sets up auto-refresh.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await RefreshTelemetryManual(); // Use manual refresh for initial load

        // Set up auto-refresh if enabled
        StartAutoRefresh();
    }

    /// <summary>
    /// Starts the auto-refresh timer if auto-refresh is enabled.
    /// </summary>
    private void StartAutoRefresh()
    {
        if (_autoRefreshEnabled && _refreshTimer == null)
        {
            _remainingSeconds = _refreshIntervalSeconds;
            
            // Main refresh timer
            _refreshTimer = new Timer(async void (_) =>
            {
                await InvokeAsync(async () =>
                {
                    await RefreshTelemetry(); // Auto-refresh excludes traces and logs
                    _remainingSeconds = _refreshIntervalSeconds; // Reset countdown
                    StateHasChanged();
                });
            }, null, TimeSpan.FromSeconds(_refreshIntervalSeconds), TimeSpan.FromSeconds(_refreshIntervalSeconds));
            
            // Countdown timer (updates every second)
            _countdownTimer = new Timer(async void (_) =>
            {
                await InvokeAsync(() =>
                {
                    if (_remainingSeconds > 0)
                    {
                        _remainingSeconds--;
                        StateHasChanged();
                    }
                });
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }
    }

    /// <summary>
    /// Stops the auto-refresh timer.
    /// </summary>
    private void StopAutoRefresh()
    {
        _refreshTimer?.Dispose();
        _refreshTimer = null;
        _countdownTimer?.Dispose();
        _countdownTimer = null;
    }

    /// <summary>
    /// Toggles the auto-refresh functionality on or off.
    /// </summary>
    private async Task ToggleAutoRefresh()
    {
        _autoRefreshEnabled = !_autoRefreshEnabled;
        
        if (_autoRefreshEnabled)
        {
            StartAutoRefresh();
        }
        else
        {
            StopAutoRefresh();
        }
        
        StateHasChanged();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Refreshes telemetry data automatically (excludes traces and logs to avoid performance impact).
    /// </summary>
    private async Task RefreshTelemetry()
    {
        _recentTracesRefreshTrigger++; // Force RecentTracesCard to refresh
        _recentLogsRefreshTrigger++; // Force RecentLogsCard to refresh
        await Task.WhenAll(
            RefreshApiHealth(),
            RefreshTelemetryHealth(),
            RefreshLiveMetrics(),
            RefreshRecentActivities()
        );
        // Update debug data
        UpdateDebugData();
    }

    /// <summary>
    /// Refreshes all telemetry data including traces and logs (used for manual refresh).
    /// </summary>
    private async Task RefreshTelemetryManual()
    {
        await Task.WhenAll(
            RefreshApiHealth(),
            RefreshTelemetryHealth(),
            RefreshLiveMetrics(),
            RefreshRecentActivities()
        );
        // Update debug data
        UpdateDebugData();
        _recentTracesRefreshTrigger++; // Force RecentTracesCard to refresh
        _recentLogsRefreshTrigger++; // Force RecentLogsCard to refresh
        
        // Reset countdown when manually refreshing
        if (_autoRefreshEnabled)
        {
            _remainingSeconds = _refreshIntervalSeconds;
        }
    }

    /// <summary>
    /// Refreshes the API health status.
    /// </summary>
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

    /// <summary>
    /// Refreshes the telemetry health information.
    /// </summary>
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

    /// <summary>
    /// Refreshes the live metrics data.
    /// </summary>
    private async Task RefreshLiveMetrics()
    {
        try
        {
            var result = await TelemetryService.GetLiveMetricsAsync();
            _liveMetrics = result;
        }
        catch (Exception)
        {
            _liveMetrics = null;
        }
    }



    /// <summary>
    /// Refreshes the recent activities data.
    /// </summary>
    private async Task RefreshRecentActivities()
    {
        try
        {
            var result = await TelemetryService.GetRecentActivitiesAsync();
            _recentActivities = result;
        }
        catch (Exception)
        {
            _recentActivities = null;
        }
    }

    /// <summary>
    /// Updates the debug data structure with current telemetry information.
    /// </summary>
    private void UpdateDebugData()
    {
        _debugData = new DebugInformationCard.DebugData
        {
            ApiHealthy = _apiHealthy,
            LiveMetrics = _liveMetrics,
            RecentTraces = null, // RecentTracesCard manages its own data
            RecentActivities = _recentActivities,
            TelemetryHealth = _telemetryHealth
        };
    }

    /// <summary>
    /// Handles changes to trace filters and updates the debug information.
    /// </summary>
    private Task OnTracesFiltersChanged()
    {
        UpdateDebugData();
        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes of the refresh timer when the component is disposed.
    /// </summary>
    public void Dispose()
    {
        StopAutoRefresh();
    }
}