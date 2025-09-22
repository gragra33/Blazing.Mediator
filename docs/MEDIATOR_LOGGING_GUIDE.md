# Mediator Logging Configuration Guide

This guide demonstrates how to use the new mediator logging configuration features in Blazing.Mediator.

## Overview

The new mediator logging system allows you to control exactly which parts of the mediator produce debug logs, giving you fine-grained control over logging verbosity and performance.

## 1. Simplified Configuration

```json
{
    "Logging": {
        "LogLevel": {
            "Blazing.Mediator": "Debug"
        }
    }
}
```

## 2. Mediator Logging Categories

You can now control logging for specific mediator components:

-   **Request Middleware** - Middleware pipeline execution for requests
-   **Notification Middleware** - Middleware pipeline execution for notifications
-   **Send** - Send operation logging (commands/queries)
-   **SendStream** - Streaming operation logging
-   **Publish** - Notification publishing logging
-   **Request Pipeline Resolution** - Pipeline construction for requests
-   **Notification Pipeline Resolution** - Pipeline construction for notifications
-   **Warnings** - Warning messages (missing handlers, etc.)
-   **Query Analyzer** - Query analysis and handler discovery
-   **Command Analyzer** - Command analysis and handler discovery

## 3. Configuration Examples

### Development Configuration (Verbose Logging)

```csharp
services.AddMediator(config =>
{
    config.WithLogging(logging =>
    {
        logging.EnableRequestMiddleware = true;
        logging.EnableNotificationMiddleware = true;
        logging.EnableSend = true;
        logging.EnableSendStream = true;
        logging.EnablePublish = true;
        logging.EnableRequestPipelineResolution = true;
        logging.EnableNotificationPipelineResolution = true;
        logging.EnableWarnings = true;
        logging.EnableQueryAnalyzer = true;
        logging.EnableCommandAnalyzer = true;
        logging.EnableDetailedTypeClassification = true;
        logging.EnableDetailedHandlerInfo = true;
        logging.EnableMiddlewareExecutionOrder = true;
        logging.EnablePerformanceTiming = true;
        logging.EnableSubscriberDetails = true;
    });
});
```

### Production Configuration (Minimal Logging)

```csharp
services.AddMediator(config =>
{
    config.WithLogging(logging =>
    {
        logging.EnableRequestMiddleware = false;
        logging.EnableNotificationMiddleware = false;
        logging.EnableSend = false;
        logging.EnableSendStream = false;
        logging.EnablePublish = false;
        logging.EnableRequestPipelineResolution = false;
        logging.EnableNotificationPipelineResolution = false;
        logging.EnableWarnings = true; // Keep warnings even in production
        logging.EnableQueryAnalyzer = false;
        logging.EnableCommandAnalyzer = false;
    });
});
```

### Preset Configurations

```csharp
// For maximum observability (all features enabled)
config.WithLogging(LoggingOptions.CreateVerbose());

// For minimal overhead (only warnings enabled)
config.WithLogging(LoggingOptions.CreateMinimal());
```

### Environment-Based Configuration

```csharp
services.AddMediator(config =>
{
    if (environment.IsDevelopment())
    {
        config.WithLogging(LoggingOptions.CreateVerbose());
    }
    else
    {
        config.WithLogging(LoggingOptions.CreateMinimal());
    }
});
```

## 4. Advanced Configuration Options

### Performance-Focused Logging

```csharp
config.WithLogging(logging =>
{
    // Only enable essential operations
    logging.EnableSend = true;
    logging.EnablePublish = true;
    logging.EnableWarnings = true;

    // Disable detailed info for performance
    logging.EnableDetailedTypeClassification = false;
    logging.EnableDetailedHandlerInfo = false;
    logging.EnableMiddlewareExecutionOrder = false;
    logging.EnableSubscriberDetails = false;

    // Keep timing for monitoring
    logging.EnablePerformanceTiming = true;
});
```

### Debugging-Focused Logging

```csharp
config.WithLogging(logging =>
{
    // Enable all pipeline and resolution logging
    logging.EnableRequestPipelineResolution = true;
    logging.EnableNotificationPipelineResolution = true;
    logging.EnableRequestMiddleware = true;
    logging.EnableNotificationMiddleware = true;

    // Enable detailed information
    logging.EnableDetailedTypeClassification = true;
    logging.EnableDetailedHandlerInfo = true;
    logging.EnableMiddlewareExecutionOrder = true;

    // Enable analyzers for troubleshooting
    logging.EnableQueryAnalyzer = true;
    logging.EnableCommandAnalyzer = true;
});
```

## 5. Sample Log Output

With mediator logging enabled, you'll see categorized log messages:

```
[DEBUG] [ANALYZER] Starting analysis of queries. Service provider: ServiceProvider
[DEBUG] [ANALYZER] Found 5 query types during analysis. Detailed: True
[DEBUG] [SEND] Starting Send operation for request: GetUserQuery. Telemetry enabled: True
[DEBUG] [SEND] Handler resolution: Looking for IRequestHandler`2 for GetUserQuery
[DEBUG] [SEND] Handler found: GetUserQueryHandler for GetUserQuery
[DEBUG] [MIDDLEWARE] Starting request middleware pipeline for GetUserQuery. Total middleware registered: 3
[DEBUG] [PUBLISH] Starting Publish operation for notification: UserCreatedNotification. Telemetry enabled: True
[DEBUG] [PUBLISH] Subscriber resolution: Found 2 subscribers for UserCreatedNotification
[DEBUG] [PUBLISH] Processing subscriber: EmailNotificationSubscriber for UserCreatedNotification
[DEBUG] [PUBLISH] Subscriber completed: EmailNotificationSubscriber for UserCreatedNotification. Duration: 45.2ms, Success: True
```

## 6. Performance Considerations

-   **Development**: Use verbose logging for full observability
-   **Production**: Use minimal logging to reduce overhead
-   **High-throughput scenarios**: Disable detailed logging and subscriber details
-   **Debugging issues**: Temporarily enable specific categories as needed

## 7. Migration Guide

### Existing Configuration

No changes needed to existing configurations. The new mediator logging is opt-in and backwards compatible.

### Adopting Mediator Logging

1. Update your `appsettings.json` to use `"Blazing.Mediator": "Debug"`
2. Add `config.WithLogging()` to your mediator configuration
3. Customize logging options based on your needs
4. Test in development before deploying to production

The mediator logging system provides the flexibility to balance observability with performance, ensuring you get the right level of detail for each environment.
