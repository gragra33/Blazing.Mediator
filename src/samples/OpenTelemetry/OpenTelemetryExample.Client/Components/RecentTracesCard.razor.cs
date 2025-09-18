using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Components;

public partial class RecentTracesCard : ComponentBase
{
    [Parameter] public RecentTracesDto? DataSource { get; set; }
    [Parameter] public EventCallback OnFiltersChanged { get; set; }
    [Parameter] public int RefreshTrigger { get; set; }

    private int _maxRecords = 10;
    private bool _mediatorOnly;
    private int _timeWindowMinutes = 30;
    private TraceDto? _selectedTrace;
    private bool _isLoading;
    private int _lastRefreshTrigger;

    protected override async Task OnInitializedAsync()
    {
        await RefreshTraces();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_lastRefreshTrigger != RefreshTrigger)
        {
            _lastRefreshTrigger = RefreshTrigger;
            await RefreshTraces();
        }
    }

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
        catch (Exception ex)
        {
            return [];
        }
    }

    private string GetRowClass(TraceDto trace) => trace.IsMediatorTrace ? "table-info" : string.Empty;

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

    private string ShortenTraceId(string traceId) => traceId.Length > 8 ? traceId[..8] + "..." : traceId;

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

    private async Task UpdateAge(int minutes)
    {
        _timeWindowMinutes = minutes;
        await RefreshTraces();
    }

    private async Task UpdateCount(int count)
    {
        _maxRecords = count;
        await RefreshTraces();
    }

    private async Task OnMediatorFilterChanged()
    {
        await RefreshTraces();
    }

    private async Task RefreshTraces()
    {
        try
        {
            _isLoading = true;
            await InvokeAsync(StateHasChanged);
            DataSource = await TelemetryService.GetRecentTracesAsync(_maxRecords, _mediatorOnly, _timeWindowMinutes);
            if (OnFiltersChanged.HasDelegate)
            {
                await InvokeAsync(async () => await OnFiltersChanged.InvokeAsync());
            }
        }
        catch (Exception ex)
        {
        }
        finally
        {
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ViewTraceDetails(TraceDto trace)
    {
        _selectedTrace = trace;
        await JSRuntime.InvokeVoidAsync("eval", "new bootstrap.Modal(document.getElementById('traceDetailsModal')).show()");
    }

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