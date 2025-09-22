using Blazing.Mediator;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Query to retrieve recent telemetry logs with filtering and pagination capabilities.
/// </summary>
public sealed record GetRecentLogsQuery(
    int TimeWindowMinutes = 30,
    bool AppOnly = false,
    bool MediatorOnly = false,
    bool ErrorsOnly = false,
    string? MinimumLogLevel = null,
    string? SearchText = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<RecentLogsDto>;