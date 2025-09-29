namespace Example.Common.Logging;

/// <summary>
/// Custom console formatter that provides clean, readable output without timestamps and categories.
/// </summary>
public sealed class SimpleConsoleFormatter : ConsoleFormatter, IDisposable
{
    private readonly IDisposable? _optionsReloadToken;
    private SimpleConsoleFormatterOptions _formatterOptions;

    public SimpleConsoleFormatter(IOptionsMonitor<SimpleConsoleFormatterOptions> options)
        : base("SimpleClassName")
    {
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
        _formatterOptions = options.CurrentValue;
    }

    private void ReloadLoggerOptions(SimpleConsoleFormatterOptions options)
    {
        _formatterOptions = options;
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        string? message = logEntry.Formatter(logEntry.State, logEntry.Exception);
        if (message == null)
        {
            return;
        }

        // Get log level prefix
        string logLevelPrefix = GetLogLevelPrefix(logEntry.LogLevel);

        // Write the clean message without timestamp or category
        textWriter.WriteLine($"{logLevelPrefix}{message}");
    }

    private static string GetLogLevelPrefix(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce: ",
            LogLevel.Debug => "dbug: ",
            LogLevel.Information => "info: ",
            LogLevel.Warning => "warn: ",
            LogLevel.Error => "fail: ",
            LogLevel.Critical => "crit: ",
            _ => ""
        };
    }

    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
    }
}