namespace Blazing.Mediator.Middleware;

/// <summary>
/// Thread-static context that tracks which middleware actually executed versus was conditionally
/// skipped during a single mediator send operation in <see cref="Configuration.MiddlewareCaptureMode.Executed"/> mode.
/// </summary>
/// <remarks>
/// <para>
/// Instances are created and set by <see cref="TelemetryMiddleware{TRequest,TResponse}"/> (and its void variant)
/// immediately before the pipeline delegate is invoked, and cleared in the <c>finally</c> block after
/// the pipeline completes (even on exception). The <see cref="Current"/> slot is <see langword="null"/>
/// in <see cref="Configuration.MiddlewareCaptureMode.None"/> and
/// <see cref="Configuration.MiddlewareCaptureMode.Applicable"/> modes — all call-sites use the
/// null-conditional <c>?.RecordX()</c> pattern, so there is zero overhead when the context is absent.
/// </para>
/// <para>
/// An <see cref="AsyncLocal{T}"/> backing field is used instead of <c>[ThreadStatic]</c> so the
/// context flows correctly through <c>await</c> continuations that resume on a different thread-pool
/// thread. Concurrent sends on different logical async flows do not interfere. <see cref="ClearCurrent"/>
/// in the <c>finally</c> block resets the slot for any child async operations launched after the
/// telemetry middleware returns.
/// </para>
/// </remarks>
internal sealed class MiddlewareExecutionContext
{
    // AsyncLocal flows the context through async continuations regardless of which
    // thread-pool thread resumes the awaitable — unlike [ThreadStatic] which is reset
    // when the continuation runs on a different thread.
    private static readonly AsyncLocal<MiddlewareExecutionContext?> _current = new();

    /// <summary>
    /// Gets the middleware types that were invoked (i.e. their <c>HandleAsync</c> was called)
    /// during the current send operation.
    /// </summary>
    public List<Type> Executed { get; } = [];

    /// <summary>
    /// Gets the middleware types that were evaluated but skipped because
    /// <see cref="IConditionalMiddleware{TRequest,TResponse}.ShouldExecute"/> returned <see langword="false"/>.
    /// </summary>
    public List<Type> Skipped { get; } = [];

    /// <summary>
    /// Gets the <see cref="MiddlewareExecutionContext"/> for the current async execution context,
    /// or <see langword="null"/> when no tracking context is active.
    /// </summary>
    public static MiddlewareExecutionContext? Current => _current.Value;

    /// <summary>
    /// Sets <paramref name="ctx"/> as the tracking context for the current async execution context
    /// and returns it. Must be paired with a <see cref="ClearCurrent"/> call in a <c>finally</c> block.
    /// </summary>
    /// <param name="ctx">The context instance to install.</param>
    /// <returns>The same <paramref name="ctx"/> instance, for assignment convenience.</returns>
    public static MiddlewareExecutionContext SetCurrent(MiddlewareExecutionContext ctx)
    {
        _current.Value = ctx;
        return ctx;
    }

    /// <summary>
    /// Clears the tracking context for the current async execution context.
    /// Always called from the <c>finally</c> block in <see cref="TelemetryMiddleware{TRequest,TResponse}"/>.
    /// </summary>
    public static void ClearCurrent() => _current.Value = null;

    /// <summary>Records that the middleware of <paramref name="middlewareType"/> executed normally.</summary>
    /// <param name="middlewareType">The closed middleware type that ran.</param>
    public void RecordExecuted(Type middlewareType) => Executed.Add(middlewareType);

    /// <summary>
    /// Records that the middleware of <paramref name="middlewareType"/> was skipped because
    /// <c>IConditionalMiddleware.ShouldExecute</c> returned <see langword="false"/>.
    /// </summary>
    /// <param name="middlewareType">The closed middleware type that was skipped.</param>
    public void RecordSkipped(Type middlewareType) => Skipped.Add(middlewareType);
}
