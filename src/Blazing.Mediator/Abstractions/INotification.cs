namespace Blazing.Mediator;

/// <summary>
/// Marker interface for notifications that can be published to multiple subscribers.
/// Notifications follow the observer pattern where publishers blindly send 
/// notifications and subscribers actively choose to listen.
/// </summary>
public interface INotification
{
}
