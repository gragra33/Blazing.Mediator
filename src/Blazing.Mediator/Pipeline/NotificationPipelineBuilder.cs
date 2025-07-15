namespace Blazing.Mediator.Pipeline;

/// <summary>
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
public class NotificationPipelineBuilder : INotificationPipelineBuilder
{
    private const string OrderPropertyName = "Order";
    private readonly List<NotificationMiddlewareInfo> _middlewareInfos = [];

    private sealed record NotificationMiddlewareInfo(Type Type, int Order);

    /// <summary>
    /// Determines the order for a notification middleware type, using next available order after highest explicit order as fallback.
    /// Orders range from -999 to 999. Middleware without explicit order are assigned incrementally after the highest explicit order.
    /// </summary>
    private int GetMiddlewareOrder(Type middlewareType)
    {
        // Try to get order from a static Order property or field first
        var staticOrder = GetStaticOrder(middlewareType);
        if (staticOrder.HasValue)
        {
            return Math.Clamp(staticOrder.Value, -999, 999);
        }

        // Check for OrderAttribute if it exists (common pattern)
        var attributeOrder = GetOrderFromAttribute(middlewareType);
        if (attributeOrder.HasValue)
        {
            return Math.Clamp(attributeOrder.Value, -999, 999);
        }

        // Try to get order from instance Order property
        var instanceOrder = GetInstanceOrder(middlewareType);
        if (instanceOrder.HasValue)
        {
            return Math.Clamp(instanceOrder.Value, -999, 999);
        }

        // Fallback: assign order after the highest explicitly set order
        return GetNextAvailableOrder();
    }

    private static int? GetStaticOrder(Type middlewareType)
    {
        var orderProperty = middlewareType.GetProperty(OrderPropertyName, BindingFlags.Public | BindingFlags.Static);
        if (orderProperty != null && orderProperty.PropertyType == typeof(int))
        {
            return (int)orderProperty.GetValue(null)!;
        }

        var orderField = middlewareType.GetField(OrderPropertyName, BindingFlags.Public | BindingFlags.Static);
        if (orderField != null && orderField.FieldType == typeof(int))
        {
            return (int)orderField.GetValue(null)!;
        }

        return null;
    }

    private static int? GetOrderFromAttribute(Type middlewareType)
    {
        var orderAttribute = middlewareType.GetCustomAttributes(false)
            .FirstOrDefault(attr => attr.GetType().Name == "OrderAttribute");
        if (orderAttribute != null)
        {
            var orderProp = orderAttribute.GetType().GetProperty(OrderPropertyName);
            if (orderProp != null && orderProp.PropertyType == typeof(int))
            {
                return (int)orderProp.GetValue(orderAttribute)!;
            }
        }

        return null;
    }

    private static int? GetInstanceOrder(Type middlewareType)
    {
        var instanceOrderProperty = middlewareType.GetProperty(OrderPropertyName, BindingFlags.Public | BindingFlags.Instance);
        if (instanceOrderProperty != null && instanceOrderProperty.PropertyType == typeof(int) &&
            (!instanceOrderProperty.GetGetMethod()!.IsVirtual || 
             instanceOrderProperty.DeclaringType != typeof(INotificationMiddleware)))
        {
            try
            {
                // Create a temporary instance to get the Order value
                object? instance = Activator.CreateInstance(middlewareType);
                if (instance != null)
                {
                    int orderValue = (int)instanceOrderProperty.GetValue(instance)!;
                    // Only use non-default values as explicit orders
                    if (orderValue != 0) 
                    {
                        return orderValue;
                    }
                }
            }
            catch
            {
                // If we can't create an instance, fall through to fallback logic
            }
        }

        return null;
    }

    private int GetNextAvailableOrder()
    {
        // If no middleware registered yet, start from 1
        if (_middlewareInfos.Count == 0)
        {
            return 1;
        }

        // Find the highest explicit order and add 1
        var highestExplicitOrder = _middlewareInfos.Max(m => m.Order);
        
        // Ensure we don't exceed the maximum allowed order
        var nextOrder = highestExplicitOrder + 1;
        return Math.Min(nextOrder, 999);
    }

    /// <inheritdoc />
    public INotificationPipelineBuilder AddMiddleware<TMiddleware>()
        where TMiddleware : class, INotificationMiddleware
    {
        var middlewareType = typeof(TMiddleware);
        var order = GetMiddlewareOrder(middlewareType);
        _middlewareInfos.Add(new NotificationMiddlewareInfo(middlewareType, order));
        return this;
    }

    /// <inheritdoc />
    public INotificationPipelineBuilder AddMiddleware(Type middlewareType)
    {
        if (!typeof(INotificationMiddleware).IsAssignableFrom(middlewareType))
        {
            throw new ArgumentException($"Type {middlewareType.Name} does not implement INotificationMiddleware", nameof(middlewareType));
        }

        var order = GetMiddlewareOrder(middlewareType);
        _middlewareInfos.Add(new NotificationMiddlewareInfo(middlewareType, order));
        return this;
    }

    /// <inheritdoc />
    public NotificationDelegate<TNotification> Build<TNotification>(
        IServiceProvider serviceProvider, 
        NotificationDelegate<TNotification> finalHandler)
        where TNotification : INotification
    {
        // Sort middleware by order (ascending - lower numbers execute first)
        var sortedMiddleware = _middlewareInfos
            .OrderBy(m => m.Order)
            .ToList();

        // Build pipeline from right to left (last middleware first)
        NotificationDelegate<TNotification> pipeline = finalHandler;

        for (int i = sortedMiddleware.Count - 1; i >= 0; i--)
        {
            var middlewareInfo = sortedMiddleware[i];
            var currentPipeline = pipeline;

            pipeline = async (notification, cancellationToken) =>
            {
                var middleware = (INotificationMiddleware)serviceProvider.GetRequiredService(middlewareInfo.Type);
                
                // Check if it's conditional middleware
                if (middleware is IConditionalNotificationMiddleware conditionalMiddleware &&
                    !conditionalMiddleware.ShouldExecute(notification))
                {
                    await currentPipeline(notification, cancellationToken);
                    return;
                }

                await middleware.InvokeAsync(notification, currentPipeline, cancellationToken);
            };
        }

        return pipeline;
    }

    /// <inheritdoc />
    public async Task ExecutePipeline<TNotification>(
        TNotification notification,
        IServiceProvider serviceProvider,
        NotificationDelegate<TNotification> finalHandler,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var pipeline = Build(serviceProvider, finalHandler);
        await pipeline(notification, cancellationToken);
    }
}
