using Blazing.Mediator;
using OpenTelemetryExample.Application.Commands;
using OpenTelemetryExample.Exceptions;
using OpenTelemetryExample.Models;

namespace OpenTelemetryExample.Extensions;

/// <summary>
/// Extension methods for mapping minimal API endpoints.
/// Separates endpoint definitions from Program.cs for better organization.
/// </summary>
public static class MinimalEndpointsExtensions
{
    /// <summary>
    /// Maps telemetry-related endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for method chaining.</returns>
    public static WebApplication MapTelemetryEndpoints(this WebApplication app)
    {
        var telemetryGroup = app.MapGroup("/telemetry")
            .WithTags("Telemetry")
            .WithOpenApi();

        telemetryGroup.MapGet("/health", GetTelemetryHealth)
            .WithName("GetTelemetryHealth")
            .WithSummary("Get telemetry system health status")
            .WithDescription("Returns the current health status of the OpenTelemetry and Blazing.Mediator telemetry systems.")
            .Produces<object>()
            .Produces(500);

        telemetryGroup.MapGet("/metrics", GetTelemetryMetrics)
            .WithName("GetTelemetryInfo")
            .WithSummary("Get telemetry configuration information")
            .WithDescription("Returns basic information about the telemetry configuration including meter and activity source names.")
            .Produces<object>();

        return app;
    }

    /// <summary>
    /// Maps debug-related endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for method chaining.</returns>
    public static WebApplication MapDebugEndpoints(this WebApplication app)
    {
        var debugGroup = app.MapGroup("/debug")
            .WithTags("Debug")
            .WithOpenApi();

        debugGroup.MapGet("/mediator", GetMediatorDebugInfo)
            .WithName("GetMediatorDebugInfo")
            .WithSummary("Get debug information about Blazing.Mediator")
            .WithDescription("Returns debug information about the Blazing.Mediator configuration and telemetry status.")
            .Produces<object>();

        return app;
    }

    /// <summary>
    /// Maps testing-related endpoints for telemetry demonstration.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for method chaining.</returns>
    public static WebApplication MapTestingEndpoints(this WebApplication app)
    {
        var testingGroup = app.MapGroup("/testing")
            .WithTags("Testing")
            .WithOpenApi();

        testingGroup.MapPost("/notifications", TestNotifications)
            .WithName("PublishTestNotification")
            .WithSummary("Publish a test notification for telemetry testing")
            .WithDescription("Publishes a test notification through Blazing.Mediator to generate telemetry data.")
            .Produces<object>();

        testingGroup.MapPost("/middleware/error", TestMiddlewareError)
            .WithName("TestMiddlewareError")
            .WithSummary("Test middleware error handling and telemetry")
            .WithDescription("Triggers an error in the middleware pipeline to test error handling and telemetry generation.")
            .Produces<object>()
            .Produces(500);

        testingGroup.MapPost("/middleware/validation", TestMiddlewareValidation)
            .WithName("TestMiddlewareValidation")
            .WithSummary("Test middleware validation and telemetry")
            .WithDescription("Triggers validation errors in the middleware pipeline to test validation handling and telemetry generation.")
            .Produces<object>()
            .Produces<object>(400);

        return app;
    }

    #region Endpoint Handlers

    /// <summary>
    /// Handler for telemetry health endpoint.
    /// </summary>
    private static IResult GetTelemetryHealth()
    {
        try
        {
            var health = Blazing.Mediator.OpenTelemetry.MediatorTelemetryHealthCheck.CheckHealth();
            Console.WriteLine($"[+] Telemetry health check: IsHealthy={health.IsHealthy}, IsEnabled={health.IsEnabled}");
            
            // Return the correct format that matches TelemetryHealthDto
            return Results.Ok(new
            {
                health.IsHealthy,
                health.IsEnabled,
                health.CanRecordMetrics,
                MeterName = health.MeterName ?? "unknown",
                ActivitySourceName = health.ActivitySourceName ?? "unknown",
                health.Message
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[-] Telemetry health check failed: {ex.Message}");
            return Results.Ok(new
            {
                IsHealthy = false,
                IsEnabled = false,
                CanRecordMetrics = false,
                MeterName = "error",
                ActivitySourceName = "error", 
                Message = $"Health check failed: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Handler for telemetry metrics endpoint.
    /// </summary>
    private static IResult GetTelemetryMetrics()
    {
        // Return basic metrics information
        return Results.Ok(new
        {
            MeterName = Mediator.Meter.Name,
            ActivitySourceName = Mediator.ActivitySource.Name,
            Mediator.TelemetryEnabled,
            Message = "Metrics are available via OpenTelemetry exporters configured in the application"
        });
    }

    /// <summary>
    /// Handler for mediator debug endpoint.
    /// </summary>
    private static IResult GetMediatorDebugInfo()
    {
        return Results.Ok(new
        {
            Mediator.TelemetryEnabled,
            MeterName = Mediator.Meter.Name,
            ActivitySourceName = Mediator.ActivitySource.Name,
            Message = "Debug info for Blazing.Mediator"
        });
    }

    /// <summary>
    /// Handler for test notifications endpoint.
    /// </summary>
    private static async Task<IResult> TestNotifications(IMediator mediator)
    {
        Console.WriteLine("[+] Testing notification via Blazing.Mediator...");
        var notification = new TestNotification { Message = "Test notification from API" };
        await mediator.Publish(notification);
        Console.WriteLine("[+] Notification published successfully!");
        return Results.Ok(new { Message = "Test notification published successfully" });
    }

    /// <summary>
    /// Handler for middleware error testing endpoint.
    /// </summary>
    private static async Task<IResult> TestMiddlewareError(IMediator mediator)
    {
        Console.WriteLine("[!] Testing error middleware via Blazing.Mediator...");
        try
        {
            // This will go through the middleware pipeline and fail
            var command = new CreateUserCommand { Name = "Error User", Email = "error@example.com" };
            await mediator.Send(command);
            return Results.Ok(new { Message = "This should not be reached" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Error middleware test completed: {ex.Message}");
            return Results.Problem(
                title: "Middleware Error Test",
                detail: ex.Message,
                statusCode: 500
            );
        }
    }

    /// <summary>
    /// Handler for middleware validation testing endpoint.
    /// </summary>
    private static async Task<IResult> TestMiddlewareValidation(IMediator mediator)
    {
        Console.WriteLine("[!] Testing validation middleware via Blazing.Mediator...");
        try
        {
            // This will trigger validation errors
            var command = new CreateUserCommand { Name = "", Email = "invalid-email" };
            await mediator.Send(command);
            return Results.Ok(new { Message = "This should not be reached" });
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"[!] Validation middleware test completed: {ex.Message}");
            var errors = ex.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage });
            return Results.BadRequest(new { Message = "Validation failed (for testing)", Errors = errors });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[-] Unexpected error during validation test: {ex.Message}");
            return Results.Problem(
                title: "Unexpected Error",
                detail: ex.Message,
                statusCode: 500
            );
        }
    }

    #endregion
}