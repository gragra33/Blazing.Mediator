using Blazing.Mediator;
using Blazing.Mediator;
using System.Text.Json;

namespace UserManagement.Api.Application.Middleware;

/// <summary>
/// General logging middleware for void commands that logs all requests.
/// Demonstrates standard (non-conditional) middleware execution for commands.
/// </summary>
/// <typeparam name="TRequest">The command type</typeparam>
public class GeneralCommandLoggingMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Execution order - standard order
    /// </summary>
    public int Order => 0;

    /// <summary>
    /// Handles all commands by logging them
    /// </summary>
    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        // Use Console.WriteLine for simple logging since we can't easily inject ILogger
        var requestType = request.GetType().Name;
        var startTime = DateTime.UtcNow;

        // Log the command
        Console.WriteLine($"🔍 COMMAND: {requestType} started at {startTime:yyyy-MM-dd HH:mm:ss.fff}");

        try
        {
            // Serialize and log command details (be careful with sensitive data in production)
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            Console.WriteLine($"🔍 COMMAND DATA: {requestJson}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔍 Could not serialize command: {ex.Message}");
        }

        try
        {
            // Execute the next middleware or handler
            await next();

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Log successful completion
            Console.WriteLine($"🔍 COMMAND COMPLETED: {requestType} completed successfully in {duration.TotalMilliseconds}ms at {endTime:yyyy-MM-dd HH:mm:ss.fff}");
        }
        catch (Exception ex)
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Log error
            Console.WriteLine($"🔍 COMMAND ERROR: {requestType} failed after {duration.TotalMilliseconds}ms at {endTime:yyyy-MM-dd HH:mm:ss.fff} - {ex.Message}");

            throw;
        }
    }
}
