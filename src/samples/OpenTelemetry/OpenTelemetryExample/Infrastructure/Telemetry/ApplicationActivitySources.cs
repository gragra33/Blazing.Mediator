using System.Diagnostics;

namespace OpenTelemetryExample.Infrastructure.Telemetry;

/// <summary>
/// Centralized management of ActivitySource instances for the OpenTelemetry example application.
/// Provides static, thread-safe ActivitySource instances for different application layers.
/// </summary>
/// <remarks>
/// This class follows OpenTelemetry best practices by providing:
/// - Static ActivitySource instances for optimal performance
/// - Proper naming conventions for telemetry correlation
/// - Centralized management for easy maintenance
/// - Thread-safe access patterns
/// - Proper disposal management
/// </remarks>
public static class ApplicationActivitySources
{
    private const string AppSourceName = "OpenTelemetryExample";

    /// <summary>
    /// ActivitySource for application handlers (queries, commands, and business logic).
    /// Use this for handler-level spans that represent business operations.
    /// </summary>
    public static readonly ActivitySource Handlers = new($"{AppSourceName}.Handlers");

    /// <summary>
    /// ActivitySource for API controllers and endpoints.
    /// Use this for HTTP request handling and API-level operations.
    /// </summary>
    public static readonly ActivitySource Controllers = new($"{AppSourceName}.Controllers");

    /// <summary>
    /// ActivitySource for application services and business logic.
    /// Use this for service-level operations and business workflows.
    /// </summary>
    public static readonly ActivitySource Services = new($"{AppSourceName}.Services");

    /// <summary>
    /// ActivitySource for infrastructure operations (database, external APIs, etc.).
    /// Use this for infrastructure-level spans like database queries or HTTP calls.
    /// </summary>
    public static readonly ActivitySource Infrastructure = new($"{AppSourceName}.Infrastructure");

    /// <summary>
    /// ActivitySource for middleware operations and cross-cutting concerns.
    /// Use this for middleware, filters, and pipeline operations.
    /// </summary>
    public static readonly ActivitySource Middleware = new($"{AppSourceName}.Middleware");

    /// <summary>
    /// Disposes all ActivitySource instances.
    /// Should be called during application shutdown to ensure proper cleanup.
    /// </summary>
    /// <remarks>
    /// This method is typically called from the application's disposal mechanism
    /// or from a hosted service's StopAsync method to ensure all telemetry
    /// resources are properly released.
    /// </remarks>
    public static void Dispose()
    {
        Handlers.Dispose();
        Controllers.Dispose();
        Services.Dispose();
        Infrastructure.Dispose();
        Middleware.Dispose();
    }
}