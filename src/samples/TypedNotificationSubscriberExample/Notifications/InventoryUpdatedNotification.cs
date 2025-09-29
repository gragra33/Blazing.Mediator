namespace TypedNotificationSubscriberExample.Notifications;

/// <summary>
/// Notification published when inventory levels change.
/// Implements IInventoryNotification for type-constrained middleware.
/// </summary>
public class InventoryUpdatedNotification(
    string productId,
    string productName,
    int oldQuantity,
    int newQuantity,
    int changeAmount,
    DateTime updatedAt)
    : IInventoryNotification
{
    public string ProductId { get; } = productId;
    public string ProductName { get; } = productName;
    public int OldQuantity { get; } = oldQuantity;
    public int NewQuantity { get; } = newQuantity;
    public int Quantity { get; } = changeAmount; // For IInventoryNotification
    public int ChangeAmount { get; } = changeAmount;
    public DateTime UpdatedAt { get; } = updatedAt;
}