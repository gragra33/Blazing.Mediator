using Microsoft.AspNetCore.Components;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Components.Telemetry;

public partial class CommandsQueriesPerformanceCard : ComponentBase
{
    [Parameter] public LiveMetricsDto? DataSource { get; set; }

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
