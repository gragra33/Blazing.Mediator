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
}
