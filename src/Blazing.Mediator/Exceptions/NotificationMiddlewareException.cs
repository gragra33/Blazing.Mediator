using System.Runtime.Serialization;

namespace Blazing.Mediator.Exceptions;

/// <summary>
/// Exception thrown when an error occurs in notification middleware processing.
/// Provides detailed context about the middleware, notification type, and execution context.
/// </summary>
[Serializable]
public class NotificationMiddlewareException : Exception
{
    /// <summary>
    /// Gets the name of the middleware that caused the exception.
    /// </summary>
    public string? MiddlewareName { get; }

    /// <summary>
    /// Gets the type of notification being processed when the exception occurred.
    /// </summary>
    public Type? NotificationType { get; }

    /// <summary>
    /// Gets the actual runtime type of the notification instance.
    /// </summary>
    public Type? ActualNotificationType { get; }

    /// <summary>
    /// Initializes a new instance of the NotificationMiddlewareException class.
    /// </summary>
    public NotificationMiddlewareException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the NotificationMiddlewareException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NotificationMiddlewareException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the NotificationMiddlewareException class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public NotificationMiddlewareException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the NotificationMiddlewareException class with detailed context information.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="middlewareName">The name of the middleware that caused the exception.</param>
    /// <param name="notificationType">The type of notification being processed.</param>
    /// <param name="actualNotificationType">The actual runtime type of the notification instance.</param>
    public NotificationMiddlewareException(string message, Exception innerException, string middlewareName, Type notificationType, Type? actualNotificationType = null)
        : base(message, innerException)
    {
        MiddlewareName = middlewareName;
        NotificationType = notificationType;
        ActualNotificationType = actualNotificationType;
    }

    /// <summary>
    /// Initializes a new instance of the NotificationMiddlewareException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
    protected NotificationMiddlewareException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        MiddlewareName = info.GetString(nameof(MiddlewareName));
        NotificationType = (Type?)info.GetValue(nameof(NotificationType), typeof(Type));
        ActualNotificationType = (Type?)info.GetValue(nameof(ActualNotificationType), typeof(Type));
    }

    /// <summary>
    /// Sets the SerializationInfo with information about the exception.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(MiddlewareName), MiddlewareName);
        info.AddValue(nameof(NotificationType), NotificationType);
        info.AddValue(nameof(ActualNotificationType), ActualNotificationType);
    }

    /// <summary>
    /// Returns a string representation of the exception with detailed context.
    /// </summary>
    /// <returns>A detailed string representation of the exception.</returns>
    public override string ToString()
    {
        var baseString = base.ToString();
        
        if (string.IsNullOrEmpty(MiddlewareName) && NotificationType == null)
            return baseString;

        var contextInfo = new List<string>();
        
        if (!string.IsNullOrEmpty(MiddlewareName))
            contextInfo.Add($"Middleware: {MiddlewareName}");
            
        if (NotificationType != null)
            contextInfo.Add($"Notification Type: {NotificationType.Name}");
            
        if (ActualNotificationType != null && ActualNotificationType != NotificationType)
            contextInfo.Add($"Actual Type: {ActualNotificationType.Name}");

        return $"{baseString}\nContext: {string.Join(", ", contextInfo)}";
    }
}