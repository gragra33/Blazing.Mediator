namespace Blazing.Mediator.Abstractions;

/// <summary>
/// Stream pipeline delegate that represents the next middleware in the pipeline.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
/// <returns>An async enumerable of response items from the next middleware or handler</returns>
public delegate IAsyncEnumerable<TResponse> StreamRequestHandlerDelegate<TResponse>();
