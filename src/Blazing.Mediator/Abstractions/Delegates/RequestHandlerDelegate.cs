namespace Blazing.Mediator;

/// <summary>
/// Generic pipeline delegate that represents the next middleware in the pipeline.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
/// <returns>The response from the next middleware or handler</returns>
public delegate ValueTask<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Pipeline delegate that represents the next middleware in the pipeline for void commands.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
/// <returns>A <see cref="ValueTask"/> representing the completion of the command</returns>
public delegate ValueTask RequestHandlerDelegate();
