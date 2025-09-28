using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Infrastructure.Telemetry;
using OpenTelemetryExample.Shared.Models;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for GetUsersQuery using Entity Framework Core.
/// Demonstrates OpenTelemetry best practices with static ActivitySource usage.
/// </summary>
internal sealed class GetUsersHandler(ApplicationDbContext context, ILogger<GetUsersHandler> logger) : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private const string ActivityName = "GetUsersHandler.Handle";

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken = default)
    {
        LogProcessing(logger, request.IncludeInactive, request.SearchTerm ?? "none", null);
        using var activity = ApplicationActivitySources.Handlers.StartActivity(ActivityName);
        activity?.SetTag("handler.name", nameof(GetUsersHandler));
        activity?.SetTag("handler.type", "QueryHandler");
        activity?.SetTag("query.include_inactive", request.IncludeInactive);
        activity?.SetTag("query.search_term", request.SearchTerm ?? "none");
        activity?.SetTag("query.has_search_filter", !string.IsNullOrEmpty(request.SearchTerm));
        activity?.SetTag("operation.type", "get_users");
        activity?.SetTag("data.source", "database");
        try
        {
            activity?.AddEvent(new ActivityEvent("handler.execution.started", DateTimeOffset.UtcNow));
            var ticks = DateTime.UtcNow.Ticks;
            var delay = (int)(ticks % 151) + 50; // 50-200ms, deterministic, not Random
            LogSimulatingDelay(logger, delay, null);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            var query = context.Users.AsNoTracking();
            LogBuildingQuery(logger, null);
            if (!request.IncludeInactive)
            {
                query = query.Where(u => u.IsActive);
                LogAppliedActiveFilter(logger, null);
                activity?.SetTag("query.active_filter_applied", true);
            }
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(u => u.Name.Contains(request.SearchTerm)
                                         || u.Email.Contains(request.SearchTerm));
                LogAppliedSearchFilter(logger, request.SearchTerm, null);
                activity?.SetTag("query.search_filter_applied", true);
                activity?.SetTag("query.search_term_length", request.SearchTerm.Length);
            }
            LogExecutingQuery(logger, null);
            var users = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            activity?.SetTag("handler.result", "success");
            activity?.SetTag("query.user_count", users.Count);
            activity?.SetTag("query.has_results", users.Count > 0);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.AddEvent(new ActivityEvent("handler.execution.completed", DateTimeOffset.UtcNow,
                new ActivityTagsCollection
                {
                    ["user_count"] = users.Count,
                    ["query.completed"] = true
                }));
            LogSuccess(logger, users.Count, null);
            var userDtos = users.Select(u => u.ToDto()).ToList();
            LogConvertedToDtos(logger, userDtos.Count, null);
            return userDtos;
        }
        catch (OperationCanceledException)
        {
            LogCancelled(logger, null);
            activity?.SetTag("handler.result", "cancelled");
            activity?.SetStatus(ActivityStatusCode.Error, "Operation cancelled");
            activity?.AddEvent(new ActivityEvent("handler.cancelled", DateTimeOffset.UtcNow));
            throw;
        }
        catch (Exception ex)
        {
            LogError(logger, ex, null);
            activity?.SetTag("handler.result", "error");
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent("handler.exception", DateTimeOffset.UtcNow,
                new ActivityTagsCollection
                {
                    ["exception.type"] = ex.GetType().Name,
                    ["exception.source"] = ex.Source ?? "unknown"
                }));
            throw;
        }
    }
    // LoggerMessage delegates for CA1848
    private static readonly Action<ILogger, bool, string, Exception?> LogProcessing =
        LoggerMessage.Define<bool, string>(
            LogLevel.Information,
            new EventId(1, nameof(LogProcessing)),
            "Processing GetUsersQuery with IncludeInactive: {IncludeInactive}, SearchTerm: {SearchTerm}");
    private static readonly Action<ILogger, int, Exception?> LogSimulatingDelay =
        LoggerMessage.Define<int>(
            LogLevel.Debug,
            new EventId(2, nameof(LogSimulatingDelay)),
            "Simulating database processing delay of {DelayMs}ms");
    private static readonly Action<ILogger, Exception?> LogBuildingQuery =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(3, nameof(LogBuildingQuery)),
            "Building user query with filters");
    private static readonly Action<ILogger, Exception?> LogAppliedActiveFilter =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(4, nameof(LogAppliedActiveFilter)),
            "Applied active users filter");
    private static readonly Action<ILogger, string, Exception?> LogAppliedSearchFilter =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(5, nameof(LogAppliedSearchFilter)),
            "Applied search term filter: {SearchTerm}");
    private static readonly Action<ILogger, Exception?> LogExecutingQuery =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(6, nameof(LogExecutingQuery)),
            "Executing database query to retrieve users");
    private static readonly Action<ILogger, int, Exception?> LogSuccess =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(7, nameof(LogSuccess)),
            "Successfully retrieved {UserCount} users from database");
    private static readonly Action<ILogger, int, Exception?> LogConvertedToDtos =
        LoggerMessage.Define<int>(
            LogLevel.Debug,
            new EventId(8, nameof(LogConvertedToDtos)),
            "Converted {UserCount} users to DTOs");
    private static readonly Action<ILogger, Exception?> LogCancelled =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(9, nameof(LogCancelled)),
            "GetUsersQuery operation was cancelled");
    private static readonly Action<ILogger, Exception, Exception?> LogError =
        LoggerMessage.Define<Exception>(
            LogLevel.Error,
            new EventId(10, nameof(LogError)),
            "Error occurred while processing GetUsersQuery: {Exception}");
}
