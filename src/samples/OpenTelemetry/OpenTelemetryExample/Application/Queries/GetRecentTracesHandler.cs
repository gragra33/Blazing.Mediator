using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Application.Queries;
using OpenTelemetryExample.Domain.Entities;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Shared.Models;
using System.Globalization;

namespace OpenTelemetryExample.Application.Handlers;

/// <summary>
/// Handler for GetRecentTracesQuery that retrieves real trace data from the database with server-side pagination.
/// </summary>
public sealed class GetRecentTracesHandler(ApplicationDbContext context, ILogger<GetRecentTracesHandler> logger)
    : IRequestHandler<GetRecentTracesQuery, RecentTracesDto>
{
    public async Task<RecentTracesDto> Handle(GetRecentTracesQuery request, CancellationToken cancellationToken)
    {
        var cutoffTime = DateTime.UtcNow - request.TimeWindow;

        try
        {
            // Validate and normalize pagination parameters
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Max(1, Math.Min(request.PageSize, 100)); // Cap at 100 items per page
            var skip = (page - 1) * pageSize;

            // Get total count for the timeframe first (before any filtering)
            var totalAvailableInTimeframe = await context.TelemetryTraces
                .Where(t => t.StartTime >= cutoffTime)
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);

            // Start with base query for the timeframe
            var baseQuery = context.TelemetryTraces
                .Where(t => t.StartTime >= cutoffTime);

            // Apply combined filter logic
            var filteredQuery = ApplyTraceFilters(baseQuery, request.MediatorOnly, request.ExampleAppOnly);

            // Get total filtered count (matching the filter criteria)
            var totalFilteredCount = await filteredQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            // Apply pagination and ordering - most recent first
            var pagedTraces = await filteredQuery
                .OrderByDescending(t => t.StartTime)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!pagedTraces.Any())
            {
                logger.LogInformation("No telemetry traces found in the last {TimeWindow} minutes with filter: MediatorOnly={MediatorOnly}, ExampleAppOnly={ExampleAppOnly}, Page={Page}",
                    request.TimeWindow.TotalMinutes, request.MediatorOnly, request.ExampleAppOnly, page);

                return new RecentTracesDto
                {
                    Timestamp = DateTime.UtcNow,
                    Traces = [],
                    Message = $"No trace data available in the last {request.TimeWindow.TotalMinutes:F0} minutes. Total traces available: {totalAvailableInTimeframe}",
                    TotalTracesInTimeframe = totalAvailableInTimeframe,
                    Pagination = new PaginationInfo
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalFilteredCount = totalFilteredCount,
                        ItemCount = 0
                    }
                };
            }

            var traceDtos = pagedTraces.Select(trace =>
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
            }).ToList();

            // Create pagination info
            var pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalFilteredCount = totalFilteredCount,
                ItemCount = traceDtos.Count
            };

            // Create detailed message with accurate counts and pagination info
            string filterMessage = $"MediatorOnly={request.MediatorOnly}, ExampleAppOnly={request.ExampleAppOnly} " +
                                 $"(page {pagination.Page} of {pagination.TotalPages}, showing {pagination.StartIndex}-{pagination.EndIndex} of {totalFilteredCount} filtered, total: {totalAvailableInTimeframe} available)";

            var result = new RecentTracesDto
            {
                Timestamp = DateTime.UtcNow,
                Traces = traceDtos,
                Message = $"Recent traces from the last {request.TimeWindow.TotalMinutes:F0} minutes - {filterMessage}",
                TotalTracesInTimeframe = totalAvailableInTimeframe,
                Pagination = pagination
            };

            logger.LogInformation("Retrieved page {Page} of recent traces (PageSize={PageSize}, MediatorOnly={MediatorOnly}, ExampleAppOnly={ExampleAppOnly}, filtered count: {FilteredCount}, total available: {TotalAvailable})",
                page, pageSize, request.MediatorOnly, request.ExampleAppOnly, totalFilteredCount, totalAvailableInTimeframe);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving recent traces");

            return new RecentTracesDto
            {
                Timestamp = DateTime.UtcNow,
                Traces = [],
                Message = $"Error retrieving traces: {ex.Message}",
                TotalTracesInTimeframe = 0,
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
    /// Applies the correct filter combination for MediatorOnly and ExampleAppOnly.
    /// </summary>
    private static IQueryable<TelemetryTrace> ApplyTraceFilters(
        IQueryable<TelemetryTrace> baseQuery,
        bool mediatorOnly,
        bool exampleAppOnly)
    {
        // Apply filters based on the combination
        if (mediatorOnly && exampleAppOnly)
        {
            return baseQuery.Where(trace => trace.OperationName.StartsWith("Mediator")
                                         || trace.OperationName.StartsWith("OpenTelemetryExample"));
        }

        if (mediatorOnly)
        {
            return baseQuery.Where(trace => trace.OperationName.StartsWith("Mediator"));
        }

        if (exampleAppOnly)
        {
            return baseQuery.Where(trace => trace.OperationName.StartsWith("OpenTelemetryExample"));
        }

        return baseQuery;
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
