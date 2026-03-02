namespace Blazing.Mediator;

/// <summary>
/// Handler for requests that don't return a value (Command Handlers)
/// </summary>
/// <typeparam name="TRequest">The type of request</typeparam>
public interface IRequestHandler<in TRequest> where TRequest : IRequest
{
    /// <summary>
    /// Handles the specified request.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
    ValueTask Handle(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler for requests that return a value (Query Handlers)
/// </summary>
/// <typeparam name="TRequest">The type of request</typeparam>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request and returns a response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask{TResponse}"/> that represents the asynchronous operation and contains the response.</returns>
    ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}
