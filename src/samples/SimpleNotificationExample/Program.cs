var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // Register Blazing.Mediator with automatic notification middleware discovery
        // This will automatically find and register:
        //   - NotificationLoggingMiddleware
        //   - NotificationValidationMiddleware  
        //   - NotificationMetricsMiddleware
        //   - NotificationAuditMiddleware
        services.AddMediator(
            configureMiddleware: null,
            discoverMiddleware: false,                    // Don't discover request middleware
            discoverNotificationMiddleware: true,         // DO discover notification middleware
            Assembly.GetExecutingAssembly()
        );

        // Register notification subscribers as scoped services
        // These are simple classes that handle notifications when subscribed
        services.AddScoped<EmailNotificationHandler>();
        services.AddScoped<InventoryNotificationHandler>();

        // Register the demo runner
        services.AddScoped<Runner>();

        // Configure logging for better output
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });
    })
    .Build();

const string separator = "==============================================";

Console.WriteLine(separator);
Console.WriteLine("* Blazing.Mediator - Simple Notification Example");
Console.WriteLine(separator);
Console.WriteLine();
Console.WriteLine("This example demonstrates the notification system with:");
Console.WriteLine("  - OrderCreatedNotification - published when orders are created");
Console.WriteLine("  - EmailNotificationHandler - simple subscriber class");
Console.WriteLine("  - InventoryNotificationHandler - simple subscriber class");
Console.WriteLine("  - Auto-discovered notification middleware:");
Console.WriteLine("    * NotificationLoggingMiddleware (order: 100)");
Console.WriteLine("    * NotificationValidationMiddleware (order: 200)");
Console.WriteLine("    * NotificationMetricsMiddleware (order: 300)");
Console.WriteLine("    * NotificationAuditMiddleware (order: 400)");
Console.WriteLine();
Console.WriteLine("Key concepts demonstrated:");
Console.WriteLine("  - Multiple subscribers to the same notification");
Console.WriteLine("  - Simple scoped services (no background services)");
Console.WriteLine("  - Auto-discovery of notification middleware (discoverNotificationMiddleware: true)");
Console.WriteLine("  - Notification pipeline inspection for debugging");
Console.WriteLine("  - Manual subscription at runtime");
Console.WriteLine("  - Error handling in subscribers");
Console.WriteLine("  - Uses recommended scoped mediator lifetime");
Console.WriteLine();
Console.WriteLine(separator);
Console.WriteLine();

// Create a scope to resolve services and demonstrate the notification system
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

// Get the mediator and manually subscribe both handlers
var mediator = services.GetRequiredService<IMediator>();
var emailHandler = services.GetRequiredService<EmailNotificationHandler>();
var inventoryHandler = services.GetRequiredService<InventoryNotificationHandler>();

// Subscribe both handlers to OrderCreatedNotification
mediator.Subscribe(emailHandler);
mediator.Subscribe(inventoryHandler);

Console.WriteLine("* Registered notification middleware automatically discovered");
Console.WriteLine("* EmailNotificationHandler subscribed to OrderCreatedNotification");
Console.WriteLine("* InventoryNotificationHandler subscribed to OrderCreatedNotification");
Console.WriteLine();

// Run the demo
var runner = services.GetRequiredService<Runner>();
await runner.RunAsync();

Console.WriteLine();
Console.WriteLine("@ Demo finished. Press any key to exit...");
Console.ReadKey();
