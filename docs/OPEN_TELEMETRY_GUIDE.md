 OpenTelemetry Support in Blazing.Mediator

Blazing.Mediator provides comprehensive built-in support for OpenTelemetry, allowing you to trace and monitor your application's behavior with detailed telemetry data across commands, queries, notifications, and streaming operations.

## Installation

First, make sure you have the necessary OpenTelemetry packages installed:

```xml
<PackageReference Include="OpenTelemetry" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.9.0" />
<!-- Add other exporters as needed -->
```

## Configuration

### Basic Setup

To enable OpenTelemetry in your application, configure both the Mediator and OpenTelemetry services:

```csharp
using Blazing.Mediator.Extensions;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add Blazing.Mediator with OpenTelemetry enabled
builder.Services.AddBlazingMediator(options =>
{
    options.EnableOpenTelemetry = true;
});

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(traceBuilder =>
    {
        traceBuilder
            .AddSource(ActivitySources.BlazingMediatorSource)
            .AddSource(ActivitySources.BlazingMediatorStreamingSource)
            .AddConsoleExporter();
    });

var app = builder.Build();
```

### Activity Sources

Blazing.Mediator uses two main activity sources:

-   `ActivitySources.BlazingMediatorSource` - For commands, queries, and notifications
-   `ActivitySources.BlazingMediatorStreamingSource` - For streaming operations

## Features

### Automatic Telemetry Creation

When OpenTelemetry is enabled, Blazing.Mediator automatically creates activities and spans for:

-   **Command execution** - Traces command processing from start to finish
-   **Query execution** - Monitors query performance and results
-   **Notification handling** - Tracks notification distribution and processing
-   **Streaming operations** - Monitors streaming request lifecycle and data flow
-   **Pipeline behavior execution** - Traces middleware and behavior execution

### Telemetry Context and Tags

The mediator automatically adds contextual information to spans:

#### Standard Tags

-   `mediator.request.type` - The type of the request being processed
-   `mediator.handler.type` - The type of the handler processing the request
-   `mediator.operation.type` - The operation type (Request, Notification, Stream)

#### Streaming-Specific Tags

-   `mediator.streaming.request.type` - Type of streaming request
-   `mediator.streaming.handler.type` - Type of streaming handler
-   `mediator.streaming.item.count` - Number of items processed in stream
-   `mediator.streaming.duration` - Total streaming duration

### Error and Exception Tracking

Exceptions during request processing are automatically recorded with:

-   Exception type and message
-   Stack trace information
-   Error status on the span

## Usage Examples

### Basic Request Tracing

All requests are automatically traced without additional code:

```csharp
// Command - automatically traced
public record CreateUserCommand(string Name, string Email) : IRequest<User>;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, User>
{
    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Handler logic - automatically traced
        return await CreateUserInDatabase(request.Name, request.Email);
    }
}

// Query - automatically traced
public record GetUserQuery(int Id) : IRequest<User>;

public class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Handler execution automatically traced
        return await GetUserFromDatabase(request.Id);
    }
}
```

### Streaming Request Tracing

Streaming operations get comprehensive telemetry:

```csharp
public record GetUsersStreamQuery(int PageSize) : IStreamRequest<User>;

public class GetUsersStreamHandler : IStreamRequestHandler<GetUsersStreamQuery, User>
{
    public async IAsyncEnumerable<User> Handle(GetUsersStreamQuery request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Streaming execution automatically traced
        // Item count and duration tracked automatically
        await foreach (var user in GetUsersFromDatabaseStream(request.PageSize))
        {
            yield return user;
        }
    }
}
```

### Adding Custom Telemetry

Enhance traces with custom telemetry data:

```csharp
using System.Diagnostics;

public class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        using var activity = Activity.Current;

        // Add custom tags for better observability
        activity?.SetTag("user.id", request.Id.ToString());
        activity?.SetTag("operation.category", "user-lookup");
        activity?.SetTag("cache.enabled", true);

        // Check cache first
        var cachedUser = await GetUserFromCache(request.Id);
        if (cachedUser != null)
        {
            activity?.SetTag("cache.hit", true);
            return cachedUser;
        }

        activity?.SetTag("cache.hit", false);

        // Get from database
        var user = await GetUserFromDatabase(request.Id);
        activity?.SetTag("user.found", user != null);

        if (user != null)
        {
            await CacheUser(user);
            activity?.SetTag("cache.stored", true);
        }

        return user;
    }
}
```

### Notification Tracing

Notifications are traced across all handlers:

```csharp
public record UserCreatedNotification(int UserId, string Name) : INotification;

// Each handler gets its own span
public class EmailNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Email sending automatically traced
        await SendWelcomeEmail(notification.UserId, notification.Name);
    }
}

public class AuditLogHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Audit logging automatically traced
        await LogUserCreation(notification.UserId);
    }
}
```

## Configuration Options

### Mediator Configuration

```csharp
builder.Services.AddBlazingMediator(options =>
{
    options.EnableOpenTelemetry = true;
    // OpenTelemetry is automatically configured when enabled
});
```

### OpenTelemetry Sampling and Filtering

Configure sampling to manage trace volume:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(traceBuilder =>
    {
        traceBuilder
            .AddSource(ActivitySources.BlazingMediatorSource)
            .AddSource(ActivitySources.BlazingMediatorStreamingSource)
            .SetSampler(new TraceIdRatioBasedSampler(0.1)) // Sample 10% of traces
            .AddConsoleExporter();
    });
```

### Production Configuration

Example production setup with multiple exporters:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(traceBuilder =>
    {
        traceBuilder
            .AddSource(ActivitySources.BlazingMediatorSource)
            .AddSource(ActivitySources.BlazingMediatorStreamingSource)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddJaegerExporter(options =>
            {
                options.AgentHost = builder.Configuration["Jaeger:Host"];
                options.AgentPort = int.Parse(builder.Configuration["Jaeger:Port"]);
            })
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"]);
            });
    });
```

## Integration with ASP.NET Core

Blazing.Mediator's OpenTelemetry integrates seamlessly with ASP.NET Core telemetry:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(traceBuilder =>
    {
        traceBuilder
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.Filter = (httpContext) =>
                {
                    // Filter out health check requests
                    return !httpContext.Request.Path.StartsWithSegments("/health");
                };
            })
            .AddSource(ActivitySources.BlazingMediatorSource)
            .AddSource(ActivitySources.BlazingMediatorStreamingSource)
            .AddConsoleExporter();
    });
```

This creates complete distributed traces from HTTP request through mediator execution.

## Monitoring Patterns

### Performance Monitoring

Use telemetry to monitor performance patterns:

```csharp
public class PerformanceMonitoringBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using var activity = Activity.Current;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();
            activity?.SetTag("performance.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetTag("performance.status", "success");

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                activity?.SetTag("performance.slow_request", true);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("performance.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetTag("performance.status", "error");
            activity?.SetTag("performance.error_type", ex.GetType().Name);
            throw;
        }
    }
}
```

### Business Metrics

Track business-relevant metrics:

```csharp
public class BusinessMetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using var activity = Activity.Current;

        // Add business context
        activity?.SetTag("business.operation", typeof(TRequest).Name);
        activity?.SetTag("business.user_id", GetCurrentUserId());
        activity?.SetTag("business.tenant_id", GetCurrentTenantId());

        var response = await next();

        // Track business outcomes
        if (request is ICommand)
        {
            activity?.SetTag("business.command_executed", true);
        }

        return response;
    }
}
```

## Best Practices

1. **Activity Source Registration**: Always register both mediator activity sources for complete coverage
2. **Custom Attributes**: Add meaningful business context through custom tags
3. **Sampling Strategy**: Use appropriate sampling rates for production to balance observability and performance
4. **Error Tracking**: Let the mediator handle exception recording automatically
5. **Performance Impact**: Monitor the overhead of telemetry collection in production
6. **Security**: Avoid including sensitive data (passwords, tokens) in span attributes
7. **Correlation**: Use correlation IDs to track requests across service boundaries

## Troubleshooting

### No Traces Appearing

1. Verify OpenTelemetry is enabled: `options.EnableOpenTelemetry = true`
2. Check activity source registration matches the constants:
    ```csharp
    .AddSource(ActivitySources.BlazingMediatorSource)
    .AddSource(ActivitySources.BlazingMediatorStreamingSource)
    ```
3. Ensure exporters are properly configured and accessible
4. Check sampling configuration isn't filtering out traces

### Incomplete Traces

1. Verify all activity sources are registered
2. Check for exceptions during handler execution
3. Ensure proper async/await patterns in handlers

### Performance Issues

1. Implement sampling to reduce trace volume
2. Use filtering to exclude high-frequency, low-value operations
3. Monitor telemetry collection overhead
4. Consider async exporters for production

### Missing Streaming Telemetry

1. Ensure `ActivitySources.BlazingMediatorStreamingSource` is registered
2. Verify streaming handlers implement `IAsyncEnumerable<T>` correctly
3. Check for proper cancellation token usage

## Example: Complete Setup

Here's a complete example with production-ready configuration:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Blazing.Mediator with OpenTelemetry
builder.Services.AddBlazingMediator(options =>
{
    options.EnableOpenTelemetry = true;
});

// Add comprehensive OpenTelemetry configuration
builder.Services.AddOpenTelemetry()
    .WithTracing(traceBuilder =>
    {
        traceBuilder
            // Add ASP.NET Core instrumentation
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.Filter = httpContext =>
                    !httpContext.Request.Path.StartsWithSegments("/health");
            })

            // Add Blazing.Mediator sources
            .AddSource(ActivitySources.BlazingMediatorSource)
            .AddSource(ActivitySources.BlazingMediatorStreamingSource)

            // Add other instrumentation
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()

            // Configure sampling
            .SetSampler(new TraceIdRatioBasedSampler(0.1))

            // Add exporters based on environment
            .AddConsoleExporter(); // Development only

        if (builder.Environment.IsProduction())
        {
            traceBuilder.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"]);
                options.Headers = builder.Configuration["OpenTelemetry:Headers"];
            });
        }
    });

var app = builder.Build();
```

