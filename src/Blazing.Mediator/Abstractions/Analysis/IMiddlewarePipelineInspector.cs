namespace Blazing.Mediator;

/// <summary>
/// Interface for inspecting the middleware pipeline for debugging and monitoring.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
public interface IMiddlewarePipelineInspector : IPipelineInspector
{
    // All methods are now inherited from IPipelineInspector
    // This maintains 100% backward compatibility while enabling shared references
}
