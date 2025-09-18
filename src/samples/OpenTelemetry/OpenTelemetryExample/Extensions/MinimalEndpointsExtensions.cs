using Blazing.Mediator;
using OpenTelemetryExample.Application.Commands;
using OpenTelemetryExample.Application.Queries;
using OpenTelemetryExample.Exceptions;
using OpenTelemetryExample.Models;
using OpenTelemetryExample.Shared.Models;

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

        // NEW: Add a simple test endpoint to verify basic connectivity
        telemetryGroup.MapGet("/test", ()
                => Results.Ok(new
                {
                    Message = "Telemetry API is working!",
                    Timestamp = DateTime.UtcNow,
                    Status = "OK"
                }))
            .WithName("TestTelemetryAPI")
            .WithSummary("Simple connectivity test")
            .WithDescription("Returns a simple response to verify the telemetry API is accessible.");

        telemetryGroup.MapGet("/health", GetTelemetryHealth)
            .WithName("GetTelemetryHealth")
            .WithSummary("Get telemetry system health status")
            .WithDescription("Returns the current health status of the OpenTelemetry and Blazing.Mediator telemetry systems.")
            .Produces<TelemetryHealthDto>()
            .Produces(500);

        telemetryGroup.MapGet("/metrics", GetTelemetryMetrics)
            .WithName("GetTelemetryInfo")
            .WithSummary("Get telemetry configuration information")
            .WithDescription("Returns basic information about the telemetry configuration including meter and activity source names.")
            .Produces<object>();

        // NEW: Add endpoints for actual telemetry data with strongly-typed models
        telemetryGroup.MapGet("/live-metrics", GetLiveMetrics)
            .WithName("GetLiveMetrics")
            .WithSummary("Get live telemetry metrics")
            .WithDescription("Returns current telemetry metrics data for display in the dashboard.")
            .Produces<LiveMetricsDto>();

        telemetryGroup.MapGet("/traces", async (IMediator mediator, int? maxRecords, bool? blazingMediatorOnly, bool? exampleAppOnly, int? timeWindowMinutes) =>
                await GetRecentTraces(mediator, maxRecords, blazingMediatorOnly, exampleAppOnly, timeWindowMinutes))
            .WithName("GetRecentTraces")
            .WithSummary("Get recent trace data")
            .WithDescription("Returns recent OpenTelemetry traces for display in the dashboard. Supports filtering and pagination.")
            .Produces<RecentTracesDto>();

        telemetryGroup.MapGet("/activities", GetRecentActivities)
            .WithName("GetRecentActivities")
            .WithSummary("Get recent activity data")
            .WithDescription("Returns recent OpenTelemetry activities for display in the dashboard.")
            .Produces<RecentActivitiesDto>();

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
            
            // Return strongly-typed TelemetryHealthDto
            var result = new TelemetryHealthDto
            {
                IsHealthy = health.IsHealthy,
                IsEnabled = health.IsEnabled,
                CanRecordMetrics = health.CanRecordMetrics,
                MeterName = health.MeterName ?? "unknown",
                ActivitySourceName = health.ActivitySourceName ?? "unknown",
                Message = health.Message
            };
            
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[-] Telemetry health check failed: {ex.Message}");
            var errorResult = new TelemetryHealthDto
            {
                IsHealthy = false,
                IsEnabled = false,
                CanRecordMetrics = false,
                MeterName = "error",
                ActivitySourceName = "error",
                Message = $"Health check failed: {ex.Message}"
            };
            
            return Results.Ok(errorResult);
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
    /// Handler for live metrics endpoint - now returns real data from OpenTelemetry
    /// </summary>
    private static async Task<IResult> GetLiveMetrics(IMediator mediator)
    {
        try
        {
            var query = new GetLiveMetricsQuery
            {
                MaxRecords = 100,
                TimeWindow = TimeSpan.FromMinutes(30)
            };

            var result = await mediator.Send(query);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[-] Error getting live metrics: {ex.Message}");
            
            // Return fallback data in case of error
            var fallbackResult = new LiveMetricsDto
            {
                Timestamp = DateTime.UtcNow,
                Metrics = new MetricsData(),
                Commands = [],
                Queries = [],
                Message = $"Error retrieving live metrics: {ex.Message}"
            };
            
            return Results.Ok(fallbackResult);
        }
    }

    /// <summary>
    /// Handler for recent traces endpoint - now returns real data from OpenTelemetry
    /// </summary>
    private static async Task<IResult> GetRecentTraces(IMediator mediator, int? maxRecords, bool? blazingMediatorOnly, bool? exampleAppOnly, int? timeWindowMinutes)
    {
        try
        {
            var query = new GetRecentTracesQuery
            {
                MaxRecords = maxRecords ?? 10,
                TimeWindow = TimeSpan.FromMinutes(timeWindowMinutes ?? 30),
                MediatorOnly = blazingMediatorOnly ?? false,
                ExampleAppOnly = exampleAppOnly ?? false
            };

            var result = await mediator.Send(query);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[-] Error getting recent traces: {ex.Message}");
            
            // Return fallback data in case of error
            var fallbackResult = new RecentTracesDto
            {
                Timestamp = DateTime.UtcNow,
                Traces = [],
                Message = $"Error retrieving traces: {ex.Message}"
            };
            
            return Results.Ok(fallbackResult);
        }
    }

    /// <summary>
    /// Handler for recent activities endpoint - now returns real data from OpenTelemetry
    /// </summary>
    private static async Task<IResult> GetRecentActivities(IMediator mediator)
    {
        try
        {
            var query = new GetRecentActivitiesQuery
            {
                MaxRecords = 50,
                TimeWindow = TimeSpan.FromMinutes(30)
            };

            var result = await mediator.Send(query);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[-] Error getting recent activities: {ex.Message}");
            
            // Return fallback data in case of error
            var fallbackResult = new RecentActivitiesDto
            {
                Timestamp = DateTime.UtcNow,
                Activities = [],
                Message = $"Error retrieving activities: {ex.Message}"
            };
            
            return Results.Ok(fallbackResult);
        }
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