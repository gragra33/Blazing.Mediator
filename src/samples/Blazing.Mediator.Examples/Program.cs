using Blazing.Mediator;
using Blazing.Mediator.Examples;
using Blazing.Mediator.Examples.Streams;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Get the writer for capturing output
var writer = new WrappingWriter(Console.Out);

// Build the host with dependency injection
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // Register Blazing.Mediator with middleware and proper handler discovery
        services.AddMediator(config =>
        {
            config.WithMiddlewareDiscovery();
            //config.WithConstrainedMiddlewareDiscovery();
            //config.WithNotificationMiddlewareDiscovery();

            // Add request middleware (equivalent to MediatR pipeline behaviors)
            //config.AddMiddleware(typeof(GenericRequestPreProcessor<,>));
            //config.AddMiddleware(typeof(GenericRequestMiddleware<,>));
            //config.AddMiddleware(typeof(GenericRequestPostProcessor<,>));

            // Add conditional middleware for specific request types
            //config.AddMiddleware(typeof(ConstrainedRequestPostProcessor));

            // Add stream request middleware for streaming functionality
            //config.AddMiddleware(typeof(GenericStreamRequestMiddleware<,>));

            // Enable proper notification handler discovery
            config.WithNotificationHandlerDiscovery();

            // Disable telemetry to improve performance
            config.WithoutTelemetry();
            config.WithoutStatistics();
            config.WithoutLogging();

            config.AddFromAssembly(typeof(Program).Assembly);
        });
    //}, typeof(Program).Assembly);

        // Register the WrappingWriter for dependency injection
        services.AddSingleton<TextWriter>(writer);
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Warning);
    })
    .Build();

// Get the mediator and run examples
var mediator = host.Services.GetRequiredService<IMediator>();


const string separator = "==============================================";

Console.WriteLine(separator);
Console.WriteLine("Blazing.Mediator Examples - Handler-Based System");
Console.WriteLine(separator);
Console.WriteLine();
Console.WriteLine("This project demonstrates the core features of Blazing.Mediator");
Console.WriteLine("converted from subscription-based to handler-based notifications.");
Console.WriteLine();
Console.WriteLine();
Console.WriteLine();

try
{
    await Runner.Run(mediator, writer, "Blazing.Mediator Examples", testStreams: true);

    Console.WriteLine();
    Console.WriteLine(separator);
    Console.WriteLine("Examples completed successfully!");
    Console.WriteLine(separator);
}
catch (Exception ex)
{
    Console.WriteLine($"Error running examples: {ex}");
}

/// <summary>
/// Entry point for the Blazing.Mediator examples application.
/// </summary>
public static partial class Program
{
    // Partial method implementation is handled by top-level program
}
