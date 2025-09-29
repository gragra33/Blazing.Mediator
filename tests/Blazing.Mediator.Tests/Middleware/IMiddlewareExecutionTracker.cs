namespace Blazing.Mediator.Tests.Middleware;

/// <summary>
/// Interface for tracking middleware execution during pipeline processing.
/// This is used to capture which middleware were actually executed for telemetry purposes.
/// </summary>
public interface IMiddlewareExecutionTracker
{
    /// <summary>
    /// Records that a middleware was executed.
    /// </summary>
    /// <param name="middlewareType">The type of middleware that was executed.</param>
    void RecordExecution(Type middlewareType);

    /// <summary>
    /// Gets the list of middleware types that have been executed.
    /// </summary>
    /// <returns>A list of middleware type names in execution order.</returns>
    IReadOnlyList<string> GetExecutedMiddleware();

    /// <summary>
    /// Clears the execution history.
    /// </summary>
    void Clear();
}