namespace TypedMiddlewareExample.Services;

/// <summary>
/// Helper class for analyzing middleware pipeline configuration.
/// </summary>
public static class MiddlewarePipelineAnalyzer
{
    /// <summary>
    /// Analyzes and returns ordered middleware information.
    /// </summary>
    /// <param name="pipelineInspector">The pipeline inspector service.</param>
    /// <param name="serviceProvider">The service provider for resolving middleware instances.</param>
    /// <returns>Ordered list of middleware information.</returns>
    public static List<MiddlewareInfo> AnalyzeMiddleware(IMiddlewarePipelineInspector pipelineInspector, IServiceProvider serviceProvider)
    {
        var middlewareAnalysis = pipelineInspector.AnalyzeMiddleware(serviceProvider);
        var middlewareInfos = new List<MiddlewareInfo>();

        foreach (var analysis in middlewareAnalysis)
        {
            var info = new MiddlewareInfo(
                analysis.Order,
                FormatOrderValue(analysis.Order),
                analysis.ClassName,
                analysis.TypeParameters,
                analysis.GenericConstraints
            );
            middlewareInfos.Add(info);
        }

        // Already sorted by the inspector, but sort again to be sure
        return middlewareInfos.OrderBy(m => m.Order).ToList();
    }

    /// <summary>
    /// Formats order values for display.
    /// </summary>
    private static string FormatOrderValue(int order)
    {
        return order switch
        {
            int.MinValue => "int.MinValue",
            int.MaxValue => "int.MaxValue",
            _ => order.ToString()
        };
    }
}

/// <summary>
/// Information about a middleware component.
/// </summary>
/// <param name="Order">The execution order.</param>
/// <param name="OrderDisplay">The formatted order display.</param>
/// <param name="ClassName">The middleware class name.</param>
/// <param name="TypeParameters">The generic type parameters.</param>
/// <param name="GenericConstraints">The generic constraints.</param>
public record MiddlewareInfo(
    int Order,
    string OrderDisplay,
    string ClassName,
    string TypeParameters,
    string GenericConstraints
);