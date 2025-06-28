using FluentValidation.Results;

namespace ECommerce.Api.Application.Exceptions;

/// <summary>
/// Exception thrown when validation failures occur during command processing.
/// </summary>
/// <param name="failures">The collection of validation failures.</param>
public class ValidationException(IEnumerable<ValidationFailure> failures)
    : Exception("One or more validation failures have occurred.")
{
    /// <summary>
    /// Gets the collection of validation errors.
    /// </summary>
    public IEnumerable<ValidationFailure> Errors { get; } = failures;
}