This setup provides comprehensive observability for your Blazing.Mediator application with minimal configuration.
| `mediator.publish.success` | Counter | Number of successful publish operations |
| `mediator.publish.failure` | Counter | Number of failed publish operations |
| `mediator.publish.subscriber.duration` | Histogram | Duration of individual subscriber processing |
| `mediator.publish.subscriber.success` | Counter | Number of successful subscriber notifications |
| `mediator.publish.subscriber.failure` | Counter | Number of failed subscriber notifications |

#### Streaming Operations

| Metric Name                              | Type      | Description                               |
| ---------------------------------------- | --------- | ----------------------------------------- |
| `mediator.stream.duration`               | Histogram | Total duration of streaming operations    |
| `mediator.stream.success`                | Counter   | Number of successful streaming operations |
| `mediator.stream.failure`                | Counter   | Number of failed streaming operations     |
| `mediator.stream.item_count`             | Histogram | Number of items streamed                  |
| `mediator.stream.time_to_first_byte`     | Histogram | Time until first item is received         |
| `mediator.stream.packet.processing_time` | Histogram | Individual packet processing times        |
| `mediator.stream.inter_packet_time`      | Histogram | Time between consecutive packets          |
| `mediator.stream.throughput`             | Histogram | Items per second throughput               |

#### Health and System Metrics

| Metric Name                           | Type    | Description                               |
| ------------------------------------- | ------- | ----------------------------------------- |
| `mediator.telemetry.health`           | Counter | Health check counter for telemetry system |
| `mediator.middleware.execution_count` | Counter | Number of middleware executions           |
| `mediator.handler.execution_count`    | Counter | Number of handler executions              |

### Tracing (Activities)

#### Request/Response Tracing

-   **Request Processing** - Complete request/response lifecycle
-   **Middleware Execution** - Individual middleware execution spans
-   **Handler Execution** - Business logic execution spans
-   **Error Tracking** - Exception details and stack traces
-   **Validation Results** - Validation success/failure details

#### Streaming Tracing

-   **Stream Initialization** - Stream setup and configuration
-   **Packet Processing** - Individual packet processing spans
-   **Stream Completion** - Final statistics and cleanup
-   **Performance Classification** - Throughput analysis and patterns

### Tags and Attributes

#### Common Tags

-   `request_name` - The request type name (sanitized)
-   `request_type` - "query", "command", or "stream"
-   `response_type` - The response type name
-   `middleware.pipeline` - Complete middleware pipeline
-   `middleware.executed` - List of executed middleware
-   `handler.type` - The handler type name

#### Error Tags

-   `exception.type` - Exception type (sanitized)
-   `exception.message` - Exception message (sanitized, filtered for sensitive data)
-   `validation.passed` - Validation result

#### Performance Tags

-   `performance.duration_ms` - Operation duration in milliseconds
-   `performance.classification` - Performance classification (fast/normal/slow)

#### Streaming-Specific Tags

-   `stream.item_count` - Total items in the stream
-   `stream.time_to_first_byte_ms` - Time to first item in milliseconds
-   `stream.average_inter_packet_time_ms` - Average time between packets
-   `stream.throughput_items_per_second` - Throughput measurement
-   `stream.packet_size_bytes` - Packet size (when enabled)
-   `stream.performance_pattern` - Detected performance pattern

---

## Monitoring and Observability

### Console Output

The application outputs telemetry data to the console in real-time, including trace IDs, span IDs, and activity details for each operation. This enables immediate feedback during development and troubleshooting.

### Prometheus Metrics

Metrics are exposed at `/metrics` in Prometheus format, ready for scraping by Prometheus or compatible tools. Example metrics include operation durations, success/failure counts, and health checks.

### Health Checks

A dedicated health check endpoint (`/health`) reports the status of the telemetry system, including whether metrics and tracing are enabled and operational.

---

## How to Integrate OpenTelemetry in Your Application

### 1. Install Required Packages

Add the following NuGet packages to your project:

-   `OpenTelemetry`
-   `OpenTelemetry.Extensions.Hosting`
-   `OpenTelemetry.Exporter.Console`
-   `OpenTelemetry.Exporter.OpenTelemetryProtocol`
-   `OpenTelemetry.Instrumentation.AspNetCore`
-   `OpenTelemetry.Instrumentation.Http`
-   `Blazing.Mediator.OpenTelemetry` (if available)

### 2. Configure OpenTelemetry in Program.cs

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("Blazing.Mediator")
            .AddMeter("YourAppName")
            .AddConsoleExporter()
            .AddPrometheusExporter();
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("Blazing.Mediator")
            .AddSource("YourAppName")
            .AddConsoleExporter();
    });
```

### 3. Enable Mediator Telemetry

```csharp
// Enable telemetry with default settings
builder.Services.AddMediatorTelemetry();

// Or configure with custom options
builder.Services.ConfigureMediatorTelemetry(options =>
{
    options.Enabled = true;
    options.CaptureMiddlewareDetails = true;
    options.CaptureHandlerDetails = true;
    options.CaptureExceptionDetails = true;
    options.MaxExceptionMessageLength = 200;
    options.SensitiveDataPatterns.Add("custom_sensitive_pattern");
});
```

### 4. Expose Metrics and Health Endpoints

Ensure your API exposes endpoints for metrics and health checks:

-   `/metrics` for Prometheus
-   `/health` for health status
-   `/telemetry/metrics` for telemetry info
-   `/telemetry/health` for telemetry health

### 5. Monitor and Visualize

-   Use the console output for real-time feedback during development.
-   Integrate with Prometheus and Grafana for production monitoring.
-   Use the health endpoints to verify telemetry is working as expected.

---

## Best Practices

-   **Sanitize Sensitive Data:** Use built-in and custom patterns to filter sensitive information from telemetry.
-   **Use Health Checks:** Regularly check the `/health` and `/telemetry/health` endpoints.
-   **Leverage Middleware:** Add custom middleware for additional metrics or tracing as needed.
-   **Test Error Scenarios:** Simulate errors and validation failures to ensure telemetry captures all relevant data.
-   **Document Your Metrics:** Keep a reference of all custom and built-in metrics for your team.

---

## Learn More

-   [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/languages/net/)
-   [Prometheus Metrics](https://prometheus.io/docs/concepts/metric_types/)
-   [ASP.NET Core Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
-   [Blazing.Mediator Documentation](../README.md)

---

This guide provides a practical overview for developers to quickly enable, configure, and monitor OpenTelemetry-powered metrics and tracing in Blazing.Mediator-based applications. For advanced scenarios, refer to the sample project and official documentation.
