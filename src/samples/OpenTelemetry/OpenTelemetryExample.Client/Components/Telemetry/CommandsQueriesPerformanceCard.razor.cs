using Microsoft.AspNetCore.Components;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Components.Telemetry;

/// <summary>
/// Blazor component card for displaying command and query performance metrics.
/// </summary>
public partial class CommandsQueriesPerformanceCard : ComponentBase
{
    /// <summary>
    /// Gets or sets the live metrics data source for the component.
    /// </summary>
    [Parameter] public LiveMetricsDto? DataSource { get; set; }

    /// <summary>
    /// Retrieves the list of command performance metrics from the data source.
    /// </summary>
    /// <returns>An array of <see cref="CommandPerformanceDto"/> representing command metrics.</returns>
    private CommandPerformanceDto[] GetCommands()
    {
        try
        {
            return DataSource?.Commands.ToArray() ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error getting commands: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Retrieves the list of query performance metrics from the data source.
    /// </summary>
    /// <returns>An array of <see cref="QueryPerformanceDto"/> representing query metrics.</returns>
    private QueryPerformanceDto[] GetQueries()
    {
        try
        {
            return DataSource?.Queries.ToArray() ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error getting queries: {ex.Message}");
            return [];
        }
    }
}
