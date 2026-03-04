using System.Diagnostics;

namespace Blazing.Mediator;

public sealed partial class Mediator
{
    /// <summary>
    /// Efficiently determines if a request is a query or command with minimal performance impact.
    /// First checks primary interfaces, then falls back to name-based detection using ReadOnlySpan.
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="request">The request to analyze</param>
    /// <returns>True if it's a query, false if it's a command</returns>
    private static bool DetermineIfQuery<TResponse>(IRequest<TResponse> request)
    {
        Type requestType = request.GetType();

        // Fast path: Check if request directly implements IQuery<TResponse>
        if (request is IQuery<TResponse>)
        {
            return true;
        }

        // Fast path: Check if request directly implements ICommand<TResponse>
        if (request is ICommand<TResponse>)
        {
            return false;
        }

        // Fallback: Check type name suffix using ReadOnlySpan for performance
        ReadOnlySpan<char> typeName = requestType.Name.AsSpan();

        // Check for "Query" suffix (case-insensitive)
        if (typeName.Length >= 5 &&
            typeName[^5..].Equals("Query".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check for "Command" suffix (case-insensitive)
        if (typeName.Length >= 7 &&
            typeName[^7..].Equals("Command".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Default to query if no clear indication (maintains backward compatibility)
        return true;
    }

    /// <summary>
    /// Sanitizes type names by removing sensitive information and generic suffixes.
    /// </summary>
    /// <param name="typeName">The type name to sanitize.</param>
    /// <returns>A sanitized type name safe for telemetry.</returns>
    private string SanitizeTypeName(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            return "unknown";
        }

        // Remove generic type suffix (e.g., "`1", "`2)
        var backtickIndex = typeName.IndexOf('`');
        if (backtickIndex > 0)
        {
            typeName = typeName[..backtickIndex];
        }

        // Remove sensitive patterns based on configuration
        foreach (var pattern in SensitiveDataPatterns)
        {
            if (typeName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                typeName = typeName.Replace(pattern, "***", StringComparison.OrdinalIgnoreCase);
            }
        }

        return typeName;
    }

    /// <summary>
    /// Sanitizes middleware names for telemetry with full generic signature.
    /// </summary>
    /// <param name="middlewareType">The middleware type to sanitize.</param>
    /// <returns>A sanitized middleware name safe for telemetry with full generic signature.</returns>
    private string SanitizeMiddlewareName(Type middlewareType)
    {
        // Use PipelineUtilities to format the type name with full generic signature
        // This ensures ErrorHandlingMiddleware<TRequest> is distinguished from ErrorHandlingMiddleware<TRequest, TResponse>
        var formattedName = PipelineUtilities.FormatTypeName(middlewareType);
        
        // Apply sensitive data pattern filtering
        foreach (var pattern in SensitiveDataPatterns)
        {
            if (formattedName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                formattedName = formattedName.Replace(pattern, "***", StringComparison.OrdinalIgnoreCase);
            }
        }
        
        return formattedName;
    }

    /// <summary>
    /// Sanitizes exception messages by removing sensitive information.
    /// </summary>
    /// <param name="message">The exception message to sanitize.</param>
    /// <returns>A sanitized exception message safe for telemetry.</returns>
    private string SanitizeExceptionMessage(string? message)
    {
        if (string.IsNullOrEmpty(message))
            return "unknown_error";

        // Remove potential sensitive data patterns based on configuration
        var sanitized = message;

        foreach (string pattern in SensitiveDataPatterns.Where(pattern => sanitized.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            sanitized = $"{pattern}_error";
            break; // Stop at first match to avoid over-sanitization
        }

        // Remove SQL connection strings
        if (sanitized.Contains("connection", StringComparison.OrdinalIgnoreCase))
        {
            sanitized = "connection_error";
        }

        // Remove file paths
        if (sanitized.Contains(":\\") || sanitized.Contains("/"))
        {
            sanitized = "file_path_error";
        }

        // Limit length based on configuration
        return sanitized.Length > MaxExceptionMessageLength ?
            sanitized[..MaxExceptionMessageLength] + "..." : sanitized;
    }

    /// <summary>
    /// Sanitizes stack traces by removing file paths and limiting content.
    /// </summary>
    /// <param name="stackTrace">The stack trace to sanitize.</param>
    /// <returns>A sanitized stack trace safe for telemetry.</returns>
    private string? SanitizeStackTrace(string? stackTrace)
    {
        if (string.IsNullOrEmpty(stackTrace))
            return null;

        // For telemetry, we only want the first few lines without file paths
        var lines = stackTrace.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var sanitizedLines = new List<string>();

        for (int i = 0; i < Math.Min(MaxStackTraceLines, lines.Length); i++)
        {
            var line = lines[i].Trim();

            // Remove file paths
            var inIndex = line.LastIndexOf(" in ", StringComparison.Ordinal);
            if (inIndex > 0)
            {
                line = line[..inIndex];
            }

            sanitizedLines.Add(line);
        }

        return string.Join(" | ", sanitizedLines);
    }

    /// <summary>
    /// Gets telemetry health status.
    /// </summary>
    /// <returns>True if telemetry is enabled and working.</returns>
    public static bool GetTelemetryHealth()
    {
        try
        {
            if (!TelemetryEnabled)
                return false;

            // Test if we can record a metric
            MediatorMetrics.TelemetryHealthCounter.Add(1, new TagList { { "operation", "health_check" } });
            return true;
        }
        catch
        {
            return false;
        }
    }
}