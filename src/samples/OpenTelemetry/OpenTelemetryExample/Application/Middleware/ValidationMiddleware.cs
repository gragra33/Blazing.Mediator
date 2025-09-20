using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using FluentValidation;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Middleware;

/// <summary>
/// Middleware for request validation using FluentValidation for void commands.
/// </summary>
public sealed class ValidationMiddleware<TRequest>(
    IServiceProvider serviceProvider,
    ILogger<ValidationMiddleware<TRequest>> logger)
    : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    private readonly ILogger _logger = logger;

    public int Order => -1000; // Execute early

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        var validator = serviceProvider.GetService<IValidator<TRequest>>();
        if (validator != null)
        {
            _logger.LogDebug("Validating request {RequestType}", typeof(TRequest).Name);

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validation failed for {RequestType}: {Errors}", typeof(TRequest).Name, errors);

                // Add validation details to current activity
                Activity.Current?.SetTag("validation.failed", true);
                Activity.Current?.SetTag("validation.errors", errors);

                throw new ValidationException(validationResult.Errors);
            }

            Activity.Current?.SetTag("validation.passed", true);
        }

        await next();
    }
}

/// <summary>
/// Middleware for request validation using FluentValidation.
/// </summary>
public sealed class ValidationMiddleware<TRequest, TResponse>(
    IServiceProvider serviceProvider,
    ILogger<ValidationMiddleware<TRequest, TResponse>> logger)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger _logger = logger;

    public int Order => -1000; // Execute early

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validator = serviceProvider.GetService<IValidator<TRequest>>();
        if (validator != null)
        {
            _logger.LogDebug("Validating request {RequestType}", typeof(TRequest).Name);

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validation failed for {RequestType}: {Errors}", typeof(TRequest).Name, errors);

                // Add validation details to current activity
                Activity.Current?.SetTag("validation.failed", true);
                Activity.Current?.SetTag("validation.errors", errors);

                throw new ValidationException(validationResult.Errors);
            }

            Activity.Current?.SetTag("validation.passed", true);
        }

        return await next();
    }
}