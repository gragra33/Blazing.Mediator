using Microsoft.AspNetCore.Components;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Components.Telemetry;

/// <summary>
/// Component for displaying traces in a grouped/hierarchical table format.
/// </summary>
public partial class GroupedTracesTable : ComponentBase
{
    /// <summary>
    /// The trace groups to display.
    /// </summary>
    [Parameter] public IEnumerable<TraceGroupDto>? TraceGroups { get; set; }

    /// <summary>
    /// Indicates whether the component is in a loading state.
    /// </summary>
    [Parameter] public bool IsLoading { get; set; }

    /// <summary>
    /// Whether to hide packet-level traces.
    /// </summary>
    [Parameter] public bool HidePackets { get; set; }

    /// <summary>
    /// Callback invoked when the user wants to view trace details.
    /// </summary>
    [Parameter] public EventCallback<TraceDto> OnViewTraceDetails { get; set; }

    /// <summary>
    /// Gets filtered trace groups based on the HidePackets setting.
    /// </summary>
    /// <returns>Filtered collection of trace groups.</returns>
    private IEnumerable<TraceGroupDto> GetFilteredTraceGroups()
    {
        if (TraceGroups == null) return [];

        if (HidePackets)
        {
            // Filter out packet traces from child traces
            return TraceGroups.Select(group => new TraceGroupDto
            {
                TraceId = group.TraceId,
                RootTrace = group.RootTrace,
                Status = group.Status,
                StartTime = group.StartTime,
                TotalDuration = group.TotalDuration,
                TraceCount = group.TraceCount,
                IsExpanded = group.IsExpanded,
                ChildTraces = FilterPacketsFromHierarchy(group.ChildTraces)
            }).Where(group => group.ChildTraces.Any() || !group.RootTrace.OperationName.Contains("packet_", StringComparison.OrdinalIgnoreCase));
        }

        return TraceGroups;
    }

    /// <summary>
    /// Recursively filters packet traces from hierarchical trace structure.
    /// </summary>
    /// <param name="traces">The hierarchical traces to filter.</param>
    /// <returns>Filtered hierarchical traces.</returns>
    private List<HierarchicalTraceDto> FilterPacketsFromHierarchy(IEnumerable<HierarchicalTraceDto> traces)
    {
        return traces
            .Where(trace => !trace.Trace.OperationName.Contains("packet_", StringComparison.OrdinalIgnoreCase))
            .Select(trace => new HierarchicalTraceDto
            {
                Trace = trace.Trace,
                Level = trace.Level,
                IsExpanded = trace.IsExpanded,
                Children = FilterPacketsFromHierarchy(trace.Children)
            })
            .ToList();
    }

    /// <summary>
    /// Toggles the expansion state of a trace group.
    /// </summary>
    /// <param name="group">The group to toggle.</param>
    private void ToggleGroupExpansion(TraceGroupDto group)
    {
        group.IsExpanded = !group.IsExpanded;
        StateHasChanged();
    }

    /// <summary>
    /// Toggles the expansion state of a hierarchical trace.
    /// </summary>
    /// <param name="trace">The trace to toggle.</param>
    private void ToggleTraceExpansion(HierarchicalTraceDto trace)
    {
        trace.IsExpanded = !trace.IsExpanded;
        StateHasChanged();
    }

    /// <summary>
    /// Expands all trace groups.
    /// </summary>
    private void ExpandAll()
    {
        if (TraceGroups == null) return;

        foreach (var group in TraceGroups)
        {
            group.IsExpanded = true;
            ExpandAllInHierarchy(group.ChildTraces);
        }
        StateHasChanged();
    }

    /// <summary>
    /// Collapses all trace groups.
    /// </summary>
    private void CollapseAll()
    {
        if (TraceGroups == null) return;

        foreach (var group in TraceGroups)
        {
            group.IsExpanded = false;
            CollapseAllInHierarchy(group.ChildTraces);
        }
        StateHasChanged();
    }

    /// <summary>
    /// Recursively expands all traces in the hierarchy.
    /// </summary>
    /// <param name="traces">The traces to expand.</param>
    private void ExpandAllInHierarchy(IEnumerable<HierarchicalTraceDto> traces)
    {
        foreach (var trace in traces)
        {
            trace.IsExpanded = true;
            ExpandAllInHierarchy(trace.Children);
        }
    }

    /// <summary>
    /// Recursively collapses all traces in the hierarchy.
    /// </summary>
    /// <param name="traces">The traces to collapse.</param>
    private void CollapseAllInHierarchy(IEnumerable<HierarchicalTraceDto> traces)
    {
        foreach (var trace in traces)
        {
            trace.IsExpanded = false;
            CollapseAllInHierarchy(trace.Children);
        }
    }

    /// <summary>
    /// Gets the indentation in pixels for a given level.
    /// </summary>
    /// <param name="level">The nesting level.</param>
    /// <returns>Indentation in pixels.</returns>
    private int GetIndentationPixels(int level) => level * 20;

    /// <summary>
    /// Gets the CSS style for child rows based on indentation level.
    /// </summary>
    /// <param name="level">The nesting level.</param>
    /// <returns>CSS style string.</returns>
    private string GetChildRowStyle(int level)
    {
        return string.Empty; // No additional styling needed, border handled inline
    }

    /// <summary>
    /// Gets the border color for child traces based on nesting level.
    /// </summary>
    /// <param name="level">The nesting level.</param>
    /// <returns>CSS color value for the left border.</returns>
    private string GetChildBorderColor(int level)
    {
        return level switch
        {
            1 => "#0d6efd", // Primary blue
            2 => "#198754", // Success green  
            3 => "#ffc107", // Warning yellow
            _ => "#6c757d"  // Secondary gray
        };
    }

    /// <summary>
    /// Gets the CSS row class based on trace type.
    /// </summary>
    /// <param name="trace">The trace to get the class for.</param>
    /// <returns>CSS class for the table row.</returns>
    private string GetRowClass(TraceDto trace)
    {
        return string.Empty; // Remove background colors, only use borders
    }

    /// <summary>
    /// Gets the CSS row class for a trace group.
    /// </summary>
    /// <param name="group">The group to get the class for.</param>
    /// <returns>CSS class for the table row.</returns>
    private string GetGroupRowClass(TraceGroupDto group)
    {
        return string.Empty; // Remove background colors, only use borders
    }

    /// <summary>
    /// Gets the border color for a trace group.
    /// </summary>
    /// <param name="group">The group to get the border color for.</param>
    /// <returns>CSS color value for the left border.</returns>
    private string GetGroupBorderColor(TraceGroupDto group)
    {
        if (group.RootTrace.IsMediatorTrace) return "#0d6efd"; // Bootstrap primary blue
        if (group.RootTrace.IsAppTrace) return "#198754";      // Bootstrap success green
        return "#ffc107";                                      // Bootstrap warning yellow
    }

    /// <summary>
    /// Shortens a trace ID for display.
    /// </summary>
    /// <param name="traceId">The full trace ID.</param>
    /// <returns>Shortened trace ID for display.</returns>
    private string ShortenTraceId(string traceId)
    {
        if (string.IsNullOrEmpty(traceId))
            return string.Empty;
        return traceId.Length > 8 ? traceId[..8] + "..." : traceId;
    }

    /// <summary>
    /// Shortens a span ID for display.
    /// </summary>
    /// <param name="spanId">The full span ID.</param>
    /// <returns>Shortened span ID for display.</returns>
    private string ShortenSpanId(string spanId)
    {
        if (string.IsNullOrEmpty(spanId))
            return string.Empty;
        return spanId.Length > 8 ? spanId[..8] + "..." : spanId;
    }

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
            "opentelemetryexample" => "bg-success text-white",
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
            "OpenTelemetryExample" => "Example",
            _ => source
        };
    }
}
