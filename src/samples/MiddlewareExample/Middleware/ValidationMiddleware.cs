
namespace MiddlewareExample.Middleware;

/// <summary>
/// Base class for validation middleware that provides shared validation logic.
/// </summary>
/// <typeparam name="TRequest">The type of request to validate.</typeparam>
public abstract class ValidationMiddlewareBase<TRequest>
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ILogger Logger;

    protected ValidationMiddlewareBase(IServiceProvider serviceProvider, ILogger logger)
    {
        ServiceProvider = serviceProvider;
        Logger = logger;
    }

    public int Order => 100; // Execute after error handling but before business logic

    /// <summary>
    /// Validates a request using FluentValidation validators and throws ValidationException if validation fails.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    protected async Task ValidateRequestAsync(TRequest request, CancellationToken cancellationToken)
    {
        Logger.LogDebug(">> Validating request: {RequestType}", typeof(TRequest).Name);

        // Get all validators for this request type (handles multiple validators)
        var validatorType = typeof(IValidator<>).MakeGenericType(typeof(TRequest));
        var validators = ServiceProvider.GetServices(validatorType).Cast<IValidator<TRequest>>().ToList();

        if (validators.Any())
        {
            Logger.LogDebug("-- Found {ValidatorCount} validator(s) for {RequestType}", validators.Count, typeof(TRequest).Name);

            var allErrors = new List<ValidationFailure>();

            // Run all validators
            foreach (var validator in validators)
            {
                var validationResult = await validator.ValidateAsync(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    allErrors.AddRange(validationResult.Errors);
                }
            }

            // If any validator failed, throw exception with all errors
            if (allErrors.Any())
            {
                var errors = string.Join(", ", allErrors.Select(e => e.ErrorMessage));
                Logger.LogWarning("!! Validation failed for {RequestType}: {Errors}", typeof(TRequest).Name, errors);

                throw new ValidationException(errors);
            }

            Logger.LogDebug("-- All validations passed for {RequestType}", typeof(TRequest).Name);
        }
        else
        {
            Logger.LogDebug("-- No validators found for {RequestType}", typeof(TRequest).Name);
        }
    }
}

/// <summary>
/// Validation middleware for commands that don't return a value.
/// Validates requests before they reach the business logic handlers.
/// </summary>
/// <typeparam name="TRequest">The type of request to validate.</typeparam>
public class ValidationMiddleware<TRequest> : ValidationMiddlewareBase<TRequest>, IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    public ValidationMiddleware(IServiceProvider serviceProvider, ILogger<ValidationMiddleware<TRequest>> logger)
        : base(serviceProvider, logger)
    {
    }

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await ValidateRequestAsync(request, cancellationToken);

        // Continue to next middleware or handler
        await next();
    }
}

/// <summary>
/// Validation middleware for queries that return a value.
/// Validates requests before they reach the business logic handlers.
/// </summary>
/// <typeparam name="TRequest">The type of request to validate.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public class ValidationMiddleware<TRequest, TResponse> : ValidationMiddlewareBase<TRequest>, IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public ValidationMiddleware(IServiceProvider serviceProvider, ILogger<ValidationMiddleware<TRequest, TResponse>> logger)
        : base(serviceProvider, logger)
    {
    }

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await ValidateRequestAsync(request, cancellationToken);

        // Continue to next middleware or handler
        return await next();
    }
}
