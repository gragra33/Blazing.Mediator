namespace Blazing.Mediator.Examples;

/// <summary>
/// Another example notification to demonstrate error handling.
/// This demonstrates notifications that might throw exceptions.
/// Compare with MediatR version: uses Blazing.Mediator.INotification instead of MediatR.INotification.
/// </summary>
public class Ponged : INotification
{
    /// <summary>
    /// Gets or sets the message associated with the pong.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
