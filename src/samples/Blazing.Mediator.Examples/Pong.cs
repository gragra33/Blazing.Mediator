namespace Blazing.Mediator.Examples;

/// <summary>
/// Response class for the Ping request.
/// This demonstrates a simple response object used with Blazing.Mediator.
/// This class is identical to the MediatR version - no changes needed for DTOs.
/// </summary>
public class Pong
{
    /// <summary>
    /// Gets or sets the response message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
