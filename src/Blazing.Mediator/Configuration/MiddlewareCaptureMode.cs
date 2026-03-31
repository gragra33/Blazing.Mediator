namespace Blazing.Mediator.Configuration;

/// <summary>
/// Controls what middleware information is captured on request telemetry spans.
/// </summary>
public enum MiddlewareCaptureMode
{
    /// <summary>
    /// No middleware tags are emitted. Zero overhead on the hot path.
    /// </summary>
    None = 0,

    /// <summary>
    /// Emits the statically applicable middleware pipeline — what is configured to run
    /// for this request type. Reads from the pipeline builder; does not track conditional skips.
    /// Tags: <c>request_middleware.pipeline</c>, <c>request_middleware.count</c>,
    /// <c>request_middleware.orders</c>, <c>request_middleware.capture_mode=applicable</c>.
    /// </summary>
    Applicable = 1,

    /// <summary>
    /// Emits a runtime execution summary — distinguishes middleware that actually ran from
    /// those skipped by <see cref="IConditionalMiddleware{TRequest, TResponse}"/>. Opt-in
    /// diagnostic mode with higher overhead than <see cref="Applicable"/>; not recommended
    /// as a production default.
    /// Tags: <c>request_middleware.executed_pipeline</c>, <c>request_middleware.executed_count</c>,
    /// <c>request_middleware.skipped_pipeline</c>, <c>request_middleware.skipped_count</c>,
    /// <c>request_middleware.capture_mode=executed</c>.
    /// </summary>
    Executed = 2
}
