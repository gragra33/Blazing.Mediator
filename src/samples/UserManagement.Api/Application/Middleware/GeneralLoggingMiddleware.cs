using Blazing.Mediator;
using System.Text.Json;

namespace UserManagement.Api.Application.Middleware;

/// <summary>
/// General logging middleware that logs all requests and responses.
/// Demonstrates standard (non-conditional) middleware execution.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class GeneralLoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Execution order - standard order
    /// </summary>
    public int Order => 0;

    /// <summary>
    /// Handles all requests by logging them
    /// </summary>
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Use Console.WriteLine for simple logging since we can't easily inject ILogger
        var requestType = request.GetType().Name;
        var startTime = DateTime.UtcNow;

        // Log the request - using ASCII instead of Unicode
        Console.WriteLine($">> REQUEST: {requestType} started at {startTime:yyyy-MM-dd HH:mm:ss.fff}");

        try
        {
            // Serialize and log request details (be careful with sensitive data in production)
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            Console.WriteLine($">> REQUEST DATA: {requestJson}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($">> Could not serialize request: {ex.Message}");
        }

        TResponse response;
        try
        {
            // Execute the next middleware or handler
            response = await next();

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Log successful response
            Console.WriteLine($"<< RESPONSE: {requestType} completed successfully in {duration.TotalMilliseconds}ms at {endTime:yyyy-MM-dd HH:mm:ss.fff}");

            try
            {
                // Serialize and log response details
                var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                Console.WriteLine($"<< RESPONSE DATA: {responseJson}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"<< Could not serialize response: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Log error response
            Console.WriteLine($"!! ERROR: {requestType} failed after {duration.TotalMilliseconds}ms at {endTime:yyyy-MM-dd HH:mm:ss.fff} - {ex.Message}");

            throw;
        }

        return response;
    }
}
