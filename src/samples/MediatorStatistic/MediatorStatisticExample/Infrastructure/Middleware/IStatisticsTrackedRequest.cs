namespace MediatorStatisticExample.Infrastructure.Middleware;

/// <summary>
/// Marker interface for requests that should be tracked by the statistics snapshot middleware.
/// Implement this interface on any request type that requires statistics capture.
/// </summary>
public interface IStatisticsTrackedRequest
{
}
