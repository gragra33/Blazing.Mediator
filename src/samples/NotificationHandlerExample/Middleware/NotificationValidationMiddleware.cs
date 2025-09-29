namespace NotificationHandlerExample.Middleware;

/// <summary>
/// Notification validation middleware that validates notification data before processing.
/// Demonstrates validation logic and error handling in the notification pipeline.
/// </summary>
public class NotificationValidationMiddleware(ILogger<NotificationValidationMiddleware> logger) 
    : INotificationMiddleware
{
    public int Order => 200; // Execute after logging but before business logic

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, 
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        logger.LogInformation("[VALIDATE] Validating notification: {NotificationType}", typeof(TNotification).Name);

        // Perform type-specific validation
        var validationResult = ValidateNotification(notification);
        
        if (!validationResult.IsValid)
        {
            logger.LogError("[-] Notification validation failed: {Errors}", 
                string.Join(", ", validationResult.Errors));
            throw new InvalidOperationException($"Notification validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        logger.LogInformation("[+] Notification validation passed");

        // Continue to next middleware/handlers
        await next(notification, cancellationToken);
    }

    private ValidationResult ValidateNotification<TNotification>(TNotification notification) where TNotification : INotification
    {
        var result = new ValidationResult();

        // Generic validation for all notifications
        if (notification == null)
        {
            result.AddError("Notification cannot be null");
            return result;
        }

        // Specific validation for OrderCreatedNotification
        if (notification is OrderCreatedNotification orderNotification)
        {
            ValidateOrderCreatedNotification(orderNotification, result);
        }

        return result;
    }

    private void ValidateOrderCreatedNotification(OrderCreatedNotification notification, ValidationResult result)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(notification.OrderId))
        {
            result.AddError("OrderId cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(notification.CustomerName))
        {
            result.AddError("CustomerName cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(notification.CustomerEmail))
        {
            result.AddError("CustomerEmail cannot be null or empty");
        }
        else if (!IsValidEmail(notification.CustomerEmail))
        {
            result.AddError("CustomerEmail format is invalid");
        }

        if (notification.TotalAmount <= 0)
        {
            result.AddError("TotalAmount must be greater than zero");
        }

        if (!notification.Items.Any())
        {
            result.AddError("Order must contain at least one item");
        }
        else
        {
            // Validate each item
            for (int i = 0; i < notification.Items.Count; i++)
            {
                var item = notification.Items[i];
                
                if (string.IsNullOrWhiteSpace(item.ProductName))
                {
                    result.AddError($"Item {i + 1}: ProductName cannot be null or empty");
                }

                if (item.Quantity <= 0)
                {
                    result.AddError($"Item {i + 1}: Quantity must be greater than zero");
                }

                if (item.UnitPrice <= 0)
                {
                    result.AddError($"Item {i + 1}: UnitPrice must be greater than zero");
                }
            }

            // Validate total amount matches sum of items
            var calculatedTotal = notification.Items.Sum(i => i.TotalPrice);
            if (Math.Abs(notification.TotalAmount - calculatedTotal) > 0.01m) // Allow for small rounding differences
            {
                result.AddError($"TotalAmount ({notification.TotalAmount:F2}) doesn't match sum of items ({calculatedTotal:F2})");
            }
        }

        // Business rule validations
        if (notification.CreatedAt > DateTime.UtcNow)
        {
            result.AddError("CreatedAt cannot be in the future");
        }

        if (notification.CreatedAt < DateTime.UtcNow.AddYears(-1))
        {
            result.AddError("CreatedAt cannot be more than 1 year in the past");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private class ValidationResult
    {
        private readonly List<string> _errors = new();

        public bool IsValid => _errors.Count == 0;
        public IReadOnlyList<string> Errors => _errors.AsReadOnly();

        public void AddError(string error)
        {
            _errors.Add(error);
        }
    }
}