var traceDtos = recentTraces.Select(trace => new TraceDto
{
    TraceId = trace.TraceId,
    SpanId = trace.SpanId,
    OperationName = trace.OperationName,
    StartTime = trace.StartTime,
    Duration = trace.Duration,
    Status = trace.Status,
    Tags = trace.Tags ?? new Dictionary<string, object>(),
    Source = DetermineTraceSource(trace.OperationName, trace.Tags),
    IsMediatorTrace = IsMediatorTrace(trace.OperationName, trace.Tags)
}).ToList();

return result;
}
catch (Exception ex)
{
    logger.LogError(ex, "Error retrieving recent traces");
    
    return new RecentTracesDto
    {
        Timestamp = DateTime.UtcNow,
        Traces = new List<TraceDto>(),
        Message = $"Error retrieving traces: {ex.Message}"
    };
}
}

/// <summary>
/// Determines the source of a trace based on operation name and tags.
/// </summary>
private static string DetermineTraceSource(string operationName, Dictionary<string, object>? tags)
{
// Check operation name patterns first
if (operationName.Contains("Mediator", StringComparison.OrdinalIgnoreCase) ||
    operationName.StartsWith("Mediator.", StringComparison.OrdinalIgnoreCase))
    return "Blazing.Mediator";
    
if (operationName.StartsWith("Microsoft.AspNetCore", StringComparison.OrdinalIgnoreCase) ||
    operationName.Contains("HttpRequestIn", StringComparison.OrdinalIgnoreCase) ||
    operationName.Contains("AspNetCore", StringComparison.OrdinalIgnoreCase))
    return "ASP.NET Core";
    
if (operationName.Contains("EntityFramework", StringComparison.OrdinalIgnoreCase) ||
    operationName.Contains("Entity Framework", StringComparison.OrdinalIgnoreCase) ||
    operationName.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase))
    return "Entity Framework";
    
if (operationName.Contains("HttpClient", StringComparison.OrdinalIgnoreCase) ||
    operationName.StartsWith("HTTP", StringComparison.OrdinalIgnoreCase) ||
    operationName.Contains("System.Net.Http", StringComparison.OrdinalIgnoreCase))
    return "HTTP Client";

// Check for CQRS patterns that indicate Blazing.Mediator
if (operationName.EndsWith("Query", StringComparison.OrdinalIgnoreCase) ||
    operationName.EndsWith("Command", StringComparison.OrdinalIgnoreCase) ||
    operationName.Contains("Handler", StringComparison.OrdinalIgnoreCase))
    return "Blazing.Mediator";

// Check tags for additional context
if (tags != null)
{
    foreach (var tag in tags)
    {
        var key = tag.Key?.ToLower() ?? "";
        var value = tag.Value?.ToString()?.ToLower() ?? "";
        
        if (key.Contains("mediator") || value.Contains("blazing.mediator") || value.Contains("mediator"))
            return "Blazing.Mediator";
        
        if (key.Contains("aspnetcore") || value.Contains("aspnetcore") || value.Contains("microsoft.aspnetcore"))
            return "ASP.NET Core";
        
        if (key.Contains("entityframework") || value.Contains("entityframework") || value.Contains("ef.core"))
            return "Entity Framework";
            
        if (key.Contains("httpclient") || value.Contains("httpclient") || value.Contains("system.net.http"))
            return "HTTP Client";

        // Check request_type tag for CQRS operations
        if (key == "request_type" && (value == "command" || value == "query"))
            return "Blazing.Mediator";
    }
}

return "System";
}

/// <summary>
/// Determines if a trace is from Blazing.Mediator.
/// </summary>
private static bool IsMediatorTrace(string operationName, Dictionary<string, object>? tags)
{
// Check operation name patterns
if (operationName.Contains("Mediator", StringComparison.OrdinalIgnoreCase) ||
    operationName.StartsWith("Mediator.", StringComparison.OrdinalIgnoreCase))
    return true;

// Check for CQRS patterns
if (operationName.EndsWith("Command", StringComparison.OrdinalIgnoreCase) ||
    operationName.EndsWith("Query", StringComparison.OrdinalIgnoreCase) ||
    operationName.Contains("Handler", StringComparison.OrdinalIgnoreCase))
    return true;

// Check tags
if (tags != null)
{
    foreach (var tag in tags)
    {
        var key = tag.Key?.ToLower() ?? "";
        var value = tag.Value?.ToString()?.ToLower() ?? "";
        
        if (key.Contains("mediator") || value.Contains("blazing.mediator") || value.Contains("mediator"))
            return true;
        
        if (key == "request_type" && (value == "command" || value == "query"))
            return true;

        if (key == "request_name" || key == "handler.type")
            return true;
    }
}

return false;
}