namespace UserManagement.Api.Application.DTOs;

/// <summary>
/// Represents the result of an operation with success/failure status and optional data.
/// </summary>
public class OperationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the message describing the operation result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional data returned from the operation.
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
}