namespace Blazing.Mediator;

/// <summary>
/// Shared interface for inspecting pipeline implementations for debugging and monitoring.
/// Provides common functionality for both request and notification middleware pipelines.
/// </summary>
public interface IPipelineInspector
{
    /// <summary>
    /// Gets the list of registered middleware types in order.
    /// </summary>
    /// <returns>Read-only list of middleware types</returns>
    IReadOnlyList<Type> GetRegisteredMiddleware();

    /// <summary>
    /// Gets configuration information for registered middleware.
    /// Each tuple contains the middleware type and its associated configuration object (if any).
    /// </summary>
    /// <returns>Read-only list of middleware configuration information</returns>
    IReadOnlyList<(Type Type, object? Configuration)> GetMiddlewareConfiguration();

    /// <summary>
    /// Gets detailed information about registered middleware including types, order values, and configuration.
    /// When a service provider is provided, gets actual runtime order values from DI-registered instances.
    /// </summary>
    /// <param name="serviceProvider">Optional service provider to resolve middleware instances for actual order values.</param>
    /// <returns>Read-only list of middleware information with type, order, and configuration</returns>
    IReadOnlyList<(Type Type, int Order, object? Configuration)> GetDetailedMiddlewareInfo(IServiceProvider? serviceProvider = null);

    /// <summary>
    /// Analyzes the middleware pipeline and returns structured information about each middleware component.
    /// Provides detailed analysis including order display formatting, class names, and type parameters.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve middleware instances for actual order values.</param>
    /// <param name="isDetailed">Indicates whether to include detailed analysis information. Defaults to true.</param>
    /// <returns>Read-only list of middleware analysis information sorted by execution order</returns>
    IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider, bool? isDetailed = true);
}