var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // Register Blazing.Mediator with automatic notification handler discovery
        // and type constraint support
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithNotificationMiddlewareDiscovery(); // Enable auto-discovery with type constraints
        }, Assembly.GetExecutingAssembly());

        // Note: Notification handlers are automatically discovered and registered!
        // No manual registration needed for:
        // - EmailNotificationHandler
        // - InventoryNotificationHandler  
        // - BusinessOperationsHandler
        // - AuditNotificationHandler

        // Register the demo runner
        services.AddScoped<Runner>();

        // Register the pipeline displayer
        services.AddScoped<NotificationPipelineDisplayer>();

        // Configure logging for better output
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });
    })
    .Build();

// Create a scope to resolve services and demonstrate the notification system
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

// Display pipeline information using displayer
var displayer = services.GetRequiredService<NotificationPipelineDisplayer>();
displayer.DisplayPipelineInfo();

// Run the demo
var runner = services.GetRequiredService<Runner>();
await runner.RunAsync();

Console.WriteLine();
Console.WriteLine("Demo finished. Press any key to exit...");
Console.ReadKey();Console.ReadKey();