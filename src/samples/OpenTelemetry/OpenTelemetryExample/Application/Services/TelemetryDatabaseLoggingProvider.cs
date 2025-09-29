using Microsoft.Extensions.Options;
using OpenTelemetryExample.Domain.Entities;
using OpenTelemetryExample.Infrastructure.Data;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Services;

/// <summary>
/// Custom logging provider that captures log entries and stores them in the database for telemetry purposes.
/// This provider integrates with OpenTelemetry to capture both application and framework logs.
/// Follows Microsoft's recommended pattern for custom logging providers.
/// </summary>
[ProviderAlias("TelemetryDatabase")]
public sealed class TelemetryDatabaseLoggingProvider : ILoggerProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDisposable? _onChangeToken;
    private TelemetryDatabaseLoggingConfiguration _currentConfig;
    private readonly ConcurrentDictionary<string, TelemetryDatabaseLogger> _loggers = new();
    private readonly Timer _batchTimer;
    private readonly ConcurrentQueue<TelemetryLog> _logQueue = new();
    private bool _disposed;

    public TelemetryDatabaseLoggingProvider(
        IServiceProvider serviceProvider,
        IOptionsMonitor<TelemetryDatabaseLoggingConfiguration> config)
    {
        _serviceProvider = serviceProvider;
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);

        // Set up batch processing timer with configurable interval
        _batchTimer = new Timer(ProcessLogBatch, null,
            TimeSpan.FromMilliseconds(_currentConfig.ProcessingIntervalMs),
            TimeSpan.FromMilliseconds(_currentConfig.ProcessingIntervalMs));

        Console.WriteLine($"[*] TelemetryDatabaseLoggingProvider initialized with batch processing (BatchSize: {_currentConfig.BatchSize}, Interval: {_currentConfig.ProcessingIntervalMs}ms)");
    }

    public ILogger CreateLogger(string categoryName)
    {
        Console.WriteLine($"[TELEMETRY DEBUG] TelemetryDatabaseLoggingProvider.CreateLogger called for category: {categoryName}");
        return _loggers.GetOrAdd(categoryName, name => new TelemetryDatabaseLogger(name, this, GetCurrentConfig));
    }

    private TelemetryDatabaseLoggingConfiguration GetCurrentConfig() => _currentConfig;

    internal void AddLogEntry(TelemetryLog logEntry)
    {
        if (!_disposed && _currentConfig.Enabled)
        {
            _logQueue.Enqueue(logEntry);
            Console.WriteLine($"[DEBUG] Enqueued log: {logEntry.LogLevel} - {logEntry.Message}");
        }
    }

    private async void ProcessLogBatch(object? state)
    {
        if (_disposed || _logQueue.IsEmpty || !_currentConfig.Enabled)
            return;

        var logs = new List<TelemetryLog>();

        // Dequeue logs up to configured batch size
        while (logs.Count < _currentConfig.BatchSize && _logQueue.TryDequeue(out var log))
        {
            logs.Add(log);
        }

        if (logs.Count == 0)
            return;

        try
        {
            Console.WriteLine($"[DEBUG] Processing batch of {logs.Count} logs");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Add logs to context
            await context.TelemetryLogs.AddRangeAsync(logs);

            // Save changes
            var savedCount = await context.SaveChangesAsync();

            Console.WriteLine($"[DEBUG] Successfully saved {savedCount} logs to database");
        }
        catch (Exception ex)
        {
            // Log processing error to console (avoid recursive logging)
            Console.WriteLine($"[ERROR] TelemetryDatabaseLoggingProvider error processing log batch: {ex.Message}");
            Console.WriteLine($"[ERROR] Exception type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[ERROR] Inner exception: {ex.InnerException.Message}");
            }
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        Console.WriteLine(Resources.ProviderDisposing);
        _disposed = true;

        // Process any remaining logs
        ProcessLogBatch(null);

        _batchTimer?.Dispose();
        _onChangeToken?.Dispose();

        foreach (var logger in _loggers.Values)
        {
            logger.Dispose();
        }
        _loggers.Clear();
    }
    private static class Resources
    {
        public const string ProviderDisposing = "[*] TelemetryDatabaseLoggingProvider disposing";
    }
}

/// <summary>
/// Custom logger implementation that captures log entries for telemetry purposes.
/// </summary>
internal sealed class TelemetryDatabaseLogger(
    string categoryName,
    TelemetryDatabaseLoggingProvider provider,
    Func<TelemetryDatabaseLoggingConfiguration> getCurrentConfig)
    : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null; // We don't implement scopes for this example
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        var config = getCurrentConfig();
        if (!config.Enabled)
            return false;

        // Use configuration to determine if the log level should be captured
        if (logLevel >= config.MinimumLogLevel)
            return true;

        // Allow debug logs if configured
        return logLevel == LogLevel.Debug && config.CaptureDebugLogs;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"[TELEMETRY DEBUG] TelemetryDatabaseLogger.Log called: Level={logLevel}, Category={categoryName}");

        if (!IsEnabled(logLevel))
        {
            Console.WriteLine($"[TELEMETRY DEBUG] Log level {logLevel} not enabled for {categoryName}");
            return;
        }

        var activity = Activity.Current;
        var message = formatter(state, exception);
        var config = getCurrentConfig();

        Console.WriteLine($"[TELEMETRY DEBUG] Processing log message: {message}");

        // Skip telemetry-related logs to avoid recursive logging using configuration
        if (ShouldSkipLog(message, categoryName, config.SkipPatterns))
        {
            Console.WriteLine($"[DEBUG] Skipping log to avoid recursion: {categoryName} - {message}");
            return;
        }

        Console.WriteLine($"[DEBUG] Capturing log: {logLevel} - {categoryName} - {message}");

        try
        {
            var logEntry = new TelemetryLog
            {
                Timestamp = DateTime.UtcNow,
                LogLevel = logLevel.ToString(),
                Category = categoryName,
                Message = message,
                Exception = exception?.ToString(),
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
                Source = DetermineSource(categoryName),
                MachineName = Environment.MachineName,
                ProcessId = Environment.ProcessId,
                ThreadId = Environment.CurrentManagedThreadId,
                EventId = eventId.Id == 0 ? null : eventId.Id,
                Tags = ExtractTags(state),
                Scopes = null // For simplicity, we're not capturing scopes in this example
            };

            provider.AddLogEntry(logEntry);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to capture log entry: {ex.Message}");
        }
    }

    private static bool ShouldSkipLog(string message, string category, string[] skipPatterns)
    {
        // Quick check for Entity Framework categories to avoid recursion
        if (category.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check if any skip pattern matches the message or category
        return skipPatterns.Any(pattern =>
            message.Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
            category.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static string DetermineSource(string categoryName)
    {
        return categoryName switch
        {
            _ when categoryName.StartsWith("Blazing.Mediator") => "Mediator",
            _ when categoryName.StartsWith("OpenTelemetryExample.Controllers") => "Controller",
            _ when categoryName.StartsWith("OpenTelemetryExample.Application") => "Application",
            _ when categoryName.StartsWith("OpenTelemetryExample") => "Application",
            _ when categoryName.StartsWith("Microsoft.AspNetCore") => "AspNetCore",
            _ when categoryName.StartsWith("Microsoft.EntityFrameworkCore") => "EntityFramework",
            _ when categoryName.StartsWith("System.Net.Http") => "HttpClient",
            _ => "System"
        };
    }

    private static Dictionary<string, object> ExtractTags<TState>(TState state)
    {
        var tags = new Dictionary<string, object>();

        if (state is IEnumerable<KeyValuePair<string, object>> keyValuePairs)
        {
            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Key != "{OriginalFormat}" && !string.IsNullOrEmpty(kvp.Key))
                {
                    tags[kvp.Key] = kvp.Value?.ToString() ?? "";
                }
            }
        }

        return tags;
    }

    public void Dispose()
    {
        // Nothing to dispose for individual logger
    }
}
