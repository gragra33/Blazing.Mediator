namespace Blazing.Mediator.Abstractions;

/// <summary>
/// Interface for middleware that should only execute under certain conditions.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IConditionalMiddleware<in TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Determines whether this middleware should execute for the given request.
    /// </summary>
    /// <param name="request">The request to evaluate</param>
    /// <returns>True if the middleware should execute, false to skip</returns>
    bool ShouldExecute(TRequest request);
}

/// <summary>
/// Interface for command middleware that should only execute under certain conditions.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
/// <typeparam name="TRequest">The command type</typeparam>
public interface IConditionalMiddleware<in TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Determines whether this middleware should execute for the given command.
    /// </summary>
    /// <param name="request">The command to evaluate</param>
    /// <returns>True if the middleware should execute, false to skip</returns>
    bool ShouldExecute(TRequest request);
}
