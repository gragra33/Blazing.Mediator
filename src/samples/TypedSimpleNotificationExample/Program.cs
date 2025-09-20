var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // Register Blazing.Mediator with automatic notification middleware discovery
        // and type constraint support
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithNotificationMiddlewareDiscovery(); // Enable auto-discovery with type constraints
        }, Assembly.GetExecutingAssembly());

        // Register notification subscribers as scoped services
        services.AddScoped<EmailNotificationHandler>();
        services.AddScoped<InventoryNotificationHandler>();
        services.AddScoped<BusinessOperationsHandler>();
        services.AddScoped<AuditNotificationHandler>();

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
Console.ReadKey();