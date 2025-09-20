using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpenTelemetryExample.Client.Components.Shared;
using OpenTelemetryExample.Client.Services;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Components.Telemetry;

/// <summary>
/// Display mode for the traces card
/// </summary>
public enum DisplayMode
{
    Raw,
    Grouped
}

/// <summary>
/// Component for displaying recent OpenTelemetry traces with filtering, pagination, and detailed view capabilities.
/// Provides real-time trace monitoring and analysis functionality.
/// </summary>
public partial class RecentTracesCard : ComponentBase
{
    [Inject] public ITelemetryService TelemetryService { get; set; } = null!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;

    /// <summary>
    /// Gets or sets the data source for recent traces.
    /// </summary>
    [Parameter] public RecentTracesDto? DataSource { get; set; }

    /// <summary>
    /// Callback invoked when the filter parameters are changed.
    /// </summary>
    [Parameter] public EventCallback OnFiltersChanged { get; set; }

    /// <summary>
    /// Gets or sets the refresh trigger identifier.
    /// </summary>
    [Parameter] public int RefreshTrigger { get; set; }

    private bool _mediatorOnly;
    private bool _appOnly;
    private bool _hidePackets;
    private int _timeWindowMinutes = 30;
    private TraceDto? _selectedTrace;
    private bool _isLoading;
    private int _lastRefreshTrigger;
    private DisplayMode _displayMode = DisplayMode.Raw;
    private TraceDetailsModal? _traceDetailsModal;
    
    // Pagination state
    private int _currentPage = 1;
    private int _pageSize = 10;
    
    // Grouped mode data source
    private GroupedTracesDto? GroupedDataSource { get; set; }

    /// <summary>
    /// Gets the current pagination info based on display mode.
    /// </summary>
    private PaginationInfo? CurrentPagination => _displayMode == DisplayMode.Raw 
        ? DataSource?.Pagination 
        : GroupedDataSource?.Pagination;

    /// <summary>
    /// Initializes the component and loads initial trace data.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await RefreshTraces();
    }

    /// <summary>
    /// Handles parameter changes and refreshes traces when triggered.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        if (_lastRefreshTrigger != RefreshTrigger)
        {
            _lastRefreshTrigger = RefreshTrigger;
            await RefreshTraces();
        }
    }

    /// <summary>
    /// Retrieves the current traces from the data source.
    /// </summary>
    /// <returns>Array of trace DTOs.</returns>
    private TraceDto[] GetTraces()
    {
        try
        {
            if (DataSource != null)
            {
                var traces = DataSource.Traces.ToArray();
                return traces;
            }
            return [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Gets the trace groups for grouped display mode.
    /// </summary>
    /// <returns>Array of trace group DTOs.</returns>
    private TraceGroupDto[] GetTraceGroups()
    {
        return GroupedDataSource?.TraceGroups?.ToArray() ?? [];
    }

    /// <summary>
    /// Sets the display mode and refreshes the traces.
    /// </summary>
    /// <param name="mode">The display mode to set.</param>
    private async Task SetDisplayMode(DisplayMode mode)
    {
        if (_displayMode != mode)
        {
            _displayMode = mode;
            _currentPage = 1; // Reset to first page when changing display mode
            await RefreshTraces();
        }
    }

    /// <summary>
    /// Handles the hide packets filter change event.
    /// </summary>
    private async Task OnHidePacketsFilterChanged()
    {
        _currentPage = 1; // Reset to first page when changing filters
        await RefreshTraces();
        await OnFiltersChanged.InvokeAsync();
    }

    /// <summary>
    /// Handles page size changes.
    /// </summary>
    /// <param name="newPageSize">The new page size.</param>
    private async Task OnPageSizeChanged(int newPageSize)
    {
        // Validate page size range: minimum 10, maximum 100
        _pageSize = Math.Max(10, Math.Min(newPageSize, 100));
        _currentPage = 1; // Reset to first page when changing page size
        await RefreshTraces();
    }

    /// <summary>
    /// Navigates to the specified page.
    /// </summary>
    /// <param name="page">The page number to navigate to.</param>
    private async Task GoToPage(int page)
    {
        if (page < 1 || (CurrentPagination != null && page > CurrentPagination.TotalPages))
            return;

        _currentPage = page;
        await RefreshTraces();
    }

    /// <summary>
    /// Navigates to the previous page.
    /// </summary>
    private async Task PreviousPage()
    {
        if (CurrentPagination?.HasPreviousPage == true)
        {
            await GoToPage(_currentPage - 1);
        }
    }

    /// <summary>
    /// Navigates to the next page.
    /// </summary>
    private async Task NextPage()
    {
        if (CurrentPagination?.HasNextPage == true)
        {
            await GoToPage(_currentPage + 1);
        }
    }

    /// <summary>
    /// Navigates to the first page.
    /// </summary>
    private async Task FirstPage()
    {
        await GoToPage(1);
    }

    /// <summary>
    /// Navigates to the last page.
    /// </summary>
    private async Task LastPage()
    {
        if (CurrentPagination != null)
        {
            await GoToPage(CurrentPagination.TotalPages);
        }
    }

    /// <summary>
    /// Expands all trace groups in grouped display mode.
    /// </summary>
    private void ExpandAllGroups()
    {
        if (GroupedDataSource?.TraceGroups != null)
        {
            foreach (var group in GroupedDataSource.TraceGroups)
            {
                group.IsExpanded = true;
            }
            StateHasChanged();
        }
    }

    /// <summary>
    /// Collapses all trace groups in grouped display mode.
    /// </summary>
    private void CollapseAllGroups()
    {
        if (GroupedDataSource?.TraceGroups != null)
        {
            foreach (var group in GroupedDataSource.TraceGroups)
            {
                group.IsExpanded = false;
            }
            StateHasChanged();
        }
    }

    /// <summary>
    /// Gets the CSS row class based on trace type.
    /// </summary>
    /// <param name="trace">The trace to get the row class for.</param>
    /// <returns>CSS class name for the table row.</returns>
    private string GetRowClass(TraceDto trace) =>
        trace.IsMediatorTrace
            ? "table-info"
            : trace.IsAppTrace
                ? "table-success"
                : string.Empty;

    /// <summary>
    /// Gets the CSS badge class for the trace source.
    /// </summary>
    /// <param name="source">The trace source name.</param>
    /// <returns>CSS class for the source badge.</returns>
    private string GetSourceBadgeClass(string source)
    {
        return source.ToLower() switch
        {
            "blazing.mediator" => "bg-primary text-white",
            "aspnetcore" => "bg-secondary text-white",
            "asp.net core" => "bg-secondary text-white",
            "entityframework" => "bg-warning text-dark",
            "entity framework" => "bg-warning text-dark",
            "httpclient" => "bg-info text-white",
            "http client" => "bg-info text-white",
            "system" => "bg-light text-dark",
            _ => "bg-light text-dark"
        };
    }

    /// <summary>
    /// Gets the CSS badge class for the trace status.
    /// </summary>
    /// <param name="status">The trace status.</param>
    /// <returns>CSS class for the status badge.</returns>
    private string GetStatusBadgeClass(string status)
    {
        return status.ToLower() switch
        {
            "success" => "bg-success text-white",
            "error" => "bg-danger text-white",
            "failed" => "bg-danger text-white",
            "unset" => "bg-secondary text-white",
            _ => "bg-secondary text-white"
        };
    }

    /// <summary>
    /// Gets a user-friendly display name for the trace source.
    /// </summary>
    /// <param name="source">The original source name.</param>
    /// <returns>Display-friendly source name.</returns>
    private string GetSourceDisplayName(string source)
    {
        if (string.IsNullOrEmpty(source))
            return "Unknown";
        return source switch
        {
            "Blazing.Mediator" => "Mediator",
            "ASP.NET Core" => "AspNetCore",
            "Entity Framework" => "EF Core",
            "HTTP Client" => "HTTP",
            _ => source
        };
    }

    /// <summary>
    /// Shortens a trace ID for display purposes.
    /// </summary>
    /// <param name="traceId">The full trace ID.</param>
    /// <returns>Shortened trace ID with ellipsis if needed.</returns>
    private string ShortenTraceId(string traceId) => traceId.Length > 8 ? traceId[..8] + "..." : traceId;

    /// <summary>
    /// Gets a human-readable label for the current time window.
    /// </summary>
    /// <returns>Time window description.</returns>
    private string GetAgeLabel()
    {
        return _timeWindowMinutes switch
        {
            1 => "1 minute",
            5 => "5 minutes",
            10 => "10 minutes",
            30 => "30 minutes",
            60 => "1 hour",
            _ => $"{_timeWindowMinutes} minutes"
        };
    }

    /// <summary>
    /// Updates the time window filter and refreshes traces.
    /// </summary>
    /// <param name="minutes">The new time window in minutes.</param>
    private async Task UpdateAge(int minutes)
    {
        _timeWindowMinutes = minutes;
        _currentPage = 1; // Reset to first page when changing filters
        await RefreshTraces();
    }

    /// <summary>
    /// Handles changes to the application filter.
    /// </summary>
    private async Task OnAppFilterChanged() 
    {
        _currentPage = 1; // Reset to first page when changing filters
        await RefreshTraces();
    }

    /// <summary>
    /// Handles changes to the mediator filter.
    /// </summary>
    private async Task OnMediatorFilterChanged() 
    {
        _currentPage = 1; // Reset to first page when changing filters
        await RefreshTraces();
    }

    /// <summary>
    /// Refreshes the trace data from the telemetry service.
    /// </summary>
    private async Task RefreshTraces()
    {
        try
        {
            _isLoading = true;
            await InvokeAsync(StateHasChanged);
            
            if (_displayMode == DisplayMode.Raw)
            {
                DataSource = await TelemetryService.GetRecentTracesAsync(_pageSize, _mediatorOnly, _appOnly, _timeWindowMinutes, _currentPage, _pageSize);
                GroupedDataSource = null;
            }
            else
            {
                GroupedDataSource = await TelemetryService.GetGroupedTracesAsync(_pageSize, _mediatorOnly, _appOnly, _timeWindowMinutes, _hidePackets, _currentPage, _pageSize);
                DataSource = null;
            }
            
            if (OnFiltersChanged.HasDelegate)
            {
                await InvokeAsync(async () => await OnFiltersChanged.InvokeAsync());
            }
        }
        catch
        {
            // Error handling - data source remains unchanged
        }
        finally
        {
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Opens the trace details modal for the specified trace.
    /// </summary>
    /// <param name="trace">The trace to display details for.</param>
    private async Task ViewTraceDetails(TraceDto trace)
    {
        if (_traceDetailsModal != null)
        {
            await _traceDetailsModal.ShowTraceDetailsAsync(trace);
        }
    }

    /// <summary>
    /// Formats the middleware pipeline string for display.
    /// </summary>
    /// <param name="pipeline">The raw pipeline string.</param>
    /// <returns>Formatted middleware pipeline string.</returns>
    private string FormatMiddlewarePipeline(string? pipeline)
    {
        if (string.IsNullOrEmpty(pipeline))
            return string.Empty;
        var middlewareNames = pipeline.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(name => name.Trim())
            .Where(name => !string.IsNullOrEmpty(name));
        return string.Join(", ", middlewareNames);
    }
}
