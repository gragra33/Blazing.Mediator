using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Domain.Entities;

namespace OpenTelemetryExample.Application.Services;

/// <summary>
/// Custom OpenTelemetry activity processor that captures raw Activity data and stores it in the database.
/// This processor receives all Activity data from the OpenTelemetry SDK before it's exported.
/// </summary>
public sealed class OpenTelemetryActivityProcessor(
    IServiceProvider serviceProvider,
    ILogger<OpenTelemetryActivityProcessor> logger)
    : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        try
        {
            // Don't capture our own telemetry endpoints to prevent feedback loops
            if (ShouldSkipActivity(activity))
            {
                return;
            }

            // Use a scope to get the DbContext since this is a singleton processor
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var telemetryTrace = new TelemetryTrace
            {
                TraceId = activity.TraceId.ToString(),
                SpanId = activity.SpanId.ToString(),
                OperationName = activity.OperationName,
                StartTime = activity.StartTimeUtc,
                Duration = activity.Duration,
                Status = activity.Status.ToString(),
                Tags = ExtractTags(activity),
                ExceptionType = GetExceptionType(activity),
                ExceptionMessage = GetExceptionMessage(activity),
                RequestType = GetRequestType(activity),
                HandlerName = GetHandlerName(activity)
            };

            var telemetryActivity = new TelemetryActivity
            {
                ActivityId = activity.Id ?? Guid.NewGuid().ToString(),
                OperationName = activity.OperationName,
                StartTime = activity.StartTimeUtc,
                Duration = activity.Duration,
                Status = activity.Status.ToString(),
                Kind = activity.Kind.ToString(),
                Tags = ExtractTags(activity),
                RequestType = GetRequestType(activity),
                HandlerName = GetHandlerName(activity),
                IsSuccess = activity.Status != ActivityStatusCode.Error
            };

            // Also create a metric entry for this activity
            var telemetryMetric = new TelemetryMetric
            {
                RequestType = telemetryActivity.RequestType,
                RequestName = ExtractRequestName(activity),
                Category = DetermineCategory(activity),
                Duration = activity.Duration.TotalMilliseconds,
                IsSuccess = activity.Status != ActivityStatusCode.Error,
                ErrorMessage = GetExceptionMessage(activity),
                Timestamp = activity.StartTimeUtc,
                HandlerName = telemetryActivity.HandlerName,
                Tags = ExtractTags(activity)
            };

            context.TelemetryTraces.Add(telemetryTrace);
            context.TelemetryActivities.Add(telemetryActivity);
            context.TelemetryMetrics.Add(telemetryMetric);

            context.SaveChanges();

            logger.LogDebug("Captured telemetry for activity: {OperationName} ({Duration}ms)", 
                activity.OperationName, activity.Duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing activity: {OperationName}", activity.OperationName);
        }
    }

    private static bool ShouldSkipActivity(Activity activity)
    {
        var operationName = activity.OperationName?.ToLowerInvariant() ?? "";
        
        // Skip telemetry endpoints to prevent feedback loops
        if (operationName.Contains("/telemetry") || 
            operationName.Contains("/debug") ||
            operationName.Contains("otlp") ||
            operationName.Contains("healthcheck"))
        {
            return true;
        }

        // Skip very short activities that are likely not interesting
        if (activity.Duration.TotalMilliseconds < 1)
        {
            return true;
        }

        return false;
    }

    private static Dictionary<string, object> ExtractTags(Activity activity)
    {
        var tags = new Dictionary<string, object>();

        // Add standard OpenTelemetry tags
        foreach (var tag in activity.Tags)
        {
            tags[tag.Key] = tag.Value ?? "";
        }

        // Add baggage
        foreach (var baggage in activity.Baggage)
        {
            tags[$"baggage.{baggage.Key}"] = baggage.Value ?? "";
        }

        // Add some computed tags
        tags["activity.kind"] = activity.Kind.ToString();
        tags["activity.status"] = activity.Status.ToString();
        tags["activity.duration_ms"] = activity.Duration.TotalMilliseconds;

        return tags;
    }

    private static string? GetExceptionType(Activity activity)
    {
        // Look for exception information in tags
        var exceptionType = activity.Tags.FirstOrDefault(t => 
            t.Key.Equals("exception.type", StringComparison.OrdinalIgnoreCase)).Value;
        
        return exceptionType;
    }

    private static string? GetExceptionMessage(Activity activity)
    {
        // Look for exception message in tags
        var exceptionMessage = activity.Tags.FirstOrDefault(t => 
            t.Key.Equals("exception.message", StringComparison.OrdinalIgnoreCase)).Value;
        
        return exceptionMessage;
    }

    private static string GetRequestType(Activity activity)
    {
        // Look for mediator-specific tags first
        var requestType = activity.Tags.FirstOrDefault(t => 
            t.Key.Equals("mediator.request_type", StringComparison.OrdinalIgnoreCase)).Value;
        
        if (!string.IsNullOrEmpty(requestType))
        {
            return requestType;
        }

        // Fallback to operation name
        return activity.OperationName;
    }

    private static string? GetHandlerName(Activity activity)
    {
        // Look for mediator handler information
        var handlerName = activity.Tags.FirstOrDefault(t => 
            t.Key.Equals("mediator.handler", StringComparison.OrdinalIgnoreCase)).Value;
        
        if (!string.IsNullOrEmpty(handlerName))
        {
            return handlerName;
        }

        // Try to infer handler name from request type
        var requestType = GetRequestType(activity);
        if (requestType.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
        {
            return requestType.Replace("Query", "Handler");
        }
        if (requestType.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
        {
            return requestType.Replace("Command", "Handler");
        }

        return null;
    }

    private static string ExtractRequestName(Activity activity)
    {
        var requestType = GetRequestType(activity);
        
        // Extract just the class name without namespace
        var lastDot = requestType.LastIndexOf('.');
        if (lastDot >= 0 && lastDot < requestType.Length - 1)
        {
            return requestType.Substring(lastDot + 1);
        }
        
        return requestType;
    }

    private static string DetermineCategory(Activity activity)
    {
        var requestType = GetRequestType(activity);
        
        if (requestType.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
            return "Query";
        if (requestType.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
            return "Command";
        if (activity.OperationName.Contains("HTTP", StringComparison.OrdinalIgnoreCase))
            return "HTTP";
        
        return "Activity";
    }
}