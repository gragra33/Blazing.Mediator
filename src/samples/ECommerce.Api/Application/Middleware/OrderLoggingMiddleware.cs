using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using System.Text.Json;

namespace ECommerce.Api.Application.Middleware;

/// <summary>
/// Conditional middleware that logs only order-related requests and responses.
/// Demonstrates conditional middleware execution based on request type.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class OrderLoggingMiddleware<TRequest, TResponse> : IConditionalMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<OrderLoggingMiddleware<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the OrderLoggingMiddleware.
    /// </summary>
    public OrderLoggingMiddleware()
    {
        // Create a logger factory for middleware that doesn't have DI
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole().SetMinimumLevel(LogLevel.Information);
        });
        _logger = loggerFactory.CreateLogger<OrderLoggingMiddleware<TRequest, TResponse>>();
    }

    /// <summary>
    /// Execution order - run before product middleware
    /// </summary>
    public int Order => 1;

    /// <summary>
    /// Determines if this middleware should execute based on whether the request is order-related
    /// </summary>
    public bool ShouldExecute(TRequest request)
    {
        var requestType = request.GetType().Name;
        return requestType.Contains("Order", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Handles the request by logging order-related operations
    /// </summary>
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = request.GetType().Name;
        var startTime = DateTime.UtcNow;

        // Log the request
        _logger.LogInformation("🛒 ORDER REQUEST: {RequestType} started at {StartTime}",
            requestType, startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

        try
        {
            // Serialize and log request details (be careful with sensitive data in production)
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            _logger.LogInformation("🛒 ORDER REQUEST DATA: {RequestData}", requestJson);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("🛒 Could not serialize order request: {Error}", ex.Message);
        }

        TResponse response;
        try
        {
            // Execute the next middleware or handler
            response = await next();

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Log successful response
            _logger.LogInformation("🛒 ORDER RESPONSE: {RequestType} completed successfully in {Duration}ms at {EndTime}",
                requestType, duration.TotalMilliseconds, endTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            try
            {
                // Serialize and log response details
                var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                _logger.LogInformation("🛒 ORDER RESPONSE DATA: {ResponseData}", responseJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("🛒 Could not serialize order response: {Error}", ex.Message);
            }
        }
        catch (Exception ex)
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Log error response
            _logger.LogError("🛒 ORDER ERROR: {RequestType} failed after {Duration}ms at {EndTime} - {Error}",
                requestType, duration.TotalMilliseconds, endTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), ex.Message);

            throw;
        }

        return response;
    }
}
