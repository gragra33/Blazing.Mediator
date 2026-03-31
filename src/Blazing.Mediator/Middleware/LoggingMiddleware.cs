namespace Blazing.Mediator.Middleware;

/// <summary>
/// Optional structured debug-logging middleware for request-with-response handlers.
/// Opens a log scope for the request, logs request type at <see cref="LogLevel.Debug"/>,
/// and logs exceptions at <see cref="LogLevel.Error"/>.
/// <para>
/// Register via the generated <c>AddMediator(MediatorConfiguration config)</c> when
/// <c>config.LoggingOptions != null</c>. When not registered, zero overhead is incurred.
/// </para>
/// </summary>
/// <typeparam name="TRequest">The request type (query or command with response).</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
[Order(int.MinValue + 1)]
public sealed class LoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly string _requestName = typeof(TRequest).Name;

    private readonly ILogger<LoggingMiddleware<TRequest, TResponse>> _logger;
    private readonly LoggingOptions _options;

    /// <summary>
    /// Initialises a new <see cref="LoggingMiddleware{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">Logging options controlling verbosity.</param>
    public LoggingMiddleware(
        ILogger<LoggingMiddleware<TRequest, TResponse>> logger,
        LoggingOptions options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public async ValueTask<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!(_options.EnableSend && _logger.IsEnabled(LogLevel.Debug)))
            return await next().ConfigureAwait(false);

        _logger.LogDebug("Mediator: dispatching {RequestName}", _requestName);

        try
        {
            var result = await next().ConfigureAwait(false);
            _logger.LogDebug("Mediator: {RequestName} completed successfully", _requestName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mediator: {RequestName} failed with {ExceptionType}", _requestName, ex.GetType().Name);
            throw;
        }
    }
}

/// <summary>
/// Optional structured debug-logging middleware for void-command handlers.
/// Behaves identically to <see cref="LoggingMiddleware{TRequest, TResponse}"/> but for commands
/// that return no value.
/// </summary>
/// <typeparam name="TRequest">The void command type.</typeparam>
[Order(int.MinValue + 1)]
public sealed class LoggingMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    private static readonly string _requestName = typeof(TRequest).Name;

    private readonly ILogger<LoggingMiddleware<TRequest>> _logger;
    private readonly LoggingOptions _options;

    /// <summary>
    /// Initialises a new <see cref="LoggingMiddleware{TRequest}"/>.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">Logging options controlling verbosity.</param>
    public LoggingMiddleware(
        ILogger<LoggingMiddleware<TRequest>> logger,
        LoggingOptions options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public async ValueTask HandleAsync(
        TRequest request,
        RequestHandlerDelegate next,
        CancellationToken cancellationToken)
    {
        if (!(_options.EnableSend && _logger.IsEnabled(LogLevel.Debug)))
        {
            await next().ConfigureAwait(false);
            return;
        }

        _logger.LogDebug("Mediator: dispatching {RequestName}", _requestName);

        try
        {
            await next().ConfigureAwait(false);
            _logger.LogDebug("Mediator: {RequestName} completed successfully", _requestName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mediator: {RequestName} failed with {ExceptionType}", _requestName, ex.GetType().Name);
            throw;
        }
    }
}
