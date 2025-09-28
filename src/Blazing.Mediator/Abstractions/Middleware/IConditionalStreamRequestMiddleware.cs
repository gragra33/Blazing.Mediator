namespace Blazing.Mediator;

/// <summary>
/// Interface for stream request middleware that should only execute under certain conditions.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
/// <typeparam name="TRequest">The stream request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IConditionalStreamRequestMiddleware<in TRequest, TResponse> : IStreamRequestMiddleware<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    /// Determines whether this middleware should execute for the given stream request.
    /// </summary>
    /// <param name="request">The stream request to evaluate</param>
    /// <returns>True if the middleware should execute, false to skip</returns>
    bool ShouldExecute(TRequest request);
}
