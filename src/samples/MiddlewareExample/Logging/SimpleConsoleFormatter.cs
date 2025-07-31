namespace MiddlewareExample.Logging;

/// <summary>
/// A console formatter that displays only the class name instead of the fully qualified namespace.
/// </summary>
public sealed class SimpleConsoleFormatter(IOptionsMonitor<SimpleConsoleFormatterOptions> options)
    : ConsoleFormatter("SimpleClassName")
{
    private readonly SimpleConsoleFormatterOptions _formatterOptions = options.CurrentValue;

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        string? message = logEntry.Formatter(logEntry.State, logEntry.Exception);
        if (message == null)
        {
            return;
        }

        // Extract only the class name from the category
        string categoryName = logEntry.Category;
        int lastDotIndex = categoryName.LastIndexOf('.');
        string className = lastDotIndex >= 0 ? categoryName.Substring(lastDotIndex + 1) : categoryName;

        // Format the log entry with only the class name
        string logLevel = GetLogLevelString(logEntry.LogLevel);
        textWriter.WriteLine($"{logLevel}: {className}[{logEntry.EventId.Id}] {message}");

        // Write exception if present
        if (logEntry.Exception != null)
        {
            textWriter.WriteLine(logEntry.Exception.ToString());
        }
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }
}
