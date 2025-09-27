namespace Blazing.Mediator.Abstractions;

/// <summary>
/// Interface for inspecting the notification middleware pipeline for debugging and monitoring.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
public interface INotificationMiddlewarePipelineInspector
{
    /// <summary>
    /// Gets a read-only list of registered notification middleware types.
    /// </summary>
    /// <returns>Read-only list of notification middleware types in the order they were registered</returns>
    IReadOnlyList<Type> GetRegisteredMiddleware();

    /// <summary>
    /// Gets configuration information for registered notification middleware.
    /// Each tuple contains the middleware type and its associated configuration object (if any).
    /// </summary>
    /// <returns>Read-only list of middleware configuration information</returns>
    IReadOnlyList<(Type Type, object? Configuration)> GetMiddlewareConfiguration();

    /// <summary>
    /// Gets detailed information about registered notification middleware including types, order values, and configuration.
    /// When a service provider is provided, gets actual runtime order values from DI-registered instances.
    /// </summary>
    /// <param name="serviceProvider">Optional service provider to resolve middleware instances for actual order values.</param>
    /// <returns>Read-only list of notification middleware information with type, order, and configuration</returns>
    IReadOnlyList<(Type Type, int Order, object? Configuration)> GetDetailedMiddlewareInfo(IServiceProvider? serviceProvider = null);

    /// <summary>
    /// Analyzes the notification middleware pipeline and returns structured information about each middleware component.
    /// Provides detailed analysis including order display formatting, class names, type parameters, and constraint information.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve middleware instances for actual order values.</param>
    /// <param name="isDetailed">Indicates whether to include detailed analysis information. Defaults to true.</param>
    /// <returns>Read-only list of middleware analysis information sorted by execution order</returns>
    IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider, bool? isDetailed = true);

    /// <summary>
    /// Analyzes the notification middleware pipeline and returns structured information about each middleware component.
    /// Provides detailed analysis including order display formatting, class names, and type parameters.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve middleware instances for actual order values.</param>
    /// <returns>Read-only list of middleware analysis information sorted by execution order</returns>
    IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider);
}
