using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for retrieving recent telemetry logs with filtering, pagination, and summary statistics.
/// </summary>
public sealed class GetRecentLogsHandler(ApplicationDbContext context, ILogger<GetRecentLogsHandler> logger)
    : IQueryHandler<GetRecentLogsQuery, RecentLogsDto>
{
    public async Task<RecentLogsDto> Handle(GetRecentLogsQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Retrieving recent logs with filters: TimeWindow={TimeWindow}min, AppOnly={AppOnly}, MediatorOnly={MediatorOnly}, ErrorsOnly={ErrorsOnly}, MinLevel={MinLevel}, Search='{Search}', Page={Page}, PageSize={PageSize}",
            request.TimeWindowMinutes, request.AppOnly, request.MediatorOnly, request.ErrorsOnly, 
            request.MinimumLogLevel, request.SearchText, request.Page, request.PageSize);

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
            var searchLower = request.SearchText.ToLower();
            query = query.Where(log => 
                log.Message.ToLower().Contains(searchLower) ||
                log.Category.ToLower().Contains(searchLower) ||
                (log.Exception != null && log.Exception.ToLower().Contains(searchLower)));
        }

        // Get total count for pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Calculate summary statistics before pagination
        var summary = await CalculateSummaryAsync(query, cancellationToken);

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
            .ToListAsync(cancellationToken);

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

        logger.LogDebug("Retrieved {LogCount} logs out of {TotalCount} total logs matching filters", 
            logs.Count, totalCount);

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
            TotalLogs = await query.CountAsync(cancellationToken),
            ErrorLogs = await query.CountAsync(log => log.LogLevel == "Error" || log.LogLevel == "Critical", cancellationToken),
            WarningLogs = await query.CountAsync(log => log.LogLevel == "Warning", cancellationToken),
            InfoLogs = await query.CountAsync(log => log.LogLevel == "Information", cancellationToken),
            DebugLogs = await query.CountAsync(log => log.LogLevel == "Debug" || log.LogLevel == "Trace", cancellationToken),
            LogsWithExceptions = await query.CountAsync(log => log.Exception != null, cancellationToken),
            AppLogs = await query.CountAsync(log => log.Source == "Application" || log.Source == "Controller", cancellationToken),
            MediatorLogs = await query.CountAsync(log => log.Source == "Mediator", cancellationToken)
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
}