using Blazing.Mediator;
using Blazing.Mediator.Configuration;
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
        var mediatorConfig = new MediatorConfiguration();
        mediatorConfig.WithMiddlewareDiscovery();
        //mediatorConfig.WithConstrainedMiddlewareDiscovery();
        //mediatorConfig.WithNotificationMiddlewareDiscovery();

        // Add request middleware (equivalent to MediatR pipeline behaviors)
        //mediatorConfig.AddMiddleware(typeof(GenericRequestPreProcessor<,>));
        //mediatorConfig.AddMiddleware(typeof(GenericRequestMiddleware<,>));
        //mediatorConfig.AddMiddleware(typeof(GenericRequestPostProcessor<,>));

        // Add conditional middleware for specific request types
        //mediatorConfig.AddMiddleware(typeof(ConstrainedRequestPostProcessor));

        // Add stream request middleware for streaming functionality
        //mediatorConfig.AddMiddleware(typeof(GenericStreamRequestMiddleware<,>));

        // Enable proper notification handler discovery
        mediatorConfig.WithNotificationHandlerDiscovery();

        // Disable telemetry to improve performance
        mediatorConfig.WithoutTelemetry();
        mediatorConfig.WithoutStatistics();
        mediatorConfig.WithoutLogging();

        services.AddMediator(mediatorConfig);

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
