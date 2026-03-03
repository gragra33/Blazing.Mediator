namespace MediatorStatisticExample.Middleware;

/// <summary>
/// Type-constrained middleware that captures a statistics snapshot for requests
/// implementing <see cref="IStatisticsTrackedRequest"/> that return a response.
/// Demonstrates how to use type constraints to apply middleware selectively.
/// </summary>
/// <typeparam name="TRequest">The request type, constrained to tracked requests.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class StatisticsSnapshotMiddleware<TRequest, TResponse>(
    ILogger<StatisticsSnapshotMiddleware<TRequest, TResponse>> logger,
    IMediator mediator)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IStatisticsTrackedRequest
{
    /// <inheritdoc />
    public int Order => 10;

    /// <inheritdoc />
    public async ValueTask<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogDebug("[StatisticsSnapshot] >> Capturing pre-execution snapshot for: {RequestName}", requestName);

        var response = await next().ConfigureAwait(false);

        logger.LogDebug("[StatisticsSnapshot] << Captured post-execution snapshot for: {RequestName}", requestName);

        return response;
    }
}

/// <summary>
/// Type-constrained middleware that captures a statistics snapshot for void requests
/// implementing <see cref="IStatisticsTrackedRequest"/>.
/// Demonstrates how to use type constraints to apply middleware selectively.
/// </summary>
/// <typeparam name="TRequest">The request type, constrained to tracked void requests.</typeparam>
public class StatisticsSnapshotMiddleware<TRequest>(
    ILogger<StatisticsSnapshotMiddleware<TRequest>> logger,
    IMediator mediator)
    : IRequestMiddleware<TRequest>
    where TRequest : IRequest, IStatisticsTrackedRequest
{
    /// <inheritdoc />
    public int Order => 10;

    /// <inheritdoc />
    public async ValueTask HandleAsync(
        TRequest request,
        RequestHandlerDelegate next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogDebug("[StatisticsSnapshot] >> Capturing pre-execution snapshot for: {RequestName}", requestName);

        await next().ConfigureAwait(false);

        logger.LogDebug("[StatisticsSnapshot] << Captured post-execution snapshot for: {RequestName}", requestName);
    }
}
