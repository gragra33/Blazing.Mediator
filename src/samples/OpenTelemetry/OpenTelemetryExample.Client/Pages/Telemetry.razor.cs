using Microsoft.AspNetCore.Components;
using OpenTelemetryExample.Client.Components.Telemetry;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Pages;

/// <summary>
/// Main telemetry page that displays comprehensive OpenTelemetry monitoring data including
/// health status, live metrics, recent traces, and activities.
/// </summary>
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

    /// <summary>
    /// Initializes the telemetry page and sets up auto-refresh.
    /// </summary>
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

    /// <summary>
    /// Refreshes telemetry data automatically (excludes traces to avoid performance impact).
    /// </summary>
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

    /// <summary>
    /// Refreshes all telemetry data including traces (used for manual refresh).
    /// </summary>
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
        catch (Exception ex)
        {
            _liveMetrics = null;
        }
    }

    /// <summary>
    /// Refreshes the recent traces data.
    /// </summary>
    private async Task RefreshRecentTraces()
    {
        try
        {
            var result = await TelemetryService.GetRecentTracesAsync();
            _recentTraces = result;
        }
        catch (Exception ex)
        {
            _recentTraces = null;
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
        catch (Exception ex)
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
            RecentTraces = _recentTraces,
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
        _refreshTimer?.Dispose();
    }
}