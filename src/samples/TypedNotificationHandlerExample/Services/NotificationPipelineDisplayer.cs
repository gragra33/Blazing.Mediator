namespace TypedNotificationHandlerExample.Services;

public class NotificationPipelineDisplayer(
    INotificationMiddlewarePipelineInspector pipelineInspector,
    IServiceProvider serviceProvider)
{
    /// <summary>
    /// Displays comprehensive information about the notification pipeline configuration and automatic handler discovery.
    /// </summary>
    public void DisplayPipelineInfo()
    {
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine(" TYPED NOTIFICATION HANDLER PIPELINE ANALYSIS");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();

        DisplayAutomaticHandlers();
        DisplayMiddlewareConfiguration();

        Console.WriteLine();
    }

    /// <summary>
    /// Displays information about automatically discovered notification handlers.
    /// </summary>
    private void DisplayAutomaticHandlers()
    {
        Console.WriteLine("?? AUTOMATIC HANDLER DISCOVERY:");
        Console.WriteLine();

        // Analyze handlers for each notification type
        AnalyzeHandlersFor<OrderCreatedNotification>("Order Created");
        AnalyzeHandlersFor<OrderStatusChangedNotification>("Order Status Changed");  
        AnalyzeHandlersFor<CustomerRegisteredNotification>("Customer Registered");
        AnalyzeHandlersFor<InventoryUpdatedNotification>("Inventory Updated");

        Console.WriteLine();
    }

    /// <summary>
    /// Analyzes and displays handlers for a specific notification type.
    /// </summary>
    private void AnalyzeHandlersFor<T>(string friendlyName) where T : INotification
    {
        var handlers = serviceProvider.GetServices<INotificationHandler<T>>();
        var handlerList = handlers.ToList();

        Console.WriteLine($"?? {friendlyName} Notification:");
        
        if (handlerList.Any())
        {
            foreach (var handler in handlerList)
            {
                var handlerType = handler.GetType();
                Console.WriteLine($"   ? {handlerType.Name} (automatic discovery)");
                
                // Show which other notifications this handler supports
                var allInterfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                    .Select(i => i.GetGenericArguments()[0].Name)
                    .Where(name => name != typeof(T).Name)
                    .ToList();
                    
                if (allInterfaces.Any())
                {
                    Console.WriteLine($"      ?? Also handles: {string.Join(", ", allInterfaces)}");
                }
            }
        }
        else
        {
            Console.WriteLine($"   ? No handlers found");
        }
        
        Console.WriteLine();
    }

    /// <summary>
    /// Displays information about the notification middleware pipeline configuration.
    /// </summary>
    private void DisplayMiddlewareConfiguration()
    {
        Console.WriteLine("?? MIDDLEWARE PIPELINE CONFIGURATION:");
        Console.WriteLine();

        // Display middleware for different notification types to show type constraints
        DisplayMiddlewareFor<OrderCreatedNotification>("Order Created Notification");
        DisplayMiddlewareFor<CustomerRegisteredNotification>("Customer Registered Notification");
        DisplayMiddlewareFor<InventoryUpdatedNotification>("Inventory Updated Notification");

        Console.WriteLine();
    }

    /// <summary>
    /// Displays middleware configuration for a specific notification type.
    /// </summary>
    private void DisplayMiddlewareFor<T>(string friendlyName) where T : INotification
    {
        try
        {
            var middlewareInfo = pipelineInspector.AnalyzeMiddleware(serviceProvider);
            
            Console.WriteLine($"?? {friendlyName}:");
            
            if (middlewareInfo.Any())
            {
                var orderedMiddleware = middlewareInfo.OrderBy(m => m.Order).ToList();
                
                foreach (var middleware in orderedMiddleware)
                {
                    var constraints = GetConstraintDescription(middleware.Type, typeof(T));
                    Console.WriteLine($"   [{middleware.Order:D3}] {middleware.ClassName} {constraints}");
                }
            }
            else
            {
                Console.WriteLine($"   ? No middleware configured");
            }
            
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ? Error analyzing middleware: {ex.Message}");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Gets a description of the type constraints for middleware.
    /// </summary>
    private string GetConstraintDescription(Type middlewareType, Type notificationType)
    {
        if (!middlewareType.IsGenericType)
            return "";

        var genericType = middlewareType.GetGenericTypeDefinition();
        var constraints = genericType.GetGenericArguments()[0].GetGenericParameterConstraints();

        if (constraints.Length == 0)
            return "(all notifications)";

        var constraintNames = constraints
            .Where(c => c != typeof(INotification))
            .Select(c => c.Name)
            .ToList();

        if (constraintNames.Any())
        {
            return $"(type-constrained: {string.Join(", ", constraintNames)})";
        }

        return "(all notifications)";
    }
}