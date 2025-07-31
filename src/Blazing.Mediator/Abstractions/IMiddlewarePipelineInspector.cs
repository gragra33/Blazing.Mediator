namespace Blazing.Mediator.Abstractions;

/// <summary>
/// Interface for inspecting the middleware pipeline for debugging and monitoring.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
public interface IMiddlewarePipelineInspector
{
    /// <summary>
    /// Gets the list of registered middleware types in order.
    /// </summary>
    /// <returns>Read-only list of middleware types</returns>
    IReadOnlyList<Type> GetRegisteredMiddleware();

    /// <summary>
    /// Gets configuration information for registered middleware.
    /// </summary>
    /// <returns>Dictionary of middleware type to configuration object</returns>
    IReadOnlyDictionary<Type, object?> GetMiddlewareConfiguration();

    /// <summary>
    /// Gets detailed information about registered middleware including types, order values, and configuration.
    /// When a service provider is provided, gets actual runtime order values from DI-registered instances.
    /// </summary>
    /// <param name="serviceProvider">Optional service provider to resolve middleware instances for actual order values.</param>
    /// <returns>Read-only list of middleware information with type, order, and configuration</returns>
    IReadOnlyList<(Type Type, int Order, object? Configuration)> GetDetailedMiddlewareInfo(IServiceProvider? serviceProvider = null);
}
