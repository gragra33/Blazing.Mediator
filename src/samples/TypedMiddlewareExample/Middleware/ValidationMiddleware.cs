using TypedMiddlewareExample.Contracts;

namespace TypedMiddlewareExample.Middleware;

/// <summary>
/// Base class for validation middleware implementations.
/// Provides common validation logic following DRY and SOLID principles.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
public abstract class ValidationMiddlewareBase<TRequest>
{
    private static readonly ActivitySource ActivitySource = new("TypedMiddlewareExample.ValidationMiddleware", "2.0.0");

    protected readonly IServiceProvider ServiceProvider;
    protected readonly ILogger Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationMiddlewareBase{TRequest}"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving validators.</param>
    /// <param name="logger">The logger instance.</param>
    protected ValidationMiddlewareBase(
        IServiceProvider serviceProvider,
        ILogger logger)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the order of this middleware in the pipeline.
    /// </summary>
    public int Order => 200; // Execute after error handling middleware

    /// <summary>
    /// Validates the request using FluentValidation if a validator is available.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous validation operation.</returns>
    /// <exception cref="FluentValidation.ValidationException">Thrown when validation fails.</exception>
    protected async Task ValidateRequestAsync(TRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestName = typeof(TRequest).Name;
        using var activity = ActivitySource.StartActivity($"Validate_{requestName}");

        activity?.SetTag("validation.request_type", requestName);

        Logger.LogDebug("?? [ValidationMiddleware] Checking for validator for CUSTOMER request type {RequestType}", requestName);

        // Try to get a validator for this request type
        var validatorType = typeof(IValidator<>).MakeGenericType(typeof(TRequest));

        if (ServiceProvider.GetService(validatorType) is IValidator<TRequest> validator)
        {
            Logger.LogDebug("? [ValidationMiddleware] Validator found for CUSTOMER {RequestType}, performing validation", requestName);

            var stopwatch = Stopwatch.StartNew();
            var context = new ValidationContext<TRequest>(request);
            var validationResult = await validator.ValidateAsync(context, cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();

            activity?.SetTag("validation.has_validator", "true");
            activity?.SetTag("validation.duration_ms", stopwatch.ElapsedMilliseconds);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                var errorMessage = string.Join(", ", errors);

                activity?.SetTag("validation.success", "false");
                activity?.SetTag("validation.error_count", errors.Count);
                activity?.SetStatus(ActivityStatusCode.Error, errorMessage);

                Logger.LogWarning("? [ValidationMiddleware] Validation failed for CUSTOMER {RequestType} in {Duration}ms: {Errors}",
                    requestName, stopwatch.ElapsedMilliseconds, errorMessage);

                throw new FluentValidation.ValidationException(validationResult.Errors);
            }

            activity?.SetTag("validation.success", "true");
            Logger.LogDebug("? [ValidationMiddleware] Validation passed for CUSTOMER {RequestName} in {Duration}ms",
                requestName, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            activity?.SetTag("validation.has_validator", "false");
            Logger.LogDebug("?? [ValidationMiddleware] No validator found for CUSTOMER {RequestName}", requestName);
        }
    }

    /// <summary>
    /// Gets the request name for logging purposes.
    /// </summary>
    /// <returns>The name of the request type.</returns>
    protected static string GetRequestName() => typeof(TRequest).Name;
}

/// <summary>
/// Validation middleware that only applies to customer-related requests with responses.
/// Demonstrates how type constraints can be used to selectively apply middleware.
/// </summary>
/// <typeparam name="TRequest">The request type, constrained to customer requests</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ValidationMiddleware<TRequest, TResponse> : ValidationMiddlewareBase<TRequest>, IRequestMiddleware<TRequest, TResponse>
    where TRequest : class, ICustomerRequest<TResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationMiddleware{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving validators.</param>
    /// <param name="logger">The logger instance.</param>
    public ValidationMiddleware(
        IServiceProvider serviceProvider,
        ILogger<ValidationMiddleware<TRequest, TResponse>> logger)
        : base(serviceProvider, logger)
    {
    }

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        Logger.LogInformation("[CUSTOMER] Processing Customer Request: {RequestType} - Validation middleware active", 
            typeof(TRequest).Name);

        // Perform FluentValidation using base class logic
        await ValidateRequestAsync(request, cancellationToken).ConfigureAwait(false);

        // Continue with next handler
        return await next().ConfigureAwait(false);
    }
}

/// <summary>
/// Validation middleware that only applies to customer requests without response.
/// Demonstrates type constraints for void commands.
/// </summary>
/// <typeparam name="TRequest">The request type, constrained to customer requests</typeparam>
public class ValidationMiddleware<TRequest> : ValidationMiddlewareBase<TRequest>, IRequestMiddleware<TRequest>
    where TRequest : class, ICustomerRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationMiddleware{TRequest}"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving validators.</param>
    /// <param name="logger">The logger instance.</param>
    public ValidationMiddleware(
        IServiceProvider serviceProvider,
        ILogger<ValidationMiddleware<TRequest>> logger)
        : base(serviceProvider, logger)
    {
    }

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        Logger.LogInformation("[CUSTOMER] Processing Customer Request: {RequestType} - Validation middleware active", 
            typeof(TRequest).Name);

        // Perform FluentValidation using base class logic
        await ValidateRequestAsync(request, cancellationToken).ConfigureAwait(false);

        // Continue with next handler
        await next().ConfigureAwait(false);
    }
}