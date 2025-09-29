namespace TypedNotificationSubscriberExample.Middleware;

/// <summary>
/// Represents notification metrics.
/// </summary>
public class NotificationMetrics
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageDuration => TotalCount > 0 ? TimeSpan.FromTicks(TotalDuration.Ticks / TotalCount) : TimeSpan.Zero;
    public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;
}