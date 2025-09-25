using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Components.Shared;

/// <summary>
/// Modal component for displaying detailed information about a telemetry log entry.
/// Provides comprehensive view of log metadata, message, exception details, and associated trace information.
/// </summary>
public partial class LogDetailsModal : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;

    private LogDto? _currentLog;
    private string _modalId = $"logDetailsModal_{Guid.NewGuid():N}";

    /// <summary>
    /// Shows the log details modal for the specified log entry.
    /// </summary>
    /// <param name="log">The log entry to display details for.</param>
    public async Task ShowLogDetailsAsync(LogDto log)
    {
        _currentLog = log;
        StateHasChanged();

        // Show the Bootstrap modal
        await JSRuntime.InvokeVoidAsync("eval", $"new bootstrap.Modal(document.getElementById('{_modalId}')).show()");
    }

    /// <summary>
    /// Hides the log details modal.
    /// </summary>
    public async Task HideLogDetailsAsync()
    {
        _currentLog = null;
        StateHasChanged();

        // Hide the Bootstrap modal
        await JSRuntime.InvokeVoidAsync("eval", $"bootstrap.Modal.getInstance(document.getElementById('{_modalId}'))?.hide()");
    }

    /// <summary>
    /// Copies the log summary to clipboard.
    /// </summary>
    private async Task CopyLogSummary()
    {
        if (_currentLog != null)
        {
            var summary = $"ID: {_currentLog.Id}, Time: {_currentLog.Timestamp}, Level: {_currentLog.LogLevel}, Message: {_currentLog.Message}";
            await CopyToClipboard(summary);
        }
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
            "mediator" => "bg-primary text-white",
            "application" => "bg-success text-white",
            "controller" => "bg-info text-white",
            "aspnetcore" => "bg-secondary text-white",
            "entityframework" => "bg-warning text-dark",
            "httpclient" => "bg-info text-white",
            "system" => "bg-light text-dark",
            _ => "bg-light text-dark"
        };
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

    /// <summary>
    /// Formats a dictionary of key-value pairs for display.
    /// </summary>
    /// <param name="dictionary">The dictionary to format.</param>
    /// <returns>Formatted string representation.</returns>
    private string FormatDictionary(Dictionary<string, object>? dictionary)
    {
        if (dictionary == null || !dictionary.Any())
            return "None";

        return string.Join(", ", dictionary.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
    }

    /// <summary>
    /// Copies text to the clipboard.
    /// </summary>
    /// <param name="text">The text to copy.</param>
    private async Task CopyToClipboard(string text)
    {
        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }
}