using Microsoft.AspNetCore.Components;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Components.Telemetry;

/// <summary>
/// Component for displaying traces in a raw/flat table format.
/// </summary>
public partial class RawTracesTable : ComponentBase
{
    /// <summary>
    /// The traces to display.
    /// </summary>
    [Parameter] public IEnumerable<TraceDto>? Traces { get; set; }

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
    /// Gets filtered traces based on the HidePackets setting.
    /// </summary>
    /// <returns>Filtered collection of traces.</returns>
    private IEnumerable<TraceDto> GetFilteredTraces()
    {
        if (Traces == null) return [];

        if (HidePackets)
        {
            return Traces.Where(trace => 
                !trace.OperationName.Contains("packet_", StringComparison.OrdinalIgnoreCase));
        }

        return Traces;
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
    /// Gets the border color for the table row.
    /// </summary>
    /// <param name="trace">The trace to get the border color for.</param>
    /// <returns>CSS color value for the left border.</returns>
    private string GetRowBorderColor(TraceDto trace)
    {
        if (trace.IsMediatorTrace) return "#0d6efd"; // Bootstrap primary blue
        if (trace.IsAppTrace) return "#198754";      // Bootstrap success green
        return "#ffc107";                            // Bootstrap warning yellow
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
