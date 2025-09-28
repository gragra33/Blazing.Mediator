namespace Blazing.Mediator;

/// <summary>
/// Represents the status of handlers for a query or command.
/// </summary>
public enum HandlerStatus
{
    /// <summary>
    /// No handler is registered for this request type.
    /// </summary>
    Missing,

    /// <summary>
    /// Exactly one handler is registered (ideal state).
    /// </summary>
    Single,

    /// <summary>
    /// Multiple handlers are registered (potential issue).
    /// </summary>
    Multiple
}