using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Application.Queries;
using OpenTelemetryExample.Domain.Entities;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Shared.Models;
using System.Globalization;

namespace OpenTelemetryExample.Application.Handlers;

/// <summary>
/// Handler for GetGroupedTracesQuery that retrieves and organizes trace data in hierarchical groups with server-side pagination.
/// </summary>
public sealed class GetGroupedTracesHandler(ApplicationDbContext context, ILogger<GetGroupedTracesHandler> logger)
    : IRequestHandler<GetGroupedTracesQuery, GroupedTracesDto>
{
    public async Task<GroupedTracesDto> Handle(GetGroupedTracesQuery request, CancellationToken cancellationToken)
    {
        var cutoffTime = DateTime.UtcNow - request.TimeWindow;

        try
        {
            // Validate and normalize pagination parameters
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Max(1, Math.Min(request.PageSize, 50)); // Cap at 50 groups per page
            var skip = (page - 1) * pageSize;

            // Get total count for the timeframe first (before any filtering)
            var totalAvailableInTimeframe = await context.TelemetryTraces
                .Where(t => t.StartTime >= cutoffTime)
                .CountAsync(cancellationToken);

            // Start with base query for the timeframe
            var baseQuery = context.TelemetryTraces
                .Where(t => t.StartTime >= cutoffTime);

            // Apply combined filter logic
            var filteredTraces = ApplyTraceFilters(baseQuery, request.MediatorOnly, request.ExampleAppOnly, request.HidePackets);

            // Get all filtered traces for grouping
            var allFilteredTraces = await filteredTraces
                .OrderByDescending(t => t.StartTime)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!allFilteredTraces.Any())
            {
                logger.LogInformation("No telemetry traces found in the last {TimeWindow} minutes with filter: MediatorOnly={MediatorOnly}, ExampleAppOnly={ExampleAppOnly}, HidePackets={HidePackets}, Page={Page}",
                    request.TimeWindow.TotalMinutes, request.MediatorOnly, request.ExampleAppOnly, request.HidePackets, page);

                return new GroupedTracesDto
                {
                    Timestamp = DateTime.UtcNow,
                    TraceGroups = [],
                    Message = $"No trace data available in the last {request.TimeWindow.TotalMinutes:F0} minutes. Total traces available: {totalAvailableInTimeframe}",
                    TotalTracesInTimeframe = totalAvailableInTimeframe,
                    TotalTraceGroups = 0,
                    Pagination = new PaginationInfo
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalFilteredCount = 0,
                        ItemCount = 0
                    }
                };
            }

            // Group traces by TraceId and get total group count
            var allTraceGroups = GroupTracesByTraceId(allFilteredTraces);
            var totalGroupCount = allTraceGroups.Count;

            // Apply pagination to the groups
            var pagedTraceGroups = allTraceGroups
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Create pagination info
            var pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalFilteredCount = totalGroupCount,
                ItemCount = pagedTraceGroups.Count
            };

            // Create detailed message with pagination info
            string filterMessage = $"MediatorOnly={request.MediatorOnly}, ExampleAppOnly={request.ExampleAppOnly}, HidePackets={request.HidePackets} " +
                                 $"(page {pagination.Page} of {pagination.TotalPages}, showing {pagination.StartIndex}-{pagination.EndIndex} of {totalGroupCount} groups, total: {totalAvailableInTimeframe} traces available)";

            var result = new GroupedTracesDto
            {
                Timestamp = DateTime.UtcNow,
                TraceGroups = pagedTraceGroups,
                Message = $"Grouped traces from the last {request.TimeWindow.TotalMinutes:F0} minutes - {filterMessage}",
                TotalTracesInTimeframe = totalAvailableInTimeframe,
                TotalTraceGroups = totalGroupCount,
                Pagination = pagination
            };

            logger.LogInformation("Retrieved page {Page} of trace groups (PageSize={PageSize}, MediatorOnly={MediatorOnly}, ExampleAppOnly={ExampleAppOnly}, HidePackets={HidePackets}, total groups: {TotalGroups}, total traces available: {TotalAvailable})",
                page, pageSize, request.MediatorOnly, request.ExampleAppOnly, request.HidePackets, totalGroupCount, totalAvailableInTimeframe);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving grouped traces");

            return new GroupedTracesDto
            {
                Timestamp = DateTime.UtcNow,
                TraceGroups = [],
                Message = $"Error retrieving traces: {ex.Message}",
                TotalTracesInTimeframe = 0,
                TotalTraceGroups = 0,
                Pagination = new PaginationInfo
                {
                    Page = Math.Max(1, request.Page),
                    PageSize = Math.Max(1, request.PageSize),
                    TotalFilteredCount = 0,
                    ItemCount = 0
                }
            };
        }
    }

    /// <summary>
    /// Applies the correct filter combination for MediatorOnly, ExampleAppOnly, and HidePackets.
    /// </summary>
    private static IQueryable<TelemetryTrace> ApplyTraceFilters(
        IQueryable<TelemetryTrace> baseQuery,
        bool mediatorOnly,
        bool exampleAppOnly,
        bool hidePackets)
    {
        // Apply filters based on the combination
        if (mediatorOnly && exampleAppOnly)
        {
            baseQuery = baseQuery.Where(trace => trace.OperationName.StartsWith("Mediator")
                                         || trace.OperationName.StartsWith("OpenTelemetryExample"));
        }
        else if (mediatorOnly)
        {
            baseQuery = baseQuery.Where(trace => trace.OperationName.StartsWith("Mediator"));
        }
        else if (exampleAppOnly)
        {
            baseQuery = baseQuery.Where(trace => trace.OperationName.StartsWith("OpenTelemetryExample"));
        }

        // Apply hide packets filter
        if (hidePackets)
        {
            baseQuery = baseQuery.Where(trace => !trace.OperationName.Contains("Mediator.SendStream:") ||
                                               !trace.OperationName.Contains(".packet_"));
        }

        return baseQuery;
    }

    /// <summary>
    /// Groups traces by TraceId and organizes them hierarchically by ParentId.
    /// </summary>
    private List<TraceGroupDto> GroupTracesByTraceId(List<TelemetryTrace> traces)
    {
        // Group by TraceId
        var traceGroups = traces
            .GroupBy(t => t.TraceId)
            .OrderByDescending(g => g.Min(t => t.StartTime)) // Order by earliest trace in group
            .Select(group => CreateTraceGroup(group.Key, group.ToList()))
            .ToList();

        return traceGroups;
    }

    /// <summary>
    /// Creates a TraceGroupDto from a collection of traces with the same TraceId.
    /// </summary>
    private TraceGroupDto CreateTraceGroup(string traceId, List<TelemetryTrace> tracesInGroup)
    {
        // Find root trace (no parent or parent not in this group)
        var rootTrace = tracesInGroup.FirstOrDefault(t => string.IsNullOrEmpty(t.ParentId)
                                                          || tracesInGroup.All(other => other.SpanId != t.ParentId)) ??
                        tracesInGroup.OrderBy(t => t.StartTime).First();

        // Build hierarchy
        var hierarchicalTraces = BuildHierarchy(tracesInGroup, rootTrace.SpanId, 0);

        // Determine overall status - Error if any trace failed, otherwise OK
        var overallStatus = tracesInGroup.Any(t => IsErrorStatus(t.Status)) ? "Error" : "OK";

        return new TraceGroupDto
        {
            TraceId = traceId,
            RootTrace = ConvertToTraceDto(rootTrace),
            ChildTraces = hierarchicalTraces,
            TotalDuration = CalculateTotalDuration(tracesInGroup),
            Status = overallStatus,
            StartTime = tracesInGroup.Min(t => t.StartTime),
            TraceCount = tracesInGroup.Count,
            IsExpanded = true
        };
    }

    /// <summary>
    /// Builds a hierarchical structure of traces based on ParentId relationships.
    /// </summary>
    private List<HierarchicalTraceDto> BuildHierarchy(List<TelemetryTrace> allTraces, string parentSpanId, int level)
    {
        var children = allTraces
            .Where(t => t.ParentId == parentSpanId)
            .OrderBy(t => t.StartTime)
            .Select(trace => new HierarchicalTraceDto
            {
                Trace = ConvertToTraceDto(trace),
                Children = BuildHierarchy(allTraces, trace.SpanId, level + 1),
                Level = level,
                IsExpanded = true
            })
            .ToList();

        return children;
    }

    /// <summary>
    /// Converts a TelemetryTrace to a TraceDto.
    /// </summary>
    private static TraceDto ConvertToTraceDto(TelemetryTrace trace)
    {
        var isAppTrace = IsAppTrace(trace.OperationName);
        return new TraceDto
        {
            TraceId = trace.TraceId,
            SpanId = trace.SpanId,
            ParentId = trace.ParentId,
            OperationName = trace.OperationName,
            StartTime = trace.StartTime,
            Duration = trace.Duration,
            Status = NormalizeStatus(trace.Status),
            Tags = trace.Tags,
            Source = DetermineTraceSource(trace.OperationName, trace.Tags),
            IsMediatorTrace = !isAppTrace && IsMediatorTrace(trace.OperationName, trace.Tags),
            IsAppTrace = isAppTrace
        };
    }

    /// <summary>
    /// Calculates the total duration for a group of traces.
    /// </summary>
    private static TimeSpan CalculateTotalDuration(List<TelemetryTrace> traces)
    {
        if (!traces.Any()) return TimeSpan.Zero;

        var minStart = traces.Min(t => t.StartTime);
        var maxEnd = traces.Max(t => t.StartTime.Add(t.Duration));
        return maxEnd - minStart;
    }

    /// <summary>
    /// Checks if a status indicates an error.
    /// </summary>
    private static bool IsErrorStatus(string status)
    {
        return status.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
               status.Contains("Failed", StringComparison.OrdinalIgnoreCase) ||
               status.Contains("Exception", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Normalizes ActivityStatusCode values to user-friendly status names.
    /// </summary>
    private static string NormalizeStatus(string status)
    {
        return status.ToLower(CultureInfo.InvariantCulture) switch
        {
            "unset" => "Success", // ActivityStatusCode.Unset typically means successful completion
            "ok" => "Success",
            "error" => "Error",
            "cancelled" => "Cancelled",
            _ => status.Length > 0 ? char.ToUpper(status[0], CultureInfo.InvariantCulture) + status[1..].ToLower(CultureInfo.InvariantCulture) : "Unknown"
        };
    }

    /// <summary>
    /// Determines the source of a trace based on operation name and tags.
    /// </summary>
    private static string DetermineTraceSource(string operationName, Dictionary<string, object>? tags)
    {
        if (string.IsNullOrEmpty(operationName))
        {
            return "Unknown";
        }

        // Check operation name patterns first
        if (operationName.Contains("OpenTelemetryExample", StringComparison.OrdinalIgnoreCase) ||
            operationName.StartsWith("OpenTelemetryExample.", StringComparison.OrdinalIgnoreCase))
            return "OpenTelemetryExample";

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
                var key = tag.Key.ToLower(CultureInfo.InvariantCulture);
                var value = tag.Value.ToString()?.ToLower(CultureInfo.InvariantCulture) ?? "";

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

                // Check for ASP.NET Core specific tags
                if (key == "http.method" || key == "http.url" || key == "http.target")
                    return "ASP.NET Core";
            }
        }

        // Determine by operation name patterns
        if (operationName.Contains("Http", StringComparison.OrdinalIgnoreCase))
            return "ASP.NET Core";

        if (operationName.Contains("Get", StringComparison.OrdinalIgnoreCase) ||
            operationName.Contains("Post", StringComparison.OrdinalIgnoreCase) ||
            operationName.Contains("Put", StringComparison.OrdinalIgnoreCase) ||
            operationName.Contains("Delete", StringComparison.OrdinalIgnoreCase))
            return "ASP.NET Core";

        return "System";
    }

    /// <summary>
    /// Determines if a trace is from OpenTelemetryExample.
    /// </summary>
    private static bool IsAppTrace(string operationName)
    {
        // Check operation name patterns
        if (operationName.Contains("Example", StringComparison.OrdinalIgnoreCase) ||
            operationName.StartsWith("OpenTelemetryExample.", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
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
                var key = tag.Key.ToLower(CultureInfo.InvariantCulture);
                var value = tag.Value.ToString()?.ToLower(CultureInfo.InvariantCulture) ?? "";

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
}
