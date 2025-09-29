# OpenTelemetry Support Guide

Blazing.Mediator provides comprehensive built-in support for OpenTelemetry, allowing you to trace and monitor your application's behavior with detailed telemetry data across commands, queries, notifications, and streaming operations.

## Quick Reference Tables

The following tables provide a comprehensive overview of all telemetry configuration options, metrics, and tags available in Blazing.Mediator's OpenTelemetry integration. These reference tables are designed to help you quickly find the specific telemetry properties you need when configuring monitoring and observability for your application. Each table focuses on a different aspect of the telemetry system, from basic configuration to advanced streaming metrics.

### TelemetryOptions Configuration Properties

The TelemetryOptions class provides fine-grained control over what telemetry data is collected and how it's processed. These properties allow you to balance observability needs with performance requirements, enabling you to capture detailed information in development while optimizing for production workloads. Each property can be configured independently to create a telemetry profile that matches your specific monitoring requirements.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | `bool` | `true` | Enable/disable telemetry collection |
| `CaptureMiddlewareDetails` | `bool` | `true` | Capture detailed middleware execution information |
| `CaptureHandlerDetails` | `bool` | `true` | Capture detailed handler execution information |
| `CaptureExceptionDetails` | `bool` | `true` | Capture detailed exception information |
| `CaptureNotificationHandlerDetails` | `bool` | `true` | Capture detailed notification handler information |
| `CreateHandlerChildSpans` | `bool` | `true` | Create child spans for individual notification handlers |
| `CaptureSubscriberMetrics` | `bool` | `true` | Capture notification subscriber metrics |
| `CaptureNotificationMiddlewareDetails` | `bool` | `true` | Capture notification middleware execution details |
| `EnableHealthChecks` | `bool` | `true` | Enable health check endpoints for telemetry |
| `MaxExceptionMessageLength` | `int` | `200` | Maximum length of exception messages in telemetry |
| `MaxStackTraceLines` | `int` | `3` | Maximum number of stack trace lines to capture |
| `PacketLevelTelemetryEnabled` | `bool` | `false` | Enable detailed streaming packet telemetry |
| `PacketTelemetryBatchSize` | `int` | `10` | Batch size for packet-level telemetry |
| `EnableStreamingMetrics` | `bool` | `true` | Enable enhanced streaming metrics including jitter analysis |
| `CapturePacketSize` | `bool` | `false` | Capture packet size information when possible |
| `EnableStreamingPerformanceClassification` | `bool` | `true` | Enable detailed streaming performance classification |
| `ExcellentPerformanceThreshold` | `double` | `0.1` | Threshold for "excellent" performance (10% jitter) |
| `GoodPerformanceThreshold` | `double` | `0.3` | Threshold for "good" performance (30% jitter) |
| `FairPerformanceThreshold` | `double` | `0.5` | Threshold for "fair" performance (50% jitter) |

### TelemetryOptions Default Configurations

Blazing.Mediator provides several preset configurations that are optimized for different environments and use cases. These presets represent battle-tested combinations of settings that balance observability needs with performance considerations.

| Configuration | Core Telemetry | Exceptions | Streaming | Notifications | Health | Performance |
|---------------|----------------|------------|-----------|-------------|---------|-------------|
| **Default (new TelemetryOptions())** | Enabled | All enabled | Metrics only | All enabled | Enabled | Basic |
| **TelemetryOptions.Development()** | All enabled | Verbose (500/10 lines) | Full packet tracking | All enabled | Enabled | All enabled |
| **TelemetryOptions.Production()** | Core only | Limited (200/3 lines) | Metrics only | Handler spans disabled | Enabled | Optimized |
| **TelemetryOptions.Disabled()** | All disabled | Disabled | All disabled | All disabled | Disabled | Disabled |
| **TelemetryOptions.Minimal()** | Exceptions only | Basic (100/2 lines) | Disabled | Disabled | Enabled | Disabled |
| **TelemetryOptions.NotificationOnly()** | Disabled | Exceptions only | Disabled | All enabled | Disabled | N/A |
| **TelemetryOptions.StreamingOnly()** | Disabled | Exceptions only | All enabled | Disabled | Disabled | N/A |

### Activity Sources and Metrics

Blazing.Mediator uses multiple activity sources to organize telemetry data logically across different operational domains. This separation allows you to selectively enable or disable specific telemetry streams based on your monitoring needs. The activity sources are designed to provide hierarchical tracing where parent-child relationships between operations are clearly established, making it easier to understand request flows and identify performance bottlenecks.

| Component | Activity Source | Meter Name | Purpose |
|-----------|----------------|------------|---------|
| Core Operations | `Blazing.Mediator` | `Blazing.Mediator` | Commands, queries, notifications |
| Streaming | `Blazing.Mediator.Streaming` | `Blazing.Mediator` | Streaming operations |
| Health Checks | `Blazing.Mediator` | `Blazing.Mediator` | Telemetry system health |

### Core Telemetry Tags

These standardized tags provide consistent metadata across all telemetry data, enabling effective filtering, grouping, and analysis in your observability platform. The tags are automatically sanitized to remove sensitive information and are designed to provide both technical and business context for each operation. Understanding these tags is crucial for creating effective dashboards and alerts in your monitoring system.

| Tag Name | Type | Applied To | Description |
|----------|------|------------|-------------|
| `request_name` | `string` | All requests | Sanitized request type name |
| `request_type` | `string` | All requests | "query", "command", or "stream" |
| `response_type` | `string` | Request/Response | The response type name |
| `handler.type` | `string` | All handlers | The handler type name (sanitized) |
| `notification.type` | `string` | Notifications | The notification type name (sanitized) |
| `notification.handler_count` | `int` | Notifications | Number of handlers for the notification |
| `notification.subscriber_count` | `int` | Notifications | Number of subscribers for the notification |
| `notification.execution_pattern` | `string` | Notifications | Detected execution pattern |
| `subscriber_type` | `string` | Notification Handlers | The subscriber/handler type name (sanitized) |
| `processor_type` | `string` | Notification Handlers | Type of processor: "subscriber", "handler", "generic_subscriber" |
| `middleware.pipeline` | `string` | Requests | Complete middleware pipeline |
| `middleware.executed` | `string` | Requests | List of executed middleware |
| `notification_middleware.pipeline` | `string` | Notifications | Complete notification middleware pipeline |
| `notification_middleware.executed` | `string` | Notifications | List of executed notification middleware |
| `exception.type` | `string` | Errors | Exception type (sanitized) |
| `exception.message` | `string` | Errors | Exception message (sanitized) |
| `validation.passed` | `bool` | Validation | Validation result |
| `performance.duration_ms` | `long` | All operations | Operation duration in milliseconds |
| `performance.classification` | `string` | All operations | "fast", "normal", or "slow" |

### Streaming-Specific Tags

Streaming operations generate unique telemetry data that reflects the nature of data flow and processing patterns. These specialized tags capture metrics that are essential for understanding streaming performance, including throughput characteristics, timing patterns, and completion status. The streaming tags work in conjunction with core telemetry tags to provide a complete picture of streaming request lifecycle and performance.

| Tag Name | Type | Description |
|----------|------|-------------|
| `stream.item_count` | `long` | Total items in the stream |
| `stream.time_to_first_byte_ms` | `long` | Time to first item in milliseconds |
| `stream.average_inter_packet_time_ms` | `double` | Average time between packets |
| `stream.throughput_items_per_second` | `double` | Throughput measurement |
| `stream.packet_size_bytes` | `long` | Individual packet size (when enabled) |
| `stream.performance_pattern` | `string` | Detected performance pattern |
| `stream.completion_status` | `string` | Stream completion status |
| `stream.cancellation_reason` | `string` | Reason for stream cancellation (if applicable) |

### Request/Response Operations Metrics

These metrics capture the fundamental performance characteristics of synchronous request-response operations including commands and queries. The histogram metrics provide detailed distribution data that helps identify performance outliers and trends over time. Success and failure counters enable you to calculate error rates and establish SLA compliance metrics for your application's core operations.

| Metric Name | Type | Description |
|-------------|------|-------------|
| `mediator.send.duration` | Histogram | Duration of send operations |
| `mediator.send.success` | Counter | Number of successful send operations |
| `mediator.send.failure` | Counter | Number of failed send operations |

### Notification Operations Metrics

Notification metrics provide visibility into both the publishing process and individual subscriber processing performance. These metrics are essential for understanding the scalability and reliability of your event-driven architecture. The dual-level metrics (publish-level and subscriber-level) allow you to identify whether performance issues stem from the notification distribution mechanism or specific subscriber implementations.

| Metric Name | Type | Description |
|-------------|------|-------------|
| `mediator.publish.duration` | Histogram | Duration of publish operations |
| `mediator.publish.success` | Counter | Number of successful publish operations |
| `mediator.publish.failure` | Counter | Number of failed publish operations |
| `mediator.publish.partial_failure` | Counter | Number of notifications with partial handler failures |
| `mediator.publish.total_failure` | Counter | Number of notifications where all handlers failed |
| `mediator.publish.subscriber.duration` | Histogram | Duration of individual subscriber processing |
| `mediator.publish.subscriber.success` | Counter | Number of successful subscriber notifications |
| `mediator.publish.subscriber.failure` | Counter | Number of failed subscriber notifications |

### Streaming Operations Metrics

Streaming metrics capture the complex performance characteristics unique to asynchronous data streams. These metrics are designed to help you understand not just overall performance, but also the quality and consistency of the streaming experience. The detailed packet-level metrics can be enabled when you need to perform deep analysis of streaming behavior patterns and optimize for specific use cases.

| Metric Name | Type | Description |
|-------------|------|-------------|
| `mediator.stream.duration` | Histogram | Total duration of streaming operations |
| `mediator.stream.success` | Counter | Number of successful streaming operations |
| `mediator.stream.failure` | Counter | Number of failed streaming operations |
| `mediator.stream.item_count` | Histogram | Number of items streamed |
| `mediator.stream.time_to_first_byte` | Histogram | Time until first item is received |
| `mediator.stream.packet.processing_time` | Histogram | Individual packet processing times |
| `mediator.stream.inter_packet_time` | Histogram | Time between consecutive packets |
| `mediator.stream.packet.jitter` | Histogram | Packet timing variability |
| `mediator.stream.throughput` | Histogram | Items per second throughput |

### Health and System Metrics

System health metrics provide essential monitoring capabilities for the telemetry infrastructure itself. These metrics help ensure that your observability system is functioning correctly and can alert you to issues with telemetry collection before they impact your ability to monitor application performance. The health metrics also provide insight into the overhead and impact of telemetry collection on your application.

| Metric Name | Type | Description |
|-------------|------|-------------|
| `mediator.telemetry.health` | Counter | Health check counter for telemetry system |
| `mediator.middleware.execution_count` | Counter | Number of middleware executions |
| `mediator.handler.execution_count` | Counter | Number of handler executions |

### Sensitive Data Patterns (Default)

The default sensitive data patterns provide automatic protection against accidentally including confidential information in telemetry data. These patterns use regular expressions to identify and sanitize common types of sensitive data in property names, values, and messages. You can extend these patterns with custom rules specific to your domain, ensuring comprehensive protection of sensitive information while maintaining useful telemetry data.

| Pattern | Default Value | Description |
|---------|---------------|-------------|
| `password` | `"password"` | Filters password fields |
| `token` | `"token"` | Filters authentication tokens |
| `secret` | `"secret"` | Filters secret keys |
| `key` | `"key"` | Filters API keys |
| `auth` | `"auth"` | Filters authentication data |
| `credential` | `"credential"` | Filters credential information |
| `connection` | `"connection"` | Filters connection strings |

**Customizing Sensitive Data Patterns:**
```csharp
builder.Services.AddMediator(config =>
{
    config.WithTelemetry(options =>
    {
        // Add custom patterns specific to your domain
        options.SensitiveDataPatterns.Add("customer_id");
        options.SensitiveDataPatterns.Add("account_number");
        options.SensitiveDataPatterns.Add("ssn");
        options.SensitiveDataPatterns.Add("credit.card");
        
        // Remove default patterns if needed (not recommended)
        options.SensitiveDataPatterns.Remove("connection");
    });
    config.AddAssembly(typeof(Program).Assembly);
});
```

## Installation

First, make sure you have the necessary OpenTelemetry packages installed:

```xml
<PackageReference Include="OpenTelemetry" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.9.0" />
<!-- Add other exporters as needed -->
```

## Configuration

Configuring OpenTelemetry with Blazing.Mediator involves setting up both the mediator's telemetry options and the OpenTelemetry infrastructure. The configuration is designed to be flexible, allowing you to start with basic tracing and gradually add more sophisticated telemetry features as your observability requirements grow. The integration automatically handles activity creation, span management, and metric collection, requiring minimal additional code in your handlers and behaviors.

### Basic Setup

```csharp
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add Blazing.Mediator with OpenTelemetry enabled
builder.Services.AddMediator(config =>
{
    config.WithTelemetry(); // Uses default TelemetryOptions
    config.AddAssembly(typeof(Program).Assembly);
});

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(traceBuilder =>
    {
        traceBuilder
            .AddSource("Blazing.Mediator")
            .AddSource("Blazing.Mediator.Streaming")
            .AddConsoleExporter();
    });

var app = builder.Build();
```

### Advanced Telemetry Configuration

Configure telemetry options for detailed control:

```csharp
builder.Services.AddMediator(config =>
{
    config.WithTelemetry(telemetryOptions =>
    {
        telemetryOptions.CaptureMiddlewareDetails = true;
        telemetryOptions.CaptureHandlerDetails = true;
        telemetryOptions.CaptureExceptionDetails = true;
        telemetryOptions.MaxExceptionMessageLength = 300;
        telemetryOptions.MaxStackTraceLines = 15;
        
        // Notification-specific telemetry options
        telemetryOptions.CaptureNotificationHandlerDetails = true;
        telemetryOptions.CreateHandlerChildSpans = true;
        telemetryOptions.CaptureSubscriberMetrics = true;
        telemetryOptions.CaptureNotificationMiddlewareDetails = true;
        
        // Streaming telemetry options
        telemetryOptions.PacketLevelTelemetryEnabled = true;
        telemetryOptions.PacketTelemetryBatchSize = 50;
        telemetryOptions.SensitiveDataPatterns.Add("custom_sensitive_pattern");
    });
    config.AddAssembly(typeof(Program).Assembly);
});
```

### MediatorConfiguration Telemetry Methods

The MediatorConfiguration class provides several fluent methods for configuring telemetry:

#### Core Telemetry Configuration Methods

```csharp
builder.Services.AddMediator(config =>
{
    // Basic telemetry with default options
    config.WithTelemetry();
    
    // Telemetry with configuration action
    config.WithTelemetry(options =>
    {
        options.CaptureMiddlewareDetails = false;
        options.MaxExceptionMessageLength = 500;
    });
    
    // Telemetry with pre-configured options
    config.WithTelemetry(TelemetryOptions.Development());
    
    // Disable telemetry completely
    config.WithoutTelemetry();
    
    config.AddAssembly(typeof(Program).Assembly);
});
```

#### Notification Telemetry Configuration Methods

```csharp
builder.Services.AddMediator(config =>
{
    // Enable comprehensive notification telemetry
    config.WithNotificationTelemetry();
    
    // Configure notification telemetry with custom options
    config.WithNotificationTelemetry(options =>
    {
        options.CreateHandlerChildSpans = false; // Disable child spans for performance
        options.CaptureNotificationMiddlewareDetails = false;
    });
    
    // Enable/disable specific notification telemetry features
    config.WithHandlerChildSpans(enabled: true);
    config.WithSubscriberMetrics(enabled: true);
    config.WithNotificationHandlerDetails(enabled: true);
    config.WithNotificationMiddlewareDetails(enabled: false);
    
    // Disable all notification telemetry
    config.WithoutNotificationTelemetry();
    
    config.AddAssembly(typeof(Program).Assembly);
});
```

### Environment-Specific Configuration

```csharp
builder.Services.AddMediator(config =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Development: Comprehensive telemetry with detailed information
        config.WithTelemetry(TelemetryOptions.Development());
    }
    else if (builder.Environment.IsProduction())
    {
        // Production: Optimized telemetry with performance focus
        config.WithTelemetry(TelemetryOptions.Production());
    }
    else
    {
        // Staging/Test: Balanced configuration
        config.WithTelemetry(options =>
        {
            options.CaptureHandlerDetails = true;
            options.CaptureNotificationHandlerDetails = true;
            options.CreateHandlerChildSpans = false; // Performance optimization
            options.MaxExceptionMessageLength = 250;
            options.MaxStackTraceLines = 5;
        });
    }
    
    config.AddAssembly(typeof(Program).Assembly);
});
```

### Jaeger and OpenTelemetry Protocol (OTLP) Configuration

To configure Jaeger and OTLP exporters, add the following to your OpenTelemetry setup:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(traceBuilder =>
    {
        traceBuilder
            .AddSource("Blazing.Mediator")
            .AddSource("Blazing.Mediator.Streaming")
            .AddConsoleExporter()
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

### Activity Sources

Blazing.Mediator uses two main activity sources:

- `Blazing.Mediator` - For commands, queries, and notifications
- `Blazing.Mediator.Streaming` - For streaming operations

## Features

The OpenTelemetry integration in Blazing.Mediator is designed to provide comprehensive observability without requiring significant changes to your existing code. The telemetry system automatically instruments all mediator operations, creating detailed traces that show the complete request lifecycle including middleware execution, handler processing, and error handling. The integration is built with performance in mind, using efficient telemetry collection techniques and providing extensive configuration options to control the overhead based on your specific requirements.

### Automatic Telemetry Creation

When OpenTelemetry is enabled, Blazing.Mediator automatically creates activities and spans for:

- **Command execution** - Traces command processing from start to finish
- **Query execution** - Monitors query performance and results
- **Notification handling** - Tracks notification distribution and processing
- **Streaming operations** - Monitors streaming request lifecycle and data flow
- **Pipeline behavior execution** - Traces middleware and behavior execution

### Telemetry Context and Tags

The mediator automatically adds contextual information to spans with standard tags, streaming-specific tags, and error tracking as detailed in the Quick Reference Tables above.

### Error and Exception Tracking

Exceptions during request processing are automatically recorded with:

- Exception type and message (sanitized)
- Stack trace information (limited lines)
- Error status on the span
- Validation error details
- Individual notification handler failures vs. total failures

## Usage Examples

The following examples demonstrate how Blazing.Mediator's automatic telemetry works in practice and how you can enhance it with custom instrumentation. The telemetry integration is designed to work transparently with your existing code while providing rich observability data. These examples show both the automatic telemetry that's generated by default and techniques for adding custom telemetry data to provide additional business and operational context.

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

### Notification Tracing

Notifications are traced across all handlers with individual child spans:

```csharp
public record UserCreatedNotification(int UserId, string Name) : INotification;

// Each handler gets its own span automatically
public class EmailNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Email sending automatically traced with individual span
        // Tags: handler.type=EmailNotificationHandler, notification.type=UserCreatedNotification, operation=handle_notification
        await SendWelcomeEmail(notification.UserId, notification.Name);
    }
}

public class AuditLogHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Audit logging automatically traced with individual span
        // Tags: handler.type=AuditLogHandler, notification.type=UserCreatedNotification, operation=handle_notification
        await LogUserCreation(notification.UserId);
    }
}

public class CacheInvalidationHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Cache invalidation automatically traced with individual span
        // Performance metrics and error tracking included automatically
        await InvalidateUserCache(notification.UserId);
    }
}
```

The notification telemetry automatically creates:
- A parent span for the `Publish` operation with tags like `notification.handler_count=3`, `notification.subscriber_count=0`
- Individual child spans for each handler with handler-specific tags
- Success/failure metrics for each handler independently
- Duration metrics for both the overall publish operation and individual handlers
- Partial vs. total failure classification

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

// Custom telemetry in notification handlers
public class EmailNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        using var activity = Activity.Current;
        
        // Add business context to the automatic handler span
        activity?.SetTag("email.type", "welcome");
        activity?.SetTag("user.new_account", true);
        activity?.SetTag("notification.priority", "high");
        
        try
        {
            await SendWelcomeEmail(notification.UserId, notification.Name);
            
            // Add success metrics
            activity?.SetTag("email.sent", true);
            activity?.SetTag("email.provider", "sendgrid");
        }
        catch (EmailServiceException ex)
        {
            // Add failure context (exception details already captured automatically)
            activity?.SetTag("email.sent", false);
            activity?.SetTag("email.failure_reason", ex.ErrorCode);
            throw;
        }
    }
}
```

### Manual Notification Subscriber Tracing

Manual subscribers (Observer pattern) are also automatically traced:

```csharp
public class RealTimeNotificationSubscriber : INotificationSubscriber<UserCreatedNotification>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    
    public RealTimeNotificationSubscriber(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task OnNotification(UserCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // Real-time notification automatically traced
        // Tags: subscriber_type=RealTimeNotificationSubscriber, processor_type=subscriber
        await _hubContext.Clients.All.SendAsync("UserCreated", notification, cancellationToken);
    }
}

