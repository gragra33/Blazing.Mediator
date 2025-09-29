using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetryExample.Domain.Entities;
using OpenTelemetryExample.Infrastructure.Data;

namespace OpenTelemetryExample.Application.Services;

/// <summary>
/// Custom OpenTelemetry metrics exporter that processes metrics data and stores it in the database.
/// </summary>
public sealed class OpenTelemetryMetricsExporter(IServiceProvider serviceProvider) : BaseExporter<Metric>
{
    private readonly ILogger<OpenTelemetryMetricsExporter> _logger = serviceProvider.GetRequiredService<ILogger<OpenTelemetryMetricsExporter>>();

    public override ExportResult Export(in Batch<Metric> batch)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            foreach (var metric in batch)
            {
                ProcessMetric(metric, context);
            }

            context.SaveChanges();

            _logger.LogDebug("Exported {MetricCount} metrics to database", batch.Count);
            return ExportResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting metrics batch");
            return ExportResult.Failure;
        }
    }

    private void ProcessMetric(Metric metric, ApplicationDbContext context)
    {
        try
        {
            // Skip internal telemetry metrics to prevent feedback loops
            if (ShouldSkipMetric(metric))
            {
                return;
            }

            foreach (ref readonly var metricPoint in metric.GetMetricPoints())
            {
                var tags = ExtractTags(metricPoint);
                var metricName = metric.Name;
                var meterName = metric.MeterName;

                // Determine the category and request type from the metric
                var category = DetermineCategory(metricName, meterName, tags);
                var requestType = GetRequestType(metricName, tags);
                var requestName = GetRequestName(requestType);

                // Get the metric value (simplified approach)
                var (value, isSuccess, errorMessage) = ExtractMetricValue(metricPoint);

                var telemetryMetric = new TelemetryMetric
                {
                    RequestType = requestType,
                    RequestName = requestName,
                    Category = category,
                    Duration = value,
                    IsSuccess = isSuccess,
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.UtcNow,
                    HandlerName = GetHandlerName(requestType),
                    Tags = tags
                };

                // Add additional metric-specific tags
                telemetryMetric.Tags["metric.name"] = metricName;
                telemetryMetric.Tags["metric.meter"] = meterName;
                telemetryMetric.Tags["metric.type"] = metric.MetricType.ToString();

                context.TelemetryMetrics.Add(telemetryMetric);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing metric: {MetricName}", metric.Name);
        }
    }

    private static bool ShouldSkipMetric(Metric metric)
    {
        var metricName = metric.Name?.ToLowerInvariant() ?? "";
        var meterName = metric.MeterName?.ToLowerInvariant() ?? "";

        // Skip internal OpenTelemetry metrics
        if (meterName.Contains("opentelemetry") && !meterName.Contains("blazing.mediator"))
        {
            return true;
        }

        // Skip HTTP metrics for telemetry endpoints
        if (metricName.Contains("http") && metricName.Contains("telemetry"))
        {
            return true;
        }

        return false;
    }

    private static Dictionary<string, object> ExtractTags(MetricPoint metricPoint)
    {
        var tags = new Dictionary<string, object>();

        foreach (var tag in metricPoint.Tags)
        {
            tags[tag.Key] = tag.Value ?? "";
        }

        return tags;
    }

    private static string DetermineCategory(string metricName, string meterName, Dictionary<string, object> tags)
    {
        // Check if this is a Blazing.Mediator metric
        if (meterName.Contains("blazing.mediator", StringComparison.OrdinalIgnoreCase))
        {
            if (tags.TryGetValue("request_type", out var requestTypeObj) && requestTypeObj is string requestType)
            {
                if (requestType.Equals("query", StringComparison.OrdinalIgnoreCase))
                    return "Query";
                if (requestType.Equals("command", StringComparison.OrdinalIgnoreCase))
                    return "Command";
            }

            // Fallback to metric name analysis
            if (metricName.Contains("send", StringComparison.OrdinalIgnoreCase))
            {
                return "Request";
            }
            if (metricName.Contains("publish", StringComparison.OrdinalIgnoreCase))
            {
                return "Notification";
            }
        }

        // Check for HTTP metrics
        if (metricName.Contains("http", StringComparison.OrdinalIgnoreCase))
        {
            return "HTTP";
        }

        return "Metric";
    }

    private static string GetRequestType(string metricName, Dictionary<string, object> tags)
    {
        // Try to get request name from tags first
        if (tags.TryGetValue("request_name", out var requestNameObj) && requestNameObj is string requestName)
        {
            return requestName;
        }

        // Fallback to metric name
        return metricName;
    }

    private static string GetRequestName(string requestType)
    {
        // Extract just the class name without namespace
        var lastDot = requestType.LastIndexOf('.');
        if (lastDot >= 0 && lastDot < requestType.Length - 1)
        {
            return requestType.Substring(lastDot + 1);
        }

        return requestType;
    }

    private static string? GetHandlerName(string requestType)
    {
        var requestName = GetRequestName(requestType);

        if (requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
        {
            return requestName.Replace("Query", "Handler");
        }
        if (requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
        {
            return requestName.Replace("Command", "Handler");
        }

        return null;
    }

    private static (double value, bool isSuccess, string? errorMessage) ExtractMetricValue(MetricPoint metricPoint)
    {
        try
        {
            // Simplified approach - try to get the sum from histogram or count from counter
            try
            {
                return (metricPoint.GetHistogramSum(), true, null);
            }
            catch
            {
                try
                {
                    return (metricPoint.GetHistogramCount(), true, null);
                }
                catch
                {
                    // If all else fails, return a default value
                    return (1.0, true, null);
                }
            }
        }
        catch (Exception ex)
        {
            return (0, false, ex.Message);
        }
    }
}