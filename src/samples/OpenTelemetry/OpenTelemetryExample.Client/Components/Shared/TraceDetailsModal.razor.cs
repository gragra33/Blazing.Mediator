using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Components.Shared;

/// <summary>
/// Shared component for displaying detailed trace information in a modal dialog.
/// Can be used by both raw and grouped trace display components.
/// </summary>
public partial class TraceDetailsModal : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;

    /// <summary>
    /// The trace to display details for.
    /// </summary>
    [Parameter] public TraceDto? SelectedTrace { get; set; }

    /// <summary>
    /// Unique ID for the modal element to avoid conflicts when multiple instances are used.
    /// </summary>
    [Parameter] public string ModalId { get; set; } = "traceDetailsModal";

    /// <summary>
    /// Shows the modal dialog for the specified trace.
    /// </summary>
    /// <param name="trace">The trace to display details for.</param>
    public async Task ShowTraceDetailsAsync(TraceDto trace)
    {
        SelectedTrace = trace;
        StateHasChanged();
        await JSRuntime.InvokeVoidAsync("eval", $"new bootstrap.Modal(document.getElementById('{ModalId}')).show()");
    }

    /// <summary>
    /// Hides the modal dialog.
    /// </summary>
    public async Task HideModalAsync()
    {
        await JSRuntime.InvokeVoidAsync("eval", $"bootstrap.Modal.getInstance(document.getElementById('{ModalId}'))?.hide()");
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

    /// <summary>
    /// Copies the specified text to the clipboard.
    /// </summary>
    /// <param name="text">The text to copy to the clipboard.</param>
    private async Task CopyToClipboard(string text)
    {
        try
        {
            if (!string.IsNullOrEmpty(text))
            {
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
            }
        }
        catch
        {
            // Silently handle any errors (e.g., clipboard not available)
        }
    }

    /// <summary>
    /// Copies a summary of the trace details to the clipboard.
    /// </summary>
    private async Task CopyTraceSummary()
    {
        if (SelectedTrace == null) return;

        var summary = $"""
            Trace Summary
            =============
            Operation: {SelectedTrace.OperationName}
            Trace ID: {SelectedTrace.TraceId}
            Span ID: {SelectedTrace.SpanId}
            Parent ID: {SelectedTrace.ParentId ?? "N/A"}
            Status: {SelectedTrace.Status}
            Duration: {SelectedTrace.Duration.TotalMilliseconds:#,##0}ms
            Start Time: {SelectedTrace.StartTime:yyyy-MM-dd HH:mm:ss.fff}
            Source: {GetSourceDisplayName(SelectedTrace.Source)}
            Mediator Trace: {(SelectedTrace.IsMediatorTrace ? "Yes" : "No")}
            
            Tags & Attributes:
            """;

        if (SelectedTrace.Tags.Any())
        {
            foreach (var tag in SelectedTrace.Tags)
            {
                summary += $"- {tag.Key}: {tag.Value}\n";
            }
        }
        else
        {
            summary += "- No tags available\n";
        }

        await CopyToClipboard(summary);
    }

    /// <summary>
    /// Formats dictionary data for display.
    /// </summary>
    /// <param name="dictionary">The dictionary to format.</param>
    /// <returns>Formatted string representation of the dictionary.</returns>
    private string FormatDictionary(IDictionary<string, object> dictionary)
    {
        if (dictionary == null || !dictionary.Any())
            return "No data available";

        return string.Join("\n", dictionary.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
    }
}
