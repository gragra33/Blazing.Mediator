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
        // Register Blazing.Mediator with middleware
        services.AddMediator(config =>
        {
            // Add request middleware (equivalent to MediatR pipeline behaviors)
            config.AddMiddleware(typeof(GenericRequestPreProcessor<,>));
            config.AddMiddleware(typeof(GenericRequestMiddleware<,>));
            config.AddMiddleware(typeof(GenericRequestPostProcessor<,>));

            // Add conditional middleware for specific request types
            config.AddMiddleware(typeof(ConstrainedRequestPostProcessor));

            // Add stream request middleware for streaming functionality
            config.AddMiddleware(typeof(GenericStreamRequestMiddleware<,>));

        }, typeof(Program).Assembly);

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

// Manually subscribe notification handlers (Blazing.Mediator requires explicit subscription)
var pingedHandler = new PingedHandler(writer);
var pingedAlsoHandler = new PingedAlsoHandler(writer);
var pongedConstrainedHandler = new PongedConstrainedHandler(writer);
var covariantHandler = new CovariantNotificationHandler(writer);

mediator.Subscribe(pingedHandler);
mediator.Subscribe(pingedAlsoHandler);
mediator.Subscribe(pongedConstrainedHandler);
mediator.Subscribe(covariantHandler); // Generic subscription for all notifications
mediator.Subscribe<Pinged>(covariantHandler); // Also subscribe to specific notification for testing

const string separator = "==============================================";

Console.WriteLine(separator);
Console.WriteLine("Blazing.Mediator Examples");
Console.WriteLine(separator);
Console.WriteLine();
Console.WriteLine("This project demonstrates the core features of Blazing.Mediator");
Console.WriteLine("converted from the original MediatR examples.");
Console.WriteLine();
Console.WriteLine("Key differences from MediatR:");
Console.WriteLine("- Uses Blazing.Mediator.IMediator instead of MediatR.IMediator");
Console.WriteLine("- Uses Blazing.Mediator.IRequest<T> instead of MediatR.IRequest<T>");
Console.WriteLine("- Uses middleware instead of pipeline behaviors");
Console.WriteLine("- Better performance and lower memory allocation");
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
/// This demonstrates all core features of Blazing.Mediator converted from MediatR examples.
/// </summary>
public static partial class Program
{
    // Partial method implementation is handled by top-level program
}
