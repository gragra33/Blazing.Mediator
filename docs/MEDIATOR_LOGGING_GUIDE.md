# Mediator Logging Configuration Guide

This guide demonstrates how to use the new mediator logging configuration features in Blazing.Mediator.

> [!CAUTION]
> Version 3.0.0 has breaking changes. See the **[Breaking Changes](https://github.com/gragra33/Blazing.Mediator/docs/BREAKING_CHANGES.md)** document for a complete list of API changes with before/after comparisons, and the **[MIGRATION_GUIDE.md](https://github.com/gragra33/Blazing.Mediator/docs/MIGRATION_GUIDE.md)** for full upgrade steps.

## Overview

The new mediator logging system allows you to control exactly which parts of the mediator produce debug logs, giving you fine-grained control over logging verbosity and performance.

## Table of Contents

1. [Simplified Configuration](#1-simplified-configuration)
2. [Mediator Logging Categories](#2-mediator-logging-categories)
3. [Configuration Examples](#3-configuration-examples)
4. [Quick Reference Tables](#quick-reference-tables)
5. [Advanced Configuration Options](#4-advanced-configuration-options)
6. [Sample Log Output](#5-sample-log-output)
7. [Performance Considerations](#6-performance-considerations)
8. [Migration Guide](#7-migration-guide)

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

- **Request Middleware** - Middleware pipeline execution for requests
- **Notification Middleware** - Middleware pipeline execution for notifications
- **Send** - Send operation logging (commands/queries)
- **SendStream** - Streaming operation logging
- **Publish** - Notification publishing logging
- **Request Pipeline Resolution** - Pipeline construction for requests
- **Notification Pipeline Resolution** - Pipeline construction for notifications
- **Warnings** - Warning messages (missing handlers, etc.)
- **Query Analyzer** - Query analysis and handler discovery
- **Command Analyzer** - Command analysis and handler discovery
- **Statistics** - Statistics collection, cleanup, and reporting operations

## 3. Configuration Examples

### Development Configuration (Verbose Logging)

```csharp
// Modern fluent configuration approach (recommended)
builder.Services.AddMediator(config =>
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
        logging.EnableStatistics = true;
        logging.EnableDetailedTypeClassification = true;
        logging.EnableDetailedHandlerInfo = true;
        logging.EnableMiddlewareExecutionOrder = true;
        logging.EnablePerformanceTiming = true;
        logging.EnableSubscriberDetails = true;
        logging.EnableConstraintLogging = true;
        logging.EnableMiddlewareRoutingLogging = true;
    });
});
```

### Production Configuration (Minimal Logging)

```csharp
// Modern fluent configuration approach (recommended)
builder.Services.AddMediator(config =>
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
        logging.EnableStatistics = false;
        logging.EnablePerformanceTiming = false;
        logging.EnableSubscriberDetails = false;
        logging.EnableConstraintLogging = false;
        logging.EnableMiddlewareRoutingLogging = false;
    });
});
```

### Preset Configurations

```csharp
// For maximum observability (all features enabled)
builder.Services.AddMediator(config =>
{
    config.WithLogging(LoggingOptions.CreateVerbose());
});

// For minimal overhead (only warnings enabled)
builder.Services.AddMediator(config =>
{
    config.WithLogging(LoggingOptions.CreateMinimal());
});
```

### Environment-Based Configuration

```csharp
builder.Services.AddMediator(config =>
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

> **v3.0.0**: All handler and middleware discovery is performed at compile time by `Blazing.Mediator.SourceGenerators`. `AddAssembly()`, `WithMiddlewareDiscovery()`, and `discoverMiddleware: true` are no longer required.

## Quick Reference Tables

### Logging Configuration Options

Understanding the available logging options helps you configure the right level of detail for different environments and use cases. Each option provides specific insights into mediator behavior and performance.

| **Logging Option**                     | **Purpose**                                    | **Default** | **Environment** | **Example**                            |
| -------------------------------------- | ---------------------------------------------- | ----------- | --------------- | -------------------------------------- |
| `EnableRequestMiddleware`              | Log request middleware pipeline execution      | `true`      | Development     | Pipeline order, execution timing       |
| `EnableNotificationMiddleware`         | Log notification middleware pipeline execution | `true`      | Development     | Middleware processing flow             |
| `EnableSend`                           | Log Send operation details                     | `true`      | Development     | Command/query execution                |
| `EnableSendStream`                     | Log streaming operation details                | `true`      | Development     | Stream processing metrics              |
| `EnablePublish`                        | Log notification publishing details            | `true`      | Development     | Notification delivery tracking         |
| `EnableRequestPipelineResolution`      | Log pipeline construction for requests         | `true`      | Debug           | Handler and middleware discovery       |
| `EnableNotificationPipelineResolution` | Log pipeline construction for notifications    | `true`      | Debug           | Subscriber and handler registration    |
| `EnableWarnings`                       | Log warning messages                           | `true`      | All             | Missing handlers, configuration issues |
| `EnableQueryAnalyzer`                  | Log query analysis and discovery               | `true`      | Development     | Query type discovery                   |
| `EnableCommandAnalyzer`                | Log command analysis and discovery             | `true`      | Development     | Command type discovery                 |
| `EnableStatistics`                     | Log statistics collection and reporting        | `true`      | Development     | Stats collection, cleanup, reporting   |

### Detailed Logging Features

Advanced logging features provide comprehensive insights into mediator behavior, performance characteristics, and internal processing details for debugging and monitoring purposes.

| **Feature**                        | **Purpose**                                                     | **Default** | **Performance Impact** | **Use Case**                              |
| ---------------------------------- | --------------------------------------------------------------- | ----------- | ---------------------- | ----------------------------------------- |
| `EnableDetailedTypeClassification` | Log detailed type information                                   | `false`     | Low                    | Understanding type hierarchy              |
| `EnableDetailedHandlerInfo`        | Log comprehensive handler details                               | `false`     | Low                    | Handler registration debugging            |
| `EnableMiddlewareExecutionOrder`   | Log middleware execution sequence                               | `false`     | Low                    | Pipeline ordering verification            |
| `EnablePerformanceTiming`          | Log operation duration metrics                                  | `true`      | Medium                 | Performance monitoring                    |
| `EnableSubscriberDetails`          | Log notification subscriber information                         | `true`      | Low                    | Subscription management debugging         |
| `EnableConstraintLogging`          | Log constraint validation and middleware skipping decisions     | `false`     | High                   | Type-constrained notification middleware  |
| `EnableMiddlewareRoutingLogging`   | Log middleware execution decisions and constraint-based routing | `false`     | High                   | Pipeline flow and routing troubleshooting |

### Environment-Based Configurations

Choose the appropriate logging configuration based on your deployment environment to balance observability needs with performance requirements.

| **Environment**                 | **Configuration**                            | **Enabled Features**                         | **Purpose**                                        |
| ------------------------------- | -------------------------------------------- | -------------------------------------------- | -------------------------------------------------- |
| **Development**                 | `LoggingOptions.CreateVerbose()`             | All logging features enabled                 | Complete observability for debugging               |
| **Staging**                     | Custom configuration                         | Send, Publish, Warnings, Performance         | Balanced monitoring without overhead               |
| **Production**                  | `LoggingOptions.CreateMinimal()`             | Warnings only                                | Minimal impact, essential alerts only              |
| **Debug/Troubleshooting**       | All analyzers enabled                        | Query/Command analyzers, Pipeline resolution | Maximum detail for issue diagnosis                 |
| **Constraint Middleware Debug** | `LoggingOptions.CreateConstraintDebugging()` | Constraint logging, middleware routing       | Debugging type-constrained notification middleware |

### Log Level Mapping

Understanding how mediator logging maps to standard .NET log levels helps you configure appropriate log filtering and routing for different scenarios.

| **Mediator Operation**     | **Log Level** | **Log Content**                        | **When to Enable**           |
| -------------------------- | ------------- | -------------------------------------- | ---------------------------- |
| Request/Command Processing | `Information` | Handler execution, timing              | Development, staging         |
| Notification Publishing    | `Information` | Subscriber count, delivery status      | Development, staging         |
| Middleware Execution       | `Debug`       | Pipeline flow, execution order         | Debugging issues             |
| Pipeline Resolution        | `Debug`       | Handler/middleware discovery           | Troubleshooting registration |
| Performance Metrics        | `Information` | Execution duration, throughput         | Performance monitoring       |
| Warning Messages           | `Warning`     | Missing handlers, configuration issues | All environments             |
| Error Conditions           | `Error`       | Handler failures, pipeline errors      | All environments             |

### Configuration Methods

Multiple configuration approaches provide flexibility for different application structures and deployment scenarios, from simple boolean flags to complex fluent configurations.

| **Configuration Method**  | **Syntax**                             | **Use Case**              | **Example**                              |
| ------------------------- | -------------------------------------- | ------------------------- | ---------------------------------------- |
| **Fluent Configuration**  | `config.WithLogging(options => {...})` | Complete control          | Custom per-feature configuration         |
| **Preset Configurations** | `LoggingOptions.CreateVerbose()`       | Quick setup               | Standard development/production patterns |
| **Environment-Based**     | Conditional configuration              | Different per environment | `if (env.IsDevelopment())` patterns      |
| **JSON Configuration**    | `appsettings.json` integration         | External configuration    | Configuration file-based settings        |
| **Runtime Configuration** | Dynamic option changes                 | Live reconfiguration      | Hot-reload logging settings              |

### Performance Impact Guide

Understanding the performance implications of different logging options helps you make informed decisions about what to enable in production environments.

| **Logging Category**        | **CPU Impact** | **Memory Impact** | **I/O Impact** | **Recommendation**             |
| --------------------------- | -------------- | ----------------- | -------------- | ------------------------------ |
| **Send/Publish Operations** | Low            | Low               | Medium         | Enable in development, staging |
| **Middleware Pipeline**     | Low            | Low               | High           | Enable only when debugging     |
| **Pipeline Resolution**     | Medium         | Medium            | High           | Disable in production          |
| **Performance Timing**      | Low            | Low               | Medium         | Enable for monitoring          |
| **Type Classification**     | Medium         | Low               | High           | Enable only when needed        |
| **Subscriber Details**      | Low            | Medium            | Medium         | Enable in development only     |
| **Warnings Only**           | Minimal        | Minimal           | Low            | Always safe for production     |

## 4. Advanced Configuration Options

### Performance-Focused Logging

```csharp
builder.Services.AddMediator(config =>
{
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
});
```

### Debugging-Focused Logging

```csharp
builder.Services.AddMediator(config =>
{
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
});
```

### Combined with Other Features

```csharp
builder.Services.AddMediator(config =>
{
    config.WithLogging(LoggingOptions.CreateVerbose())
          .WithStatisticsTracking();
});
```

> **v3.0.0**: `WithMiddlewareDiscovery()`, `WithNotificationHandlerDiscovery()`, and `AddAssembly()` are no longer needed. The source generator discovers all handlers and middleware at compile time.

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

- **Development**: Use verbose logging for full observability
- **Production**: Use minimal logging to reduce overhead
- **High-throughput scenarios**: Disable detailed logging and subscriber details
- **Debugging issues**: Temporarily enable specific categories as needed

## 7. Migration Guide

### Existing Configuration

No changes needed to existing configurations. The new mediator logging is opt-in and backwards compatible.

### Adopting Mediator Logging

1. Update your `appsettings.json` to use `"Blazing.Mediator": "Debug"`
2. Adopt the fluent configuration approach:

```csharp
builder.Services.AddMediator(config =>
{
    config.WithLogging(LoggingOptions.CreateVerbose());
});
```

3. Customize logging options based on your needs
4. Test in development before deploying to production

### Complete Migration Example

```csharp
// v3.0.0 — source generator discovers all middleware; only configure runtime options
builder.Services.AddMediator(config =>
{
    config.WithLogging(logging =>
    {
        logging.EnableSend = true;
        logging.EnablePublish = true;
        logging.EnableWarnings = true;
        logging.EnablePerformanceTiming = true;
    });
    // Middleware is auto-discovered by Blazing.Mediator.SourceGenerators
});
```

The mediator logging system provides the flexibility to balance observability with performance, ensuring you get the right level of detail for each environment.

> **v3.0.0**: The source generator replaces runtime reflection and removes the need for `WithMiddlewareDiscovery()`, `AddAssembly()`, or `discoverMiddleware: true` parameters. All middleware is discovered at compile time.
