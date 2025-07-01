namespace ECommerce.Api.Application.DTOs;

/// <summary>
/// Generic result wrapper for operations that may succeed or fail.
/// Provides a standardized way to return success/failure status along with data or error information.
/// </summary>
/// <typeparam name="T">The type of data returned on successful operations.</typeparam>
public class OperationResult<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a message describing the result of the operation.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data returned by the operation (valid when Success is true).
    /// </summary>
    public T Data { get; set; } = default(T)!;

    /// <summary>
    /// Gets or sets the list of errors that occurred during the operation.
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Creates a successful operation result with the provided data.
    /// </summary>
    /// <param name="data">The data to return.</param>
    /// <param name="message">Optional success message.</param>
    /// <returns>A successful OperationResult instance.</returns>
    public static OperationResult<T> SuccessResult(T data, string message = "")
    {
        return new OperationResult<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    /// <summary>
    /// Creates a failed operation result with the provided error information.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errors">Optional list of detailed errors.</param>
    /// <returns>A failed OperationResult instance.</returns>
    public static OperationResult<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new OperationResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? []
        };
    }
}