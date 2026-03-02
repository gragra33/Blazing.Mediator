using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazing.Mediator;

public sealed partial class Mediator
{
    /// <summary>
    /// Helper method to check if performance counters are enabled in statistics options.
    /// </summary>
    private bool HasPerformanceCountersEnabled()
        => _statistics?.Options is { EnablePerformanceCounters: true };

    /// <summary>
    /// Helper method to check if detailed analysis is enabled in statistics options.
    /// </summary>
    private bool HasDetailedAnalysisEnabled()
        => _statistics?.Options is { EnableDetailedAnalysis: true };

    /// <summary>
    /// Helper method to check if middleware metrics are enabled in statistics options.
    /// </summary>
    private bool HasMiddlewareMetricsEnabled()
        => _statistics?.Options is { EnableMiddlewareMetrics: true };
}