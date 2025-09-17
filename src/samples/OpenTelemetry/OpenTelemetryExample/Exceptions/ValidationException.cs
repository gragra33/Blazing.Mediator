namespace OpenTelemetryExample.Exceptions;

/// <summary>
/// Custom validation exception.
/// </summary>
public sealed class ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> errors)
    : Exception("Validation failed")
{
    public IEnumerable<FluentValidation.Results.ValidationFailure> Errors { get; } = errors;
}