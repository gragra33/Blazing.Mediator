var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // Register the request handlers and middleware with statistics tracking
        // Use auto-discovery to demonstrate type constraint support
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithMiddlewareDiscovery(); // Enable auto-discovery with type constraint support
        }, Assembly.GetExecutingAssembly());

        // Register FluentValidation services
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Configure logging with custom formatter
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole(options =>
            {
                options.FormatterName = "SimpleClassName";
            });
            logging.AddConsoleFormatter<SimpleConsoleFormatter, SimpleConsoleFormatterOptions>();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        // Configure the host to use the console output
        services.AddSingleton(Console.Out);

        services.AddScoped<Runner>();
    })
    .Build();

const string separator = "==============================================";

Console.WriteLine(separator);
Console.WriteLine("Blazing.Mediator TypedMiddlewareExample");
Console.WriteLine(separator);
Console.WriteLine();

Console.WriteLine("This project demonstrates the distinction between ICommand and IQuery");
Console.WriteLine("interfaces in Blazing.Mediator with type-constrained middleware.\n");
Console.WriteLine("Key Features:");
Console.WriteLine("• Commands use ICommand and ICommand<T> interfaces");
Console.WriteLine("• Queries use IQuery<T> interface");
Console.WriteLine("• Validation middleware is constrained to ICommand only");
Console.WriteLine("• Queries bypass validation middleware");
Console.WriteLine("• Clear logging shows the distinction between command and query processing\n");
Console.WriteLine("The example showcases how type constraints can be used to apply");
Console.WriteLine("middleware selectively based on the request type.\n");

Console.WriteLine(separator);
Console.WriteLine();

// Create a scope to resolve services
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

var runner = services.GetRequiredService<Runner>();

// Inspect middleware pipeline before running the demo
runner.InspectMiddlewarePipeline();

await runner.Run();
