namespace Blazing.Mediator.Examples;

/// <summary>
/// Example notification that can have multiple handlers.
/// This demonstrates the notification pattern using Blazing.Mediator.
/// Compare with MediatR version: uses Blazing.Mediator.INotification instead of MediatR.INotification.
/// </summary>
public class Pinged : INotification
{
    /// <summary>
    /// Gets or sets the timestamp when the ping occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
