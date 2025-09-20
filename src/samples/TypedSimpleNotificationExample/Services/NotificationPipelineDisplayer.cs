namespace TypedSimpleNotificationExample.Services;

public class NotificationPipelineDisplayer(
    INotificationMiddlewarePipelineInspector pipelineInspector,
    IServiceProvider serviceProvider)
{
    public void DisplayPipelineInfo()
    {
        const string separator = "==============================================";

        Console.WriteLine(separator);
        Console.WriteLine("Blazing.Mediator - Typed Simple Notification Example");
        Console.WriteLine(separator);
        Console.WriteLine();
        Console.WriteLine("This example demonstrates TYPE-CONSTRAINED notification middleware:");
        Console.WriteLine();

        DisplayNotificationTypes();
        DisplayMiddleware();
        DisplayNotificationSubscribers();
        DisplayKeyFeatures();

        Console.WriteLine(separator);
        Console.WriteLine();
    }

    private static void DisplayNotificationTypes()
    {
        Console.WriteLine("NOTIFICATION TYPES:");

        var assembly = Assembly.GetExecutingAssembly();
        var notificationInterfaces = assembly.GetTypes()
            .Where(t => t.IsInterface &&
                       !t.IsAssignableFrom(typeof(INotification)) &&
                       typeof(INotification).IsAssignableFrom(t) &&
                       t != typeof(INotification))
            .OrderBy(t => t.Name)
            .ToList();

        if (notificationInterfaces.Count == 0)
        {
            Console.WriteLine("  (No marker interfaces found - using concrete notification types)");

            // Fall back to concrete notification types
            var concreteNotifications = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(INotification).IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .ToList();

            foreach (var notification in concreteNotifications)
            {
                var interfaces = notification.GetInterfaces()
                    .Where(i => i != typeof(INotification) && typeof(INotification).IsAssignableFrom(i))
                    .OrderBy(i => i.Name)
                    .ToList();

                if (interfaces.Any())
                {
                    var interfaceNames = string.Join(", ", interfaces.Select(i => i.Name));
                    Console.WriteLine($"  - {notification.Name} : {interfaceNames}");
                }
                else
                {
                    Console.WriteLine($"  - {notification.Name}");
                }
            }
        }
        else
        {
            foreach (var notificationInterface in notificationInterfaces)
            {
                var implementations = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && notificationInterface.IsAssignableFrom(t))
                    .OrderBy(t => t.Name)
                    .ToList();

                Console.WriteLine($"  - {notificationInterface.Name}");

                foreach (var implementation in implementations)
                {
                    Console.WriteLine($"    - {implementation.Name}");
                }

                if (implementations.Count == 0)
                {
                    Console.WriteLine("    (No implementations found)");
                }
                Console.WriteLine();
            }
        }
        Console.WriteLine();
    }

    private void DisplayMiddleware()
    {
        Console.WriteLine("TYPE-CONSTRAINED MIDDLEWARE:");

        var middlewareAnalysis = pipelineInspector.AnalyzeMiddleware(serviceProvider);

        if (middlewareAnalysis.Count == 0)
        {
            Console.WriteLine("  (No middleware registered)");
        }
        else
        {
            foreach (var middleware in middlewareAnalysis)
            {
                Console.WriteLine($"  - [{middleware.OrderDisplay}] {middleware.ClassName}{middleware.TypeParameters}");
                if (!string.IsNullOrEmpty(middleware.GenericConstraints))
                {
                    Console.WriteLine($"        - Constraints: {middleware.GenericConstraints}");
                }
            }
        }
        Console.WriteLine();
    }

    private static void DisplayNotificationSubscribers()
    {
        Console.WriteLine("NOTIFICATION SUBSCRIBERS:");

        var assembly = Assembly.GetExecutingAssembly();
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass &&
                       !t.IsAbstract &&
                       t.GetInterfaces().Any(i => i.IsGenericType &&
                                                  i.GetGenericTypeDefinition() == typeof(INotificationSubscriber<>)))
            .OrderBy(t => t.Name)
            .ToList();

        if (handlerTypes.Count == 0)
        {
            Console.WriteLine("  (No notification subscribers found)");
        }
        else
        {
            foreach (var handlerType in handlerTypes)
            {
                var handledTypes = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationSubscriber<>))
                    .Select(i => i.GetGenericArguments()[0])
                    .OrderBy(t => t.Name)
                    .ToList();

                var handledTypeNames = string.Join(", ", handledTypes.Select(t => t.Name));
                Console.WriteLine($"  - {handlerType.Name} : {handledTypeNames}");
            }
        }
        Console.WriteLine();
    }

    private void DisplayKeyFeatures()
    {
        Console.WriteLine("KEY FEATURES DEMONSTRATED:");

        // Dynamically determine features based on what's actually registered
        var features = new List<string>();

        var middlewareAnalysis = pipelineInspector.AnalyzeMiddleware(serviceProvider);
        if (middlewareAnalysis.Any(m => !string.IsNullOrEmpty(m.GenericConstraints)))
        {
            features.Add("Type constraints for selective middleware execution");
        }

        var assembly = Assembly.GetExecutingAssembly();
        var notificationInterfaces = assembly.GetTypes()
            .Where(t => t.IsInterface &&
                       typeof(INotification).IsAssignableFrom(t) &&
                       t != typeof(INotification))
            .ToList();

        if (notificationInterfaces.Count > 0)
        {
            features.Add("Interface-based notification categorization");
        }

        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass &&
                       !t.IsAbstract &&
                       t.GetInterfaces().Any(i => i.IsGenericType &&
                                                  i.GetGenericTypeDefinition() == typeof(INotificationSubscriber<>)))
            .ToList();

        if (handlerTypes.Count > 0)
        {
            features.Add("Multiple subscribers per notification type");
        }

        if (middlewareAnalysis.Count > 0)
        {
            features.Add("Notification pipeline inspection and analysis");
        }

        // Always add these as they're core features of the example
        features.Add("Performance metrics tracking");
        features.Add("Complex workflow with multiple notification types");

        foreach (var feature in features)
        {
            Console.WriteLine($"  - {feature}");
        }
        Console.WriteLine();
    }
}