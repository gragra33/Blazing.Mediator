var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // Register the request handlers and middleware
        services.AddMediator(
            enableStatisticsTracking: true,
            discoverMiddleware: true,
            assemblies: Assembly.GetExecutingAssembly());

        // Register FluentValidation services
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Configure logging
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole(options =>
            {
                options.FormatterName = "SimpleClassName";
            });
            logging.AddConsoleFormatter<SimpleConsoleFormatter, SimpleConsoleFormatterOptions>();
            logging.SetMinimumLevel(LogLevel.Debug);

            //// Filter to suppress the validation error logging from ErrorHandlingMiddleware
            //// This suppresses the technical stack trace while keeping the user-friendly error message
            //logging.AddFilter((provider, category, logLevel) =>
            //{
            //    // Suppress Error level logs that contain "Validation failed for" from ErrorHandlingMiddleware
            //    if (logLevel == LogLevel.Error &&
            //        category?.Contains("ErrorHandlingMiddleware") == true)
            //    {
            //        return false; // Don't log this message
            //    }
            //    return true; // Log all other messages
            //});
        });

        // Configure the host to use the console output
        services.AddSingleton(Console.Out);

        services.AddScoped<Runner>();
    })
    .Build();

const string separator = "==============================================";

Console.WriteLine(separator);
Console.WriteLine("Blazing.Mediator Simple Example");
Console.WriteLine(separator);
Console.WriteLine();
Console.WriteLine("This project demonstrates the core features of Blazing.Mediator.\n");
Console.WriteLine("It includes a simple e-commerce scenario with product lookup,");
Console.WriteLine("inventory management, order confirmation, and customer registration.\n");
Console.WriteLine("The example showcases how to use Blazing.Mediator with middleware");
Console.WriteLine("for error handling, validation, and logging.\n");
Console.WriteLine("The code is structured to demonstrate best practices for building");
Console.WriteLine("scalable and maintainable applications using Blazing.Mediator.\n");

Console.WriteLine(separator);
Console.WriteLine();

// Create a scope to resolve services
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

var runner = services.GetRequiredService<Runner>();

// Inspect middleware pipeline before running the demo
runner.InspectMiddlewarePipeline();

await runner.Run();
