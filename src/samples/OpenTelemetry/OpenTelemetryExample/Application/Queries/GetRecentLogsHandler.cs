using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for retrieving recent telemetry logs with filtering, pagination, and summary statistics.
/// </summary>
internal sealed class GetRecentLogsHandler(ApplicationDbContext context, ILogger<GetRecentLogsHandler> logger)
    : IQueryHandler<GetRecentLogsQuery, RecentLogsDto>
{
    public async Task<RecentLogsDto> Handle(GetRecentLogsQuery request, CancellationToken cancellationToken = default)
    {
        // Use LoggerMessage delegate for CA1848
        LogRetrievingRecentLogs(logger, request.TimeWindowMinutes, request.MinimumLogLevel ?? string.Empty, request.SearchText ?? string.Empty, request.Page, request.PageSize, null);
        var cutoffTime = DateTime.UtcNow.AddMinutes(-request.TimeWindowMinutes);

        // Start with base query
        var query = context.TelemetryLogs
            .Where(log => log.Timestamp >= cutoffTime)
            .AsQueryable();

        // Apply filters
        if (request.AppOnly)
        {
            query = query.Where(log => log.Source == "Application" || log.Source == "Controller");
        }
        else if (request.MediatorOnly)
        {
            query = query.Where(log => log.Source == "Mediator");
        }

        if (request.ErrorsOnly)
        {
            query = query.Where(log => log.LogLevel == "Error" || log.LogLevel == "Critical" || log.Exception != null);
        }

        if (!string.IsNullOrEmpty(request.MinimumLogLevel))
        {
            query = query.Where(log => GetLogLevelPriority(log.LogLevel) >= GetLogLevelPriority(request.MinimumLogLevel));
        }

        if (!string.IsNullOrEmpty(request.SearchText))
        {
            var searchUpper = request.SearchText.ToUpperInvariant();
            query = query.Where(log =>
                log.Message != null && log.Message.ToUpperInvariant().Contains(searchUpper, StringComparison.Ordinal) ||
                log.Category != null && log.Category.ToUpperInvariant().Contains(searchUpper, StringComparison.Ordinal) ||
                (log.Exception != null && log.Exception.ToUpperInvariant().Contains(searchUpper, StringComparison.Ordinal)));
        }

        // Get total count for pagination
        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        // Calculate summary statistics before pagination
        var summary = await CalculateSummaryAsync(query, cancellationToken).ConfigureAwait(false);

        // Apply pagination and ordering
        var logs = await query
            .OrderByDescending(log => log.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(log => new LogDto
            {
                Id = log.Id,
                Timestamp = log.Timestamp,
                LogLevel = log.LogLevel,
                Category = log.Category,
                Message = log.Message,
                Exception = log.Exception,
                TraceId = log.TraceId,
                SpanId = log.SpanId,
                Source = log.Source,
                Tags = log.Tags,
                MachineName = log.MachineName,
                ProcessId = log.ProcessId,
                ThreadId = log.ThreadId,
                EventId = log.EventId,
                Scopes = log.Scopes
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var pagination = new PaginationInfo
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalFilteredCount = totalCount,
            ItemCount = logs.Count
        };

        var filters = new LogFilters
        {
            TimeWindowMinutes = request.TimeWindowMinutes,
            AppOnly = request.AppOnly,
            MediatorOnly = request.MediatorOnly,
            ErrorsOnly = request.ErrorsOnly,
            MinimumLogLevel = request.MinimumLogLevel,
            SearchText = request.SearchText
        };

        // Use LoggerMessage delegate for CA1848
        LogRetrievedLogs(logger, logs.Count, totalCount, null);

        return new RecentLogsDto
        {
            Logs = logs,
            Pagination = pagination,
            Filters = filters,
            Summary = summary
        };
    }

    private async Task<LogSummary> CalculateSummaryAsync(IQueryable<Domain.Entities.TelemetryLog> query, CancellationToken cancellationToken)
    {
        var summary = new LogSummary
        {
            TotalLogs = await query.CountAsync(cancellationToken).ConfigureAwait(false),
            ErrorLogs = await query.CountAsync(log => log.LogLevel == "Error" || log.LogLevel == "Critical", cancellationToken).ConfigureAwait(false),
            WarningLogs = await query.CountAsync(log => log.LogLevel == "Warning", cancellationToken).ConfigureAwait(false),
            InfoLogs = await query.CountAsync(log => log.LogLevel == "Information", cancellationToken).ConfigureAwait(false),
            DebugLogs = await query.CountAsync(log => log.LogLevel == "Debug" || log.LogLevel == "Trace", cancellationToken).ConfigureAwait(false),
            LogsWithExceptions = await query.CountAsync(log => log.Exception != null, cancellationToken).ConfigureAwait(false),
            AppLogs = await query.CountAsync(log => log.Source == "Application" || log.Source == "Controller", cancellationToken).ConfigureAwait(false),
            MediatorLogs = await query.CountAsync(log => log.Source == "Mediator", cancellationToken).ConfigureAwait(false)
        };

        return summary;
    }

    private static int GetLogLevelPriority(string logLevel)
    {
        return logLevel switch
        {
            "Trace" => 0,
            "Debug" => 1,
            "Information" => 2,
            "Warning" => 3,
            "Error" => 4,
            "Critical" => 5,
            _ => 2 // Default to Information level
        };
    }

    // LoggerMessage delegates for CA1848
    private static readonly Action<ILogger, int, string, string, int, int, Exception?> LogRetrievingRecentLogs =
        LoggerMessage.Define<int, string, string, int, int>(
            LogLevel.Debug,
            new EventId(1, nameof(LogRetrievingRecentLogs)),
            "Retrieving recent logs with filters: TimeWindow={TimeWindow}min, MinLevel={MinLevel}, Search='{Search}', Page={Page}, PageSize={PageSize}");

    private static readonly Action<ILogger, int, int, Exception?> LogRetrievedLogs =
        LoggerMessage.Define<int, int>(
            LogLevel.Debug,
            new EventId(2, nameof(LogRetrievedLogs)),
            "Retrieved {LogCount} logs out of {TotalCount} total logs matching filters");
}