// Usage in your application startup
public void ConfigureServices(IServiceCollection services)
{
    services.AddMediator(config => config.AddAssembly(typeof(Program).Assembly));
    services.AddScoped<RealTimeNotificationSubscriber>();
}

public void Configure(IServiceProvider serviceProvider)
{
    var mediator = serviceProvider.GetRequiredService<IMediator>();
    var subscriber = serviceProvider.GetRequiredService<RealTimeNotificationSubscriber>();
    
    // Subscribe to notifications - automatically traced when notifications are published
    mediator.Subscribe<UserCreatedNotification>(subscriber);
}
```

### Notification Middleware Telemetry

Notification middleware is automatically instrumented:

```csharp
public class NotificationLoggingMiddleware<TNotification> : INotificationMiddleware<TNotification>
    where TNotification : INotification
{
    private readonly ILogger<NotificationLoggingMiddleware<TNotification>> _logger;
    
    public int Order => 0; // Execute first
    
    public NotificationLoggingMiddleware(ILogger<NotificationLoggingMiddleware<TNotification>> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TNotification notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
    {
        // Middleware execution automatically traced with span
        // Tags: middleware.type=NotificationLoggingMiddleware, notification.type=UserCreatedNotification
        
        _logger.LogInformation("Processing notification: {NotificationType}", typeof(TNotification).Name);
        
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await next();
            stopwatch.Stop();
            
            _logger.LogInformation("Notification processed successfully in {Duration}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Notification processing failed after {Duration}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

## Configuration Options

The configuration system provides multiple layers of customization to match your specific observability requirements. You can configure telemetry options at the mediator level to control what data is collected, and at the OpenTelemetry level to control how that data is processed, sampled, and exported. This flexible approach allows you to optimize for different environments, from detailed development tracing to production-optimized monitoring with controlled overhead and sensitive data protection.

### Mediator Configuration

```csharp
builder.Services.AddMediator(config =>
{
    config.WithTelemetry(options =>
    {
        // Core telemetry options
        options.CaptureMiddlewareDetails = true;
        options.CaptureHandlerDetails = true;
        options.CaptureExceptionDetails = true;
        options.MaxExceptionMessageLength = 200;
        options.MaxStackTraceLines = 10;
        
        // Notification-specific telemetry options
        options.CaptureNotificationHandlerDetails = true;
        options.CreateHandlerChildSpans = true;
        options.CaptureSubscriberMetrics = true;
        options.CaptureNotificationMiddlewareDetails = true;
        
        // Streaming telemetry options
        options.PacketLevelTelemetryEnabled = false;
        options.PacketTelemetryBatchSize = 100;
        
        // Health and security options
        options.EnableHealthChecks = true;
        
        // Add custom sensitive data patterns
        options.SensitiveDataPatterns.Add("custom_pattern");
    });
    config.AddAssembly(typeof(Program).Assembly);
});
```

### Environment-Specific Configuration

```csharp
builder.Services.AddMediator(config =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Development: Capture everything for debugging
        config.WithTelemetry(TelemetryOptions.Development());
    }
    else
    {
        // Production: Optimize for performance
        config.WithTelemetry(TelemetryOptions.Production());
    }
    
    config.AddAssembly(typeof(Program).Assembly);
});
```

### OpenTelemetry Sampling and Filtering

Configure sampling to manage trace volume:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(traceBuilder =>
    {
        traceBuilder
            .AddSource("Blazing.Mediator")
            .AddSource("Blazing.Mediator.Streaming")
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
            .AddSource("Blazing.Mediator")
            .AddSource("Blazing.Mediator.Streaming")
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
            .AddSource("Blazing.Mediator")
            .AddSource("Blazing.Mediator.Streaming")
            .AddConsoleExporter();
    });
```

This creates complete distributed traces from HTTP request through mediator execution, including notification processing with individual handler spans.

## Monitoring Patterns

Effective monitoring with Blazing.Mediator's telemetry involves implementing patterns that provide both technical and business insights into your application's behavior. These patterns help you create comprehensive observability strategies that can identify performance issues, track business outcomes, and provide actionable insights for optimization. The monitoring patterns shown here demonstrate how to leverage the rich telemetry data to build sophisticated monitoring and alerting capabilities.

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

### Notification Performance Monitoring

Monitor notification processing performance:

```csharp
public class NotificationPerformanceMiddleware<TNotification> : INotificationMiddleware<TNotification>
    where TNotification : INotification
{
    public int Order => -100; // Execute early
    
    public async Task Handle(TNotification notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
    {
        using var activity = Activity.Current;
        var stopwatch = Stopwatch.StartNew();
        var handlerCount = 0;
        
        // Add performance tracking tags
        activity?.SetTag("notification.performance_monitoring", true);
        activity?.SetTag("notification.start_time", DateTimeOffset.UtcNow.ToString("O"));
        
        try
        {
            await next();
            
            stopwatch.Stop();
            
            // Classify notification processing performance
            var classification = stopwatch.ElapsedMilliseconds switch
            {
                < 100 => "fast",
                < 500 => "normal",
                < 2000 => "slow",
                _ => "very_slow"
            };
            
            activity?.SetTag("notification.performance_classification", classification);
            activity?.SetTag("notification.total_duration_ms", stopwatch.ElapsedMilliseconds);
            
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                activity?.SetTag("notification.performance_warning", "slow_processing");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("notification.performance_classification", "error");
            activity?.SetTag("notification.total_duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetTag("notification.error_during_processing", true);
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

        // Add business-specific metrics for notifications
        if (request is INotification)
        {
            activity?.SetTag("business.event_published", true);
            activity?.SetTag("business.event_category", DetermineEventCategory(request));
        }

        return response;
    }
}

// Business metrics for notification handlers
public class BusinessAuditNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        using var activity = Activity.Current;
        
        // Add business context to the automatic handler span
        activity?.SetTag("business.event_type", "user_lifecycle");
        activity?.SetTag("business.compliance_required", true);
        activity?.SetTag("business.audit_category", "user_management");
        activity?.SetTag("business.data_retention_years", 7);
        
        await AuditUserCreation(notification);
        
        activity?.SetTag("business.audit_completed", true);
    }
}
```

## Best Practices

1. **Activity Source Registration**: Always register both mediator activity sources for complete coverage
2. **Notification Telemetry**: Enable notification telemetry for comprehensive event-driven architecture monitoring
3. **Child Span Configuration**: Use `CreateHandlerChildSpans = true` to get detailed per-handler visibility
4. **Custom Attributes**: Add meaningful business context through custom tags in handlers
5. **Sampling Strategy**: Use appropriate sampling rates for production to balance observability and performance
6. **Error Tracking**: Let the mediator handle exception recording automatically for both requests and notifications
7. **Performance Impact**: Monitor the overhead of telemetry collection in production
8. **Security**: Avoid including sensitive data (passwords, tokens) in span attributes - use built-in sanitization
9. **Correlation**: Use correlation IDs to track requests across service boundaries
10. **Health Checks**: Enable health check endpoints to monitor telemetry system status
11. **Packet Telemetry**: Enable packet-level telemetry only when detailed streaming analysis is needed
12. **Notification Middleware**: Use notification middleware for cross-cutting concerns like logging, auditing, and performance monitoring
13. **Failure Classification**: Leverage partial vs. total failure metrics to understand notification reliability patterns

## Troubleshooting

### No Traces Appearing

1. Verify OpenTelemetry is enabled: `config.WithTelemetry()`
2. Check activity source registration matches:
    ```csharp
    .AddSource("Blazing.Mediator")
    .AddSource("Blazing.Mediator.Streaming")
    ```
3. Ensure exporters are properly configured and accessible
4. Check sampling configuration isn't filtering out traces

### Incomplete Traces

1. Verify all activity sources are registered
2. Check for exceptions during handler execution
3. Ensure proper async/await patterns in handlers

### Missing Notification Telemetry

1. Ensure notification telemetry is enabled: `CaptureNotificationHandlerDetails = true`
2. Verify child spans are enabled: `CreateHandlerChildSpans = true`
3. Check notification middleware telemetry: `CaptureNotificationMiddlewareDetails = true`
4. Ensure notification handlers implement `INotificationHandler<T>` correctly
5. Verify automatic handlers are registered in DI container
6. Check manual subscribers are properly registered with `mediator.Subscribe<T>(subscriber)`

### Performance Issues

1. Implement sampling to reduce trace volume
2. Use filtering to exclude high-frequency, low-value operations
3. Monitor telemetry collection overhead with health checks
4. Consider disabling packet-level telemetry in production
5. Adjust sensitive data pattern filtering
6. Disable detailed middleware telemetry in production: `CaptureNotificationMiddlewareDetails = false`
7. Optimize child span creation for high-volume notifications

### Missing Streaming Telemetry

1. Ensure `Blazing.Mediator.Streaming` source is registered
2. Verify streaming handlers implement `IAsyncEnumerable<T>` correctly
3. Check for proper cancellation token usage
4. Enable packet-level telemetry if detailed analysis is needed

### Health Check Issues

1. Verify health checks are enabled in telemetry options
2. Check telemetry health endpoint accessibility
3. Monitor health check metrics for system status

### Notification-Specific Issues

1. **Handler Spans Not Created**: Verify `CreateHandlerChildSpans = true` and handlers implement correct interfaces
2. **Missing Subscriber Metrics**: Enable `CaptureSubscriberMetrics = true` for detailed subscriber performance data
3. **Partial Failure Metrics**: Check that handlers are throwing exceptions properly for failure scenarios
4. **Middleware Not Traced**: Ensure `CaptureNotificationMiddlewareDetails = true` and middleware implements `INotificationMiddleware<T>`

## Example: Complete Setup

Here's a complete example with production-ready configuration including comprehensive notification telemetry:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Blazing.Mediator with comprehensive OpenTelemetry
builder.Services.AddMediator(config =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Development: Comprehensive telemetry with detailed information
        config.WithTelemetry(TelemetryOptions.Development());
    }
    else
    {
        // Production: Optimized telemetry configuration
        config.WithTelemetry(TelemetryOptions.Production());
    }
    
    config.AddAssembly(typeof(Program).Assembly);
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

            // Add Blazing.Mediator sources (ESSENTIAL)
            .AddSource("Blazing.Mediator")
            .AddSource("Blazing.Mediator.Streaming")

            // Add other instrumentation
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()

            // Configure sampling based on environment
            .SetSampler(builder.Environment.IsDevelopment() 
                ? new AlwaysOnSampler() 
                : new TraceIdRatioBasedSampler(0.1))

            // Add exporters based on environment
            .AddConsoleExporter(); // Development

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

// Configure notification subscribers (if using manual subscriber pattern)
var mediator = app.Services.GetRequiredService<IMediator>();
var notificationSubscriber = app.Services.GetRequiredService<RealTimeNotificationSubscriber>();
mediator.Subscribe<UserCreatedNotification>(notificationSubscriber);
```

This setup provides comprehensive observability for your Blazing.Mediator application with minimal configuration and production-ready defaults, including full notification telemetry support with individual handler spans, subscriber metrics, and notification middleware instrumentation.