using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpenTelemetryExample.Client.Components.Shared;
using OpenTelemetryExample.Client.Services;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Components.Telemetry;

/// <summary>
/// Component for displaying recent OpenTelemetry logs with filtering, pagination, and detailed view capabilities.
/// Provides real-time log monitoring and analysis functionality.
/// </summary>
public partial class RecentLogsCard : ComponentBase
{
    [Inject] public ITelemetryService TelemetryService { get; set; } = null!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;

    /// <summary>
    /// Gets or sets the data source for recent logs.
    /// </summary>
    [Parameter] public RecentLogsDto? DataSource { get; set; }

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
    private bool _errorsOnly;
    private string? _minLogLevel;
    private string? _searchText;
    private int _timeWindowMinutes = 30;
    private bool _isLoading;
    private int _lastRefreshTrigger;
    private LogDetailsModal? _logDetailsModal;
    
    // Pagination state
    private int _currentPage = 1;
    private int _pageSize = 20;

    /// <summary>
    /// Gets the current pagination info.
    /// </summary>
    private PaginationInfo? CurrentPagination => DataSource?.Pagination;

    /// <summary>
    /// Available log levels for filtering.
    /// </summary>
    private readonly string[] _logLevels = ["Trace", "Debug", "Information", "Warning", "Error", "Critical"];

    /// <summary>
    /// Initializes the component and loads initial log data.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await RefreshLogs();
    }

    /// <summary>
    /// Handles parameter changes and refreshes logs when triggered.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        if (_lastRefreshTrigger != RefreshTrigger)
        {
            _lastRefreshTrigger = RefreshTrigger;
            await RefreshLogs();
        }
    }

    /// <summary>
    /// Retrieves the current logs from the data source.
    /// </summary>
    /// <returns>Array of log DTOs.</returns>
    private LogDto[] GetLogs()
    {
        try
        {
            if (DataSource != null)
            {
                var logs = DataSource.Logs.ToArray();
                return logs;
            }
            return [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Handles the application filter change event.
    /// </summary>
    private async Task OnAppFilterChanged()
    {
        if (_appOnly && _mediatorOnly)
        {
            _mediatorOnly = false; // Reset mediator filter if both are selected
        }
        _currentPage = 1; // Reset to first page when changing filters
        await RefreshLogs();
        await OnFiltersChanged.InvokeAsync();
    }

    /// <summary>
    /// Handles the mediator filter change event.
    /// </summary>
    private async Task OnMediatorFilterChanged()
    {
        if (_mediatorOnly && _appOnly)
        {
            _appOnly = false; // Reset app filter if both are selected
        }
        _currentPage = 1; // Reset to first page when changing filters
        await RefreshLogs();
        await OnFiltersChanged.InvokeAsync();
    }

    /// <summary>
    /// Handles changes to the errors only filter.
    /// </summary>
    private async Task OnErrorsFilterChanged()
    {
        _currentPage = 1; // Reset to first page when changing filters
        await RefreshLogs();
        await OnFiltersChanged.InvokeAsync();
    }

    /// <summary>
    /// Handles changes to the minimum log level filter.
    /// </summary>
    private async Task SetMinLogLevel(string? level)
    {
        _minLogLevel = level;
        _currentPage = 1; // Reset to first page when changing filters
        await RefreshLogs();
        await OnFiltersChanged.InvokeAsync();
    }

    /// <summary>
    /// Handles changes to the search text filter.
    /// </summary>
    private async Task OnSearchTextChanged(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString();
        if (string.IsNullOrEmpty(_searchText))
            _searchText = null;
        
        _currentPage = 1; // Reset to first page when changing filters
        await RefreshLogs();
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
        await RefreshLogs();
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
        await RefreshLogs();
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
    /// Gets the CSS row class based on log level.
    /// </summary>
    /// <param name="log">The log to get the row class for.</param>
    /// <returns>CSS class name for the table row.</returns>
    private string GetRowClass(LogDto log)
    {
        // Remove background colors, only use left border styling like Recent Traces
        return string.Empty;
    }

    /// <summary>
    /// Gets the border color for the table row.
    /// </summary>
    /// <param name="log">The log to get the border color for.</param>
    /// <returns>CSS color value for the left border.</returns>
    private string GetRowBorderColor(LogDto log)
    {
        if (log.IsMediatorLog) return "#0d6efd"; // Bootstrap primary blue
        if (log.IsAppLog) return "#198754";      // Bootstrap success green
        return "#ffc107";                        // Bootstrap warning yellow (other logs)
    }

    /// <summary>
    /// Gets the CSS badge class for the log level.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns>CSS class for the log level badge.</returns>
    private string GetLogLevelBadgeClass(string logLevel)
    {
        return logLevel switch
        {
            "Critical" or "Error" => "bg-danger text-white",
            "Warning" => "bg-warning text-dark",
            "Information" => "bg-info text-white",
            "Debug" => "bg-secondary text-white",
            "Trace" => "bg-light text-dark",
            _ => "bg-secondary text-white"
        };
    }

    /// <summary>
    /// Gets the CSS badge class for the log source.
    /// </summary>
    /// <param name="source">The log source name.</param>
    /// <returns>CSS class for the source badge.</returns>
    private string GetSourceBadgeClass(string source)
    {
        return source.ToLower() switch
        {
            "blazing.mediator" => "bg-primary text-white",
            "mediator" => "bg-primary text-white",
            "aspnetcore" => "bg-secondary text-white",
            "asp.net core" => "bg-secondary text-white",
            "entityframework" => "bg-warning text-dark",
            "entity framework" => "bg-warning text-dark",
            "httpclient" => "bg-info text-white",
            "http client" => "bg-info text-white",
            "system" => "bg-light text-dark",
            "opentelemetryexample" => "bg-success text-white",
            "application" => "bg-success text-white",
            "controller" => "bg-info text-white",
            _ => "bg-light text-dark"
        };
    }

    /// <summary>
    /// Gets a user-friendly display name for the log source.
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
            "OpenTelemetryExample" => "Example",
            "Application" => "Example",
            "Controller" => "API",
            "Mediator" => "Mediator",
            "AspNetCore" => "AspNetCore",
            "EntityFramework" => "EF Core",
            "HttpClient" => "HTTP",
            _ => source
        };
    }

    /// <summary>
    /// Shortens a trace ID for display purposes.
    /// </summary>
    /// <param name="traceId">The full trace ID.</param>
    /// <returns>Shortened trace ID with ellipsis if needed.</returns>
    private string ShortenTraceId(string? traceId) 
    {
        if (string.IsNullOrEmpty(traceId))
            return "N/A";
        return traceId.Length > 8 ? traceId[..8] + "..." : traceId;
    }

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
    /// Updates the time window filter and refreshes logs.
    /// </summary>
    /// <param name="minutes">The new time window in minutes.</param>
    private async Task UpdateAge(int minutes)
    {
        _timeWindowMinutes = minutes;
        _currentPage = 1; // Reset to first page when changing filters
        await RefreshLogs();
    }

    /// <summary>
    /// Refreshes the log data from the telemetry service.
    /// </summary>
    private async Task RefreshLogs()
    {
        try
        {
            _isLoading = true;
            await InvokeAsync(StateHasChanged);
            
            DataSource = await TelemetryService.GetRecentLogsAsync(
                _timeWindowMinutes, 
                _appOnly, 
                _mediatorOnly, 
                _errorsOnly, 
                _minLogLevel, 
                _searchText, 
                _currentPage, 
                _pageSize);
            
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
    /// Opens the log details modal for the specified log.
    /// </summary>
    /// <param name="log">The log to display details for.</param>
    private async Task ViewLogDetails(LogDto log)
    {
        if (_logDetailsModal != null)
        {
            await _logDetailsModal.ShowLogDetailsAsync(log);
        }
    }

    /// <summary>
    /// Truncates a message for display in the table.
    /// </summary>
    /// <param name="message">The message to truncate.</param>
    /// <param name="maxLength">Maximum length to display.</param>
    /// <returns>Truncated message with ellipsis if needed.</returns>
    private string TruncateMessage(string message, int maxLength = 80)
    {
        if (string.IsNullOrEmpty(message))
            return "";
        
        if (message.Length <= maxLength)
            return message;
        
        return message[..maxLength] + "...";
    }

    /// <summary>
    /// Gets the icon for the log level.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns>Bootstrap icon class name.</returns>
    private string GetLogLevelIcon(string logLevel)
    {
        return logLevel switch
        {
            "Critical" or "Error" => "bi-exclamation-triangle-fill",
            "Warning" => "bi-exclamation-triangle",
            "Information" => "bi-info-circle",
            "Debug" => "bi-bug",
            "Trace" => "bi-search",
            _ => "bi-circle"
        };
    }
}