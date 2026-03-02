using Blazing.Mediator.Statistics;

namespace TypedNotificationSubscriberExample.Services;

/// <summary>
/// Helper class for analyzing notification middleware pipeline.
/// Uses the compile-time <see cref="IMediatorTypeCatalog"/> for AOT-safe, zero-reflection analysis.
/// </summary>
public static class NotificationPipelineAnalyzer
{
    /// <summary>
    /// Analyzes the notification middleware pipeline and returns detailed information.
    /// Uses the compile-time catalog — no reflection, fully AOT-compatible.
    /// </summary>
    public static List<NotificationMiddlewareInfo> AnalyzeMiddleware(
        IMediatorTypeCatalog catalog,
        MediatorStatistics mediatorStatistics)
    {
        return mediatorStatistics
            .AnalyzeNotificationMiddleware(catalog)
            .Select(m => new NotificationMiddlewareInfo(
                m.Order,
                m.OrderDisplay,
                m.ClassName,
                m.TypeParameters,
                m.GenericConstraints))
            .OrderBy(m => m.Order)
            .ToList();
    }

}