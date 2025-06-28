namespace UserManagement.Api.Application.Exceptions;

/// <summary>
/// Exception thrown when a business rule violation occurs.
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BusinessException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessException"/> class with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public BusinessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}