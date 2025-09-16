using Microsoft.Extensions.Logging.Console;

namespace TypedMiddlewareExample.Logging;

/// <summary>
/// Custom console formatter options for clean output.
/// </summary>
public class SimpleConsoleFormatterOptions : ConsoleFormatterOptions
{
    /// <summary>
    /// Gets or sets whether to include timestamps in the output.
    /// </summary>
    public bool IncludeTimestamp { get; set; } = false;
}