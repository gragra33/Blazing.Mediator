# Mediator Statistics Configuration Guide

## Table of Contents

1. [Overview](#overview)
2. [Quick Reference Tables](#quick-reference-tables)
   - [StatisticsOptions Configuration Properties](#statisticsoptions-configuration-properties)
   - [StatisticsOptions Preset Configurations](#statisticsoptions-preset-configurations)
   - [Query & Command Analysis Properties (QueryCommandAnalysis)](#query--command-analysis-properties-querycommandanalysis)
   - [Notification Analysis Properties (NotificationAnalysis)](#notification-analysis-properties-notificationanalysis)
   - [Handler & Subscriber Status Enums](#handler--subscriber-status-enums)
   - [Performance Metrics Properties (PerformanceMetrics)](#performance-metrics-properties-performancemetrics)
   - [Performance Summary Properties (PerformanceSummary)](#performance-summary-properties-performancesummary)
   - [Middleware Analysis Properties (MiddlewareAnalysis)](#middleware-analysis-properties-middlewareanalysis)
   - [Statistics Tracking Methods](#statistics-tracking-methods)
3. [Basic Statistics Configuration](#1-basic-statistics-configuration)
   - [Default Configuration](#default-configuration)
   - [Custom Configuration](#custom-configuration)
4. [Statistics Configuration Levels](#2-statistics-configuration-levels)
   - [Development Configuration](#development-configuration)
   - [Production Configuration](#production-configuration)
   - [High-Performance Configuration](#high-performance-configuration)
   - [Custom High-Observability Configuration](#custom-high-observability-configuration)
5. [Statistics Features by Configuration Level](#3-statistics-features-by-configuration-level)
   - [Request Metrics (`EnableRequestMetrics = true`)](#request-metrics-enablerequestmetrics--true)
   - [Notification Metrics (`EnableNotificationMetrics = true`)](#notification-metrics-enablenotificationmetrics--true)
   - [Middleware Metrics (`EnableMiddlewareMetrics = true`)](#middleware-metrics-enablemiddlewaremetrics--true)
   - [Performance Counters (`EnablePerformanceCounters = true`)](#performance-counters-enableperformancecounters--true)
   - [Detailed Analysis (`EnableDetailedAnalysis = true`)](#detailed-analysis-enabledetailedanalysis--true)
6. [Using Statistics in Your Application](#4-using-statistics-in-your-application)
   - [Basic Statistics Reporting](#basic-statistics-reporting)
   - [Health Check Integration](#health-check-integration)
   - [Performance Monitoring](#performance-monitoring)
7. [Real-Time Session-Based Statistics](#5-real-time-session-based-statistics)
   - [Session Tracking Setup](#session-tracking-setup)
   - [Session Statistics Middleware](#session-statistics-middleware)
   - [Request-Level Statistics Tracking](#request-level-statistics-tracking)
8. [Advanced Analysis and Insights](#6-advanced-analysis-and-insights)
   - [Query/Command Analysis](#querycommand-analysis)
   - [Performance Insights](#performance-insights)
9. [API Endpoints for Statistics](#7-api-endpoints-for-statistics)
   - [Statistics Dashboard Endpoints (Sample Implementation)](#statistics-dashboard-endpoints-sample-implementation)
10. [Statistics Configuration Best Practices](#8-statistics-configuration-best-practices)
    - [Performance Considerations](#performance-considerations)
    - [Monitoring Setup](#monitoring-setup)
    - [Integration with Application Metrics](#integration-with-application-metrics)
11. [Complete Setup Example](#9-complete-setup-example)
    - [Production-Ready Statistics Configuration](#production-ready-statistics-configuration)
12. [Troubleshooting Statistics](#10-troubleshooting-statistics)
    - [Common Issues](#common-issues)
    - [Diagnostic Endpoints](#diagnostic-endpoints)

---

This guide demonstrates how to use the comprehensive statistics tracking capabilities in Blazing.Mediator to monitor, analyze, and optimize your application's performance and usage patterns.

## Overview

The Blazing.Mediator statistics system provides detailed insights into your application's mediator usage with configurable tracking levels, real-time monitoring, and comprehensive analysis capabilities.

## Quick Reference Tables

The following quick reference tables provide comprehensive information about all the statistics features, configuration options, and data structures available in Blazing.Mediator. These tables are designed to give you immediate access to the most important configuration parameters and analysis properties without having to search through the detailed documentation. Use these tables to quickly understand what data is available, how to configure statistics tracking for your specific needs, and what each property means in the context of your application monitoring and analysis.

### StatisticsOptions Configuration Properties

The `StatisticsOptions` class controls all aspects of statistics collection in Blazing.Mediator. Each property enables different levels of monitoring, from basic request counting to advanced performance analytics. Understanding these properties is crucial for configuring the appropriate level of observability for your application while maintaining optimal performance. The default values are carefully chosen to provide basic tracking with minimal overhead, but you can adjust them based on your monitoring needs and performance requirements.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `EnableRequestMetrics` | `bool` | `true` | Track queries and commands execution counts |
| `EnableNotificationMetrics` | `bool` | `true` | Track notification publication counts |
| `EnableMiddlewareMetrics` | `bool` | `false` | Track middleware execution times and success rates |
| `EnablePerformanceCounters` | `bool` | `false` | Advanced metrics: percentiles, memory allocation, timing |
| `EnableDetailedAnalysis` | `bool` | `false` | Comprehensive analysis with all properties populated |
| `MetricsRetentionPeriod` | `TimeSpan` | `TimeSpan.Zero` | How long to keep metrics data (0 = indefinite) |
| `CleanupInterval` | `TimeSpan` | `TimeSpan.FromHours(1)` | Frequency of automatic cleanup |
| `MaxTrackedRequestTypes` | `int` | `0` | Maximum number of request types to track (0 = unlimited) |

### StatisticsOptions Preset Configurations

Blazing.Mediator provides several preset configurations that are optimized for different environments and use cases. These presets represent battle-tested combinations of settings that balance observability needs with performance considerations. The Development preset maximizes visibility for debugging and troubleshooting, while the Production preset focuses on essential metrics with minimal performance impact. The Disabled preset turns off all tracking for maximum performance in scenarios where statistics are not needed.

| Configuration | Request | Notification | Middleware | Performance | Detailed | Retention | Cleanup |
|---------------|---------|-------------|------------|-------------|----------|-----------|---------|
| `StatisticsOptions.Development()` | ? | ? | ? | ? | ? | 1 hour | 15 min |
| `StatisticsOptions.Production()` | ? | ? | ? | ? | ? | 24 hours | 4 hours |
| `StatisticsOptions.Disabled()` | ? | ? | ? | ? | ? | 0 | Never |
| Custom High-Observability | ? | ? | ? | ? | ? | 7 days | 2 hours |

### Query & Command Analysis Properties (QueryCommandAnalysis)

The `QueryCommandAnalysis` record provides comprehensive information about each query and command type discovered in your application. This analysis is essential for understanding your application's CQRS structure, verifying handler registration, and identifying potential architectural issues. The analysis can be run in detailed or compact mode, with detailed mode providing additional properties for thorough investigation and compact mode focusing on essential information for quick overviews. The handler status information is particularly valuable for detecting missing or duplicate handlers during development and deployment verification.

| Property | Type | Description | Detailed Mode | Compact Mode |
|----------|------|-------------|---------------|--------------|
| `Type` | `Type` | Actual .NET Type being analyzed | ? | ? |
| `ClassName` | `string` | Clean class name without generic parameters | ? | ? |
| `TypeParameters` | `string` | String representation of generic parameters | ? | ? |
| `Assembly` | `string` | Assembly name containing the type | ? | ? |
| `Namespace` | `string` | Namespace of the type | ? | ? |
| `ResponseType` | `Type?` | Response type for queries/commands with return values | ? | ? |
| `PrimaryInterface` | `string` | Primary interface (IQuery<T>, ICommand, etc.) | ? | ? |
| `IsResultType` | `bool` | True if response implements IResult interface | ? | ? |
| `HandlerStatus` | `HandlerStatus` | Handler registration status | ? | ? |
| `HandlerDetails` | `string` | Detailed handler information | Full details | Simplified |
| `Handlers` | `IReadOnlyList<Type>` | List of registered handler types | ? | ? |

### Notification Analysis Properties (NotificationAnalysis)

The `NotificationAnalysis` record extends the analysis capabilities to include notification types, providing insights into both automatic handlers (implementing `INotificationHandler<T>`) and manual subscribers (registered via `IMediator.Subscribe()`). This dual tracking approach recognizes that notifications in Blazing.Mediator can be processed through two different patterns, and both are important for understanding the complete notification processing pipeline. The subscriber status and estimation provide visibility into dynamic subscription patterns that may not be apparent from static analysis alone.

| Property | Type | Description | Detailed Mode | Compact Mode |
|----------|------|-------------|---------------|--------------|
| `Type` | `Type` | Actual .NET Type being analyzed | ? | ? |
| `ClassName` | `string` | Clean class name without generic parameters | ? | ? |
| `TypeParameters` | `string` | String representation of generic parameters | ? | ? |
| `Assembly` | `string` | Assembly name containing the type | ? | ? |
| `Namespace` | `string` | Namespace of the type | ? | ? |
| `PrimaryInterface` | `string` | Primary interface (INotification) | ? | ? |
| `HandlerStatus` | `HandlerStatus` | Automatic handler registration status | ? | ? |
| `HandlerDetails` | `string` | Detailed handler information | Full details | Simplified |
| `Handlers` | `IReadOnlyList<Type>` | List of automatic handler types | ? | ? |
| `SubscriberStatus` | `SubscriberStatus` | Manual subscriber registration status | ? | ? |
| `SubscriberDetails` | `string` | Detailed subscriber information | Full details | Simplified |
| `EstimatedSubscribers` | `int` | Estimated number of manual subscribers | ? | ? |

### Handler & Subscriber Status Enums

#### HandlerStatus Enum

| Value | ASCII Marker | Description | Usage |
|-------|-------------|-------------|-------|
| `Single` | `+` | Exactly one handler registered | ? Ideal state |
| `Missing` | `!` | No handler registered | ? Needs attention |
| `Multiple` | `#` | Multiple handlers registered | ?? Potential issue |

#### SubscriberStatus Enum  

| Value | ASCII Marker | Description | Usage |
|-------|-------------|-------------|-------|
| `Present` | `+` | Subscribers are registered | ? Working |
| `None` | `!` | No subscribers found | ?? May need attention |
| `Unknown` | `?` | Cannot determine status | ? Check configuration |

### Performance Metrics Properties (PerformanceMetrics)

The `PerformanceMetrics` record provides detailed performance analytics for individual operation types when performance counters are enabled. This includes not only basic execution counts and timing information but also statistical measures like percentiles that help identify performance outliers and establish service level objectives. The percentile metrics (P50, P95, P99) are particularly valuable for understanding the distribution of response times and identifying when performance is acceptable for most users versus experiencing significant degradation for a small percentage of requests.

| Property | Type | Description | Available When |
|----------|------|-------------|----------------|
| `OperationType` | `string` | Name of the operation type | `EnablePerformanceCounters = true` |
| `TotalExecutions` | `long` | Total number of executions | `EnablePerformanceCounters = true` |
| `FailedExecutions` | `long` | Number of failed executions | `EnablePerformanceCounters = true` |
| `AverageTimeMs` | `double` | Average execution time in milliseconds | `EnablePerformanceCounters = true` |
| `SuccessRate` | `double` | Success rate as percentage | `EnablePerformanceCounters = true` |
| `LastExecution` | `DateTime` | Timestamp of last execution | `EnablePerformanceCounters = true` |
| `P50` | `double` | 50th percentile execution time | `EnablePerformanceCounters = true` |
| `P95` | `double` | 95th percentile execution time | `EnablePerformanceCounters = true` |
| `P99` | `double` | 99th percentile execution time | `EnablePerformanceCounters = true` |

### Performance Summary Properties (PerformanceSummary)

The `PerformanceSummary` record aggregates performance metrics across all operations or specific categories (requests vs notifications), providing a high-level view of system performance. This summary is essential for monitoring overall system health, establishing performance baselines, and detecting when performance degrades across the entire application rather than just individual operations. The ability to get separate summaries for requests and notifications allows you to understand how different parts of your CQRS architecture are performing and optimize them independently.

| Property | Type | Description | Scope |
|----------|------|-------------|-------|
| `TotalOperations` | `long` | Total number of operations | Overall, Request, or Notification |
| `TotalFailures` | `long` | Total number of failed operations | Overall, Request, or Notification |
| `AverageExecutionTimeMs` | `double` | Average execution time in milliseconds | Overall, Request, or Notification |
| `OverallSuccessRate` | `double` | Success rate as percentage | Overall, Request, or Notification |
| `TotalMemoryAllocatedBytes` | `long` | Total memory allocated in bytes | Overall (shared between types) |
| `UniqueOperationTypes` | `int` | Number of unique operation types | Overall, Request, or Notification |

### Middleware Analysis Properties (MiddlewareAnalysis)

The `MiddlewareAnalysis` record provides detailed information about middleware components in the request processing pipeline. This analysis is crucial for understanding middleware execution order, verifying that middleware is properly configured, and debugging pipeline issues. The order information helps ensure that middleware executes in the intended sequence, while the configuration data provides insight into how each middleware component is set up. This is particularly valuable when using auto-discovery features or complex middleware configurations where the exact setup might not be immediately apparent.

| Property | Type | Description | Notes |
|----------|------|-------------|-------|
| `Type` | `Type` | Actual .NET Type of the middleware | Available via `IMiddlewarePipelineInspector` |
| `ClassName` | `string` | Clean class name without generic parameters | Extracted from type name |
| `TypeParameters` | `string` | String representation of generic parameters | For generic middleware types |
| `Order` | `int` | Execution order of the middleware | Lower values execute first |
| `OrderDisplay` | `string` | Formatted display of order value | Handles special values |
| `Configuration` | `object?` | Configuration object for the middleware | May be null |

### Statistics Tracking Methods

The statistics tracking methods provide the core functionality for collecting runtime metrics and performing analysis of your mediator implementation. These methods are organized into three main categories: core tracking methods that collect basic metrics automatically, analysis methods that examine your application structure and handler registration, and performance methods that provide detailed timing and throughput metrics. Understanding these methods is essential for implementing custom monitoring solutions and integrating with external monitoring systems.

#### MediatorStatistics Core Methods

The core methods handle the fundamental statistics collection that occurs automatically as your application processes requests and notifications. These methods are called internally by the mediator infrastructure and provide the foundational data for all other statistical analysis. The recording methods require specific configuration options to be enabled, allowing you to control the performance impact and level of detail based on your monitoring needs.

| Method | Description | Triggered When | Requires |
|--------|-------------|----------------|----------|
| `IncrementQuery(string)` | Increments query execution count | Query is sent via mediator | `EnableRequestMetrics = true` |
| `IncrementCommand(string)` | Increments command execution count | Command is sent via mediator | `EnableRequestMetrics = true` |
| `IncrementNotification(string)` | Increments notification count | Notification is published | `EnableNotificationMetrics = true` |
| `RecordExecutionTime(string, long, bool)` | Records execution timing | Request completes | `EnablePerformanceCounters = true` |
| `RecordMemoryAllocation(long)` | Records memory usage | Memory is allocated | `EnablePerformanceCounters = true` |
| `RecordMiddlewareExecution(string, long, bool)` | Records middleware metrics | Middleware executes | `EnableMiddlewareMetrics = true` |
| `ReportStatistics()` | Outputs current statistics | Called manually | Always available |

#### Analysis Methods

The analysis methods examine your application's structure to discover queries, commands, and notifications, then determine their handler registration status and interface patterns. These methods are invaluable for verifying that your CQRS implementation is correctly configured, identifying missing handlers, and understanding your application's architectural patterns. The detailed analysis mode provides comprehensive information suitable for documentation generation and thorough architectural reviews.

| Method | Return Type | Description | Parameters |
|--------|------------|-------------|------------|
| `AnalyzeQueries(IServiceProvider, bool?)` | `IReadOnlyList<QueryCommandAnalysis>` | Analyzes all query types | `serviceProvider`, `isDetailed` (optional) |
| `AnalyzeCommands(IServiceProvider, bool?)` | `IReadOnlyList<QueryCommandAnalysis>` | Analyzes all command types | `serviceProvider`, `isDetailed` (optional) |
| `AnalyzeNotifications(IServiceProvider, bool?)` | `IReadOnlyList<NotificationAnalysis>` | Analyzes all notification types | `serviceProvider`, `isDetailed` (optional) |

#### Performance Methods

The performance methods provide access to detailed timing, throughput, and reliability metrics when performance counters are enabled. These methods are essential for monitoring application performance, establishing service level objectives, and identifying performance bottlenecks. The summary methods provide aggregated views that are suitable for dashboards and alerting, while the specific operation metrics allow for detailed investigation of individual performance issues.

| Method | Return Type | Description | Requires |
|--------|------------|-------------|----------|
| `GetPerformanceMetrics(string)` | `PerformanceMetrics?` | Gets metrics for specific operation | `EnablePerformanceCounters = true` |
| `GetPerformanceSummary()` | `PerformanceSummary?` | Gets overall performance summary | `EnablePerformanceCounters = true` |
| `GetRequestPerformanceSummary()` | `PerformanceSummary?` | Gets request-only performance summary | `EnablePerformanceCounters = true` |
| `GetNotificationPerformanceSummary()` | `PerformanceSummary?` | Gets notification-only performance summary | `EnablePerformanceCounters = true` |

## 1. Basic Statistics Configuration

The basic statistics configuration provides a simple entry point for enabling monitoring in your Blazing.Mediator application. This configuration is designed to be non-intrusive and provide essential insights without requiring extensive setup or configuration. The default configuration enables the most commonly needed statistics while maintaining optimal performance characteristics, making it suitable for most applications that want to add basic monitoring capabilities.

### Default Configuration

The default configuration enables basic statistics tracking with minimal setup. This is the recommended starting point for most applications, providing essential request and notification counting without performance impact. The default settings are carefully chosen to provide valuable insights while maintaining the high performance characteristics that Blazing.Mediator is known for.

```csharp
services.AddMediator(config =>
{
    // Enable basic statistics tracking
    config.WithStatisticsTracking();
}, typeof(Program).Assembly);
```

### Custom Configuration

Custom configuration allows you to fine-tune statistics collection to match your specific monitoring needs and performance requirements. This approach gives you complete control over what data is collected, how long it's retained, and how frequently cleanup operations occur. The configuration options are designed to be composable, allowing you to enable only the features you need while maintaining optimal performance for your use case.

```csharp
services.AddMediator(config =>
{
    config.WithStatisticsTracking(options =>
    {
        options.EnableRequestMetrics = true;        // Track queries and commands
        options.EnableNotificationMetrics = true;   // Track notifications
        options.EnableMiddlewareMetrics = false;    // Disabled by default for performance
        options.EnablePerformanceCounters = false;  // Advanced metrics disabled by default
        options.EnableDetailedAnalysis = false;     // Comprehensive analysis disabled
        
        // Retention settings
        options.MetricsRetentionPeriod = TimeSpan.FromHours(24);
        options.CleanupInterval = TimeSpan.FromHours(1);
        options.MaxTrackedRequestTypes = 1000;
    });
}, typeof(Program).Assembly);
```

## 2. Statistics Configuration Levels

Blazing.Mediator provides several pre-configured statistics levels that are optimized for different environments and use cases. These configuration levels represent best practices for different scenarios, from development environments where maximum observability is desired to production environments where performance and resource usage are critical considerations. Each level is designed to provide the right balance of monitoring capability and system impact for its intended use case.

### Development Configuration

Comprehensive tracking for full observability during development:

```csharp
services.AddMediator(config =>
{
    config.WithStatisticsTracking(StatisticsOptions.Development());
    // Enables:
    // - Request metrics: true
    // - Notification metrics: true 
    // - Middleware metrics: true
    // - Performance counters: false (too much overhead for development)
    // - Detailed analysis: true
    // - Retention: 1 hour
    // - Cleanup: every 15 minutes
}, typeof(Program).Assembly);
```

### Production Configuration

Essential tracking with minimal performance impact:

```csharp
services.AddMediator(config =>
{
    config.WithStatisticsTracking(StatisticsOptions.Production());
    // Enables:
    // - Request metrics: true
    // - Notification metrics: true
    // - Middleware metrics: false (performance)
    // - Performance counters: false (performance)
    // - Detailed analysis: false (memory)
    // - Retention: 24 hours
    // - Cleanup: every 4 hours
}, typeof(Program).Assembly);
```

### High-Performance Configuration

Minimal tracking for performance-critical applications:

```csharp
services.AddMediator(config =>
{
    config.WithStatisticsTracking(StatisticsOptions.Disabled());
    // All tracking disabled for maximum performance
}, typeof(Program).Assembly);
```

### Custom High-Observability Configuration

Maximum insights with all features enabled:

```csharp
services.AddMediator(config =>
{
    config.WithStatisticsTracking(options =>
    {
        // Enable all tracking features
        options.EnableRequestMetrics = true;
        options.EnableNotificationMetrics = true;
        options.EnableMiddlewareMetrics = true;
        options.EnablePerformanceCounters = true;
        options.EnableDetailedAnalysis = true;
        
        // Extended retention for analysis
        options.MetricsRetentionPeriod = TimeSpan.FromDays(7);
        options.CleanupInterval = TimeSpan.FromHours(2);
        options.MaxTrackedRequestTypes = 5000; // Higher limit for large applications
    });
}, typeof(Program).Assembly);
```

## 3. Statistics Features by Configuration Level

The statistics features in Blazing.Mediator are designed to be modular and configurable, allowing you to enable only the level of monitoring that makes sense for your application and environment. Each feature builds upon the previous ones, providing increasingly detailed insights while potentially impacting performance. Understanding these features and their trade-offs is crucial for implementing an effective monitoring strategy that provides the insights you need without compromising application performance.

### Request Metrics (`EnableRequestMetrics = true`)

Basic tracking of all mediator requests:

```csharp
// Tracks execution counts for:
// - Queries: GetUserQuery, GetProductsQuery, etc.
// - Commands: CreateOrderCommand, UpdateUserCommand, etc.
// - Stream requests: GetDataStreamQuery, etc.

var statistics = serviceProvider.GetService<MediatorStatistics>();
statistics?.ReportStatistics();
// Output:
// Mediator Statistics:
// Queries: 1,245
// Commands: 892
// Notifications: 3,421
```

### Notification Metrics (`EnableNotificationMetrics = true`)

Detailed notification processing tracking:

```csharp
// Tracks:
// - Publication counts by type
// - Handler execution success/failure
// - Subscriber processing metrics
// - Cross-pattern compatibility (both automatic handlers and manual subscribers)

var statistics = serviceProvider.GetService<MediatorStatistics>();
var notifications = statistics?.AnalyzeNotifications(serviceProvider);
// Returns analysis of all notification handlers and subscribers
```

### Middleware Metrics (`EnableMiddlewareMetrics = true`)

Pipeline performance monitoring:

```csharp
// Tracks:
// - Individual middleware execution times
// - Pipeline stage performance
// - Middleware success/failure rates
// - Memory allocation in middleware

statistics?.RecordMiddlewareExecution("ValidationMiddleware", 12, true);
statistics?.RecordMiddlewareExecution("LoggingMiddleware", 3, true);
```

### Performance Counters (`EnablePerformanceCounters = true`)

Advanced performance analytics:

```csharp
// Provides:
// - Execution time percentiles (P50, P95, P99)
// - Memory allocation tracking
// - Throughput measurements
// - Success/failure rates
// - Last execution timestamps

var metrics = statistics?.GetPerformanceMetrics("GetUserQuery");
// Returns: PerformanceMetrics with detailed timing data

var summary = statistics?.GetPerformanceSummary();
// Returns: Overall system performance summary
```

### Detailed Analysis (`EnableDetailedAnalysis = true`)

Comprehensive insights and patterns:

```csharp
// Enables:
// - Query/Command type analysis with handler detection
// - Interface pattern detection (custom domain interfaces)
// - Assembly and namespace organization analysis
// - Handler status verification (Missing/Single/Multiple)
// - ASP.NET Core IResult type detection

var queries = statistics?.AnalyzeQueries(serviceProvider, isDetailed: true);
var commands = statistics?.AnalyzeCommands(serviceProvider, isDetailed: true);
// Returns comprehensive analysis with all properties populated
```

## 4. Using Statistics in Your Application

Integrating statistics into your application involves more than just enabling the collection of metrics—it requires thoughtful implementation of monitoring, reporting, and alerting systems that provide actionable insights. The statistics system in Blazing.Mediator is designed to integrate seamlessly with existing monitoring infrastructure while also providing standalone capabilities for applications that need self-contained monitoring solutions. The key is to implement statistics usage in a way that provides value to both development teams for debugging and operations teams for production monitoring.

### Basic Statistics Reporting

Simple reporting for monitoring:

```csharp
public class StatsReportingService
{
    private readonly MediatorStatistics _statistics;

    public StatsReportingService(MediatorStatistics statistics)
    {
        _statistics = statistics;
    }

    public void LogCurrentStats()
    {
        _statistics.ReportStatistics();
    }
}
```

### Health Check Integration

Monitor mediator health using statistics (without MediatorStatsHealthCheck):

```csharp
services.AddHealthChecks()
    .AddCheck<MediatorapplicationHealthCheck>("mediator-health");

public class MediatorapplicationHealthCheck : IHealthCheck
{
    private readonly MediatorStatistics? _statistics;

    public MediatorapplicationHealthCheck(IServiceProvider serviceProvider)
    {
        // Statistics may be null if not enabled
        _statistics = serviceProvider.GetService<MediatorStatistics>();
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_statistics == null)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Statistics tracking disabled"));
        }

        var summary = _statistics.GetPerformanceSummary();
        if (summary?.OverallSuccessRate < 95.0)
        {
            return Task.FromResult(HealthCheckResult.Degraded($"Low success rate: {summary.OverallSuccessRate:F1}%"));
        }

        return Task.FromResult(HealthCheckResult.Healthy("Mediator performance is healthy"));
    }
}
```

### Performance Monitoring

Track performance metrics over time:

```csharp
public class PerformanceMonitoringService
{
    private readonly MediatorStatistics? _statistics;
    private readonly ILogger<PerformanceMonitoringService> _logger;

    public PerformanceMonitoringService(IServiceProvider serviceProvider, ILogger<PerformanceMonitoringService> logger)
    {
        _statistics = serviceProvider.GetService<MediatorStatistics>();
        _logger = logger;
    }

    public void MonitorPerformance()
    {
        if (_statistics == null)
        {
            _logger.LogInformation("Statistics tracking is disabled");
            return;
        }

        var summary = _statistics.GetPerformanceSummary();
        if (summary != null)
        {
            _logger.LogInformation("Performance Summary: {Operations} ops, {SuccessRate:F1}% success, {AvgTime:F1}ms avg", 
                summary.Value.TotalOperations, summary.Value.OverallSuccessRate, summary.Value.AverageExecutionTimeMs);

            if (summary.Value.AverageExecutionTimeMs > 100)
            {
                _logger.LogWarning("Average execution time is high: {AvgTime:F1}ms", summary.Value.AverageExecutionTimeMs);
            }
        }
    }
}
```

## 5. Real-Time Session-Based Statistics

The sample applications demonstrate session-based statistics tracking using custom `MediatorStatisticsTracker` services. These are application-level implementations and are not part of the core Blazing.Mediator library.

### Session Tracking Setup

Enable session-based statistics for user activity tracking:

```csharp
// Program.cs
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MediatorStatisticsTracker>(); // Custom application service
builder.Services.AddHostedService<StatisticsCleanupService>(); // Custom application service

var app = builder.Build();
app.UseSession();
```

### Session Statistics Middleware

Track mediator usage per user session:

```csharp
public class SessionTrackingMiddleware
{
    private readonly RequestDelegate _next;

    public SessionTrackingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Create or retrieve session-based statistics ID
        var sessionId = GetOrCreateSessionId(context);
        context.Items["StatisticsSessionId"] = sessionId;

        await _next(context);
    }

    private string GetOrCreateSessionId(HttpContext context)
    {
        const string sessionKey = "MediatorStatisticsSessionId";
        
        if (context.Session.TryGetValue(sessionKey, out var sessionBytes))
        {
            return System.Text.Encoding.UTF8.GetString(sessionBytes);
        }

        var sessionId = $"stats_{DateTimeOffset.Now.ToUnixTimeSeconds()}_{Guid.NewGuid():N}"[..24];
        var sessionIdBytes = System.Text.Encoding.UTF8.GetBytes(sessionId);
        context.Session.Set(sessionKey, sessionIdBytes);
        
        return sessionId;
    }
}
```

### Request-Level Statistics Tracking

Automatically track all mediator requests:

```csharp
public class StatisticsTrackingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly MediatorStatisticsTracker _tracker;
    private readonly IHttpContextAccessor _httpContext;

    public StatisticsTrackingMiddleware(MediatorStatisticsTracker tracker, IHttpContextAccessor httpContext)
    {
        _tracker = tracker;
        _httpContext = httpContext;
    }

    public int Order => 0; // Execute first

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var sessionId = _httpContext.HttpContext?.Items["StatisticsSessionId"] as string;
        var requestType = request.GetType().Name;
        
        // Track based on request type
        if (IsQuery(request))
        {
            _tracker.TrackQuery(requestType, sessionId);
        }
        else
        {
            _tracker.TrackCommand(requestType, sessionId);
        }

        return await next();
    }

    private static bool IsQuery(TRequest request) => request is IQuery<TResponse>;
}
```

## 6. Advanced Analysis and Insights

The advanced analysis capabilities of Blazing.Mediator's statistics system go beyond basic counting and timing metrics to provide deep insights into your application's architecture, design patterns, and operational characteristics. These features are particularly valuable for large applications where understanding the complete scope of mediator usage, identifying architectural patterns, and detecting potential issues becomes crucial for maintainability and performance optimization.

### Query/Command Analysis

Analyze application structure and handler coverage:

```csharp
public class MediatorAnalysisService
{
    private readonly MediatorStatistics? _statistics;

    public MediatorAnalysisService(IServiceProvider serviceProvider)
    {
        _statistics = serviceProvider.GetService<MediatorStatistics>();
    }

    public async Task<AnalysisReport> GenerateAnalysisReport(IServiceProvider serviceProvider)
    {
        if (_statistics == null)
        {
            return new AnalysisReport(); // Return empty if statistics disabled
        }

        var queries = _statistics.AnalyzeQueries(serviceProvider, isDetailed: true);
        var commands = _statistics.AnalyzeCommands(serviceProvider, isDetailed: true);
        var notifications = _statistics.AnalyzeNotifications(serviceProvider, isDetailed: true);

        return new AnalysisReport
        {
            QueryTypes = queries.Count,
            CommandTypes = commands.Count,
            NotificationTypes = notifications.Count,
            MissingHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Missing) +
                            commands.Count(c => c.HandlerStatus == HandlerStatus.Missing),
            MultipleHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Multiple) +
                             commands.Count(c => c.HandlerStatus == HandlerStatus.Multiple),
            CustomInterfaces = queries.Count(q => !q.PrimaryInterface.StartsWith("IRequest") && !q.PrimaryInterface.StartsWith("IQuery")) +
                             commands.Count(c => !c.PrimaryInterface.StartsWith("IRequest") && !c.PrimaryInterface.StartsWith("ICommand"))
        };
    }
}

public record AnalysisReport
{
    public int QueryTypes { get; init; }
    public int CommandTypes { get; init; }
    public int NotificationTypes { get; init; }
    public int MissingHandlers { get; init; }
    public int MultipleHandlers { get; init; }
    public int CustomInterfaces { get; init; }
}
```

### Performance Insights

Extract actionable performance insights:

```csharp
public class PerformanceInsightsService
{
    private readonly MediatorStatistics? _statistics;

    public PerformanceInsightsService(IServiceProvider serviceProvider)
    {
        _statistics = serviceProvider.GetService<MediatorStatistics>();
    }

    public List<PerformanceInsight> GetInsights()
    {
        var insights = new List<PerformanceInsight>();
        
        if (_statistics == null)
        {
            insights.Add(new PerformanceInsight
            {
                Type = InsightType.Performance,
                Message = "Statistics tracking is disabled - enable for performance insights",
                Severity = Severity.Medium
            });
            return insights;
        }

        var summary = _statistics.GetPerformanceSummary();
        
        if (summary != null)
        {
            if (summary.Value.OverallSuccessRate < 99.0)
            {
                insights.Add(new PerformanceInsight
                {
                    Type = InsightType.Reliability,
                    Message = $"Success rate is {summary.Value.OverallSuccessRate:F1}%, consider investigating failures",
                    Severity = summary.Value.OverallSuccessRate < 95.0 ? Severity.High : Severity.Medium
                });
            }

            if (summary.Value.AverageExecutionTimeMs > 100)
            {
                insights.Add(new PerformanceInsight
                {
                    Type = InsightType.Performance,
                    Message = $"Average execution time is {summary.Value.AverageExecutionTimeMs:F1}ms, consider optimization",
                    Severity = summary.Value.AverageExecutionTimeMs > 500 ? Severity.High : Severity.Medium
                });
            }
        }

        return insights;
    }
}

public record PerformanceInsight
{
    public InsightType Type { get; init; }
    public string Message { get; init; } = string.Empty;
    public Severity Severity { get; init; }
}

public enum InsightType { Performance, Reliability, Memory, Scalability }
public enum Severity { Low, Medium, High }
```

## 7. API Endpoints for Statistics

Creating API endpoints for statistics provides a powerful way to expose monitoring and analysis data to external tools, dashboards, and other applications. These endpoints can serve both human-readable information for debugging and structured data for integration with monitoring systems. The sample implementations shown here demonstrate how to create comprehensive statistics APIs that provide both real-time operational data and deep architectural analysis, making them suitable for both operational monitoring and development tooling.

### Statistics Dashboard Endpoints (Sample Implementation)

Create API endpoints for real-time statistics viewing (these use the custom MediatorStatisticsTracker from samples):

```csharp
// Minimal API endpoints using custom MediatorStatisticsTracker
app.MapGet("/api/mediator/statistics", (MediatorStatisticsTracker tracker) =>
{
    var globalStats = tracker.GetGlobalStatistics();
    var sessions = tracker.GetAllSessionStatistics();
    
    return Results.Ok(new
    {
        message = "Real-Time Mediator Statistics",
        globalStatistics = new
        {
            summary = new
            {
                totalQueryExecutions = globalStats.TotalQueryExecutions,
                totalCommandExecutions = globalStats.TotalCommandExecutions,
                totalNotificationExecutions = globalStats.TotalNotificationExecutions,
                uniqueQueryTypes = globalStats.UniqueQueryTypes,
                uniqueCommandTypes = globalStats.UniqueCommandTypes,
                uniqueNotificationTypes = globalStats.UniqueNotificationTypes,
                activeSessions = globalStats.ActiveSessions,
                lastUpdated = globalStats.LastUpdated
            },
            details = new
            {
                queryTypes = globalStats.QueryTypes,
                commandTypes = globalStats.CommandTypes,
                notificationTypes = globalStats.NotificationTypes
            }
        },
        trackingInfo = new
        {
            method = "Real-time tracking via StatisticsTrackingMiddleware",
            sessionTracking = "Enabled - per-user session statistics available",
            backgroundCleanup = "Active - automatic cleanup of inactive sessions"
        }
    });
});

app.MapGet("/api/mediator/statistics/session/{sessionId}", (string sessionId, MediatorStatisticsTracker tracker) =>
{
    var sessionStats = tracker.GetSessionStatistics(sessionId);
    if (sessionStats == null)
    {
        return Results.NotFound(new { message = $"Session {sessionId} not found" });
    }

    return Results.Ok(new
    {
        message = $"Session Statistics for {sessionId}",
        sessionStatistics = sessionStats
    });
});

// Analysis endpoints using built-in MediatorStatistics
app.MapGet("/api/mediator/analysis/queries", (IServiceProvider serviceProvider) =>
{
    var statistics = serviceProvider.GetService<MediatorStatistics>();
    if (statistics == null)
    {
        return Results.Ok(new { message = "Statistics tracking disabled", queries = Array.Empty<object>() });
    }

    var analysis = statistics.AnalyzeQueries(serviceProvider, isDetailed: true);
    return Results.Ok(new
    {
        message = "Query Analysis",
        totalQueries = analysis.Count,
        queries = analysis.Select(q => new
        {
            q.ClassName,
            q.Assembly,
            q.Namespace,
            q.PrimaryInterface,
            q.HandlerStatus,
            q.HandlerDetails,
            ResponseType = q.ResponseType?.Name,
            q.IsResultType
        })
    });
});
```

## 8. Statistics Configuration Best Practices

Implementing statistics effectively requires a systematic approach to diagnosing configuration problems, performance impacts, and data collection issues. The most common problems stem from misunderstanding the configuration requirements or not properly balancing observability needs with performance constraints. Having effective diagnostic tools and understanding the common failure modes will help you quickly identify and resolve statistics-related issues in both development and production environments.

### Performance Considerations

1. **Development Environment**:
   - Enable detailed tracking for full observability
   - Use shorter retention periods for faster feedback
   - Enable middleware metrics for pipeline debugging

2. **Production Environment**:
   - Disable middleware metrics to reduce overhead
   - Use longer retention periods for trend analysis
   - Consider disabling performance counters for high-traffic systems

3. **Memory Management**:
   - Set appropriate `MaxTrackedRequestTypes` limits
   - Configure reasonable `MetricsRetentionPeriod`
   - Monitor cleanup interval effectiveness

### Monitoring Setup

Scheduled statistics reporting provides a foundation for proactive monitoring by regularly collecting and reporting metrics. This approach ensures that performance issues are detected promptly and provides historical data for trend analysis. The background service pattern shown here is particularly effective because it operates independently of request processing, ensuring that monitoring continues even during periods of low application activity.

```csharp
// Scheduled statistics reporting
services.AddHostedService<StatisticsReportingService>();

public class StatisticsReportingService : BackgroundService
{
    private readonly MediatorStatistics? _statistics;
    private readonly ILogger<StatisticsReportingService> _logger;

    public StatisticsReportingService(IServiceProvider serviceProvider, ILogger<StatisticsReportingService> logger)
    {
        _statistics = serviceProvider.GetService<MediatorStatistics>();
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_statistics != null)
            {
                _statistics.ReportStatistics();
            }
            else
            {
                _logger.LogDebug("Statistics tracking is disabled");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
```

### Integration with Application Metrics

Integrating Blazing.Mediator statistics with existing monitoring infrastructure allows you to leverage your current dashboards, alerting systems, and data analysis tools. The custom renderer pattern provides a clean way to transform statistics output into formats suitable for your monitoring systems, whether they use Prometheus metrics, Application Insights telemetry, or custom logging formats. This integration ensures that mediator metrics become part of your overall application observability strategy rather than operating in isolation.

```csharp
// Custom metrics renderer for integration with monitoring systems
public class MetricsRenderer : IStatisticsRenderer
{
    private readonly IMetricsCollector _metrics;

    public MetricsRenderer(IMetricsCollector metrics)
    {
        _metrics = metrics;
    }

    public void Render(string message)
    {
        if (TryParseMetric(message, out var metric))
        {
            _metrics.Record(metric.Name, metric.Value, metric.Tags);
        }
    }

    private bool TryParseMetric(string message, out (string Name, double Value, IDictionary<string, string> Tags) metric)
    {
        // Implementation to parse statistics messages into metrics
        metric = default;
        return false;
    }
}
```

## 9. Complete Setup Example

The complete setup example demonstrates how to integrate all aspects of Blazing.Mediator statistics into a production-ready application. This example combines the core statistics configuration with session-based tracking, custom analysis services, and health monitoring to create a comprehensive observability solution. The configuration shown here represents a balanced approach that provides extensive monitoring capabilities while maintaining production performance characteristics.

### Production-Ready Statistics Configuration

This comprehensive configuration demonstrates how to set up statistics for a production environment that requires detailed monitoring capabilities. The configuration balances performance concerns with observability needs, enabling essential metrics while carefully managing resource usage through retention policies and cleanup intervals. The integration with health checks and custom services shows how statistics can become an integral part of your application's operational infrastructure.

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Session support for user-level tracking (if using sample pattern)
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(2);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        services.AddHttpContextAccessor();

        // Custom statistics services (from samples)
        services.AddScoped<MediatorStatisticsTracker>();
        services.AddHostedService<StatisticsCleanupService>();
        
        // Analysis services
        services.AddScoped<PerformanceInsightsService>();
        services.AddScoped<MediatorAnalysisService>();

        // Mediator with comprehensive statistics
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking(options =>
            {
                // Production configuration
                options.EnableRequestMetrics = true;
                options.EnableNotificationMetrics = true;
                options.EnableMiddlewareMetrics = false; // Disabled for performance
                options.EnablePerformanceCounters = true; // Enabled for insights
                options.EnableDetailedAnalysis = true;    // Enabled for analysis APIs

                // Memory management
                options.MetricsRetentionPeriod = TimeSpan.FromHours(24);
                options.CleanupInterval = TimeSpan.FromHours(2);
                options.MaxTrackedRequestTypes = 2000;
            });

            // Add custom statistics tracking middleware (application-level)
            config.AddMiddleware<StatisticsTrackingMiddleware<,>>();
            config.AddMiddleware<StatisticsTrackingVoidMiddleware<>>();

        }, typeof(Program).Assembly);

        // Health checks
        services.AddHealthChecks()
            .AddCheck<MediatorapplicationHealthCheck>("mediator-health");
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseSession();
        app.UseMiddleware<SessionTrackingMiddleware>();
        
        // Statistics endpoints
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health");
            // Add statistics API endpoints here
        });
    }
}
```

## 10. Troubleshooting Statistics

Troubleshooting statistics issues requires a systematic approach to diagnosing configuration problems, performance impacts, and data collection issues. The most common problems stem from misunderstanding the configuration requirements or not properly balancing observability needs with performance constraints. Having effective diagnostic tools and understanding the common failure modes will help you quickly identify and resolve statistics-related issues in both development and production environments.

### Common Issues

1. **Statistics Not Collecting**
   - Verify statistics are enabled: `config.WithStatisticsTracking()`
   - Check that `MediatorStatistics` is registered in DI
   - Ensure `StatisticsOptions.IsEnabled` returns true

2. **High Memory Usage**
   - Reduce `MaxTrackedRequestTypes` limit
   - Decrease `MetricsRetentionPeriod`
   - Disable `EnableDetailedAnalysis` if not needed

3. **Performance Impact**
   - Disable `EnableMiddlewareMetrics` in production
   - Disable `EnablePerformanceCounters` for high-traffic scenarios
   - Consider using `StatisticsOptions.Production()`

### Diagnostic Endpoints

Diagnostic endpoints provide essential visibility into the current state of your statistics configuration and can help identify configuration issues or resource usage problems. These endpoints are particularly valuable for troubleshooting production issues because they provide real-time information about the statistics system's status without requiring access to logs or debugging tools. The diagnostic information includes both configuration status and resource usage metrics, giving you a complete picture of how the statistics system is operating.

```csharp
app.MapGet("/api/mediator/diagnostics", (IServiceProvider serviceProvider) =>
{
    var statistics = serviceProvider.GetService<MediatorStatistics>();
    if (statistics == null)
    {
        return Results.Ok(new
        {
            statisticsEnabled = false,
            message = "Statistics tracking is disabled"
        });
    }

    var summary = statistics.GetPerformanceSummary();
    return Results.Ok(new
    {
        statisticsEnabled = true,
        performanceCountersEnabled = summary != null,
        totalTrackedOperations = summary?.UniqueOperationTypes ?? 0,
        memoryUsage = $"{summary?.TotalMemoryAllocatedBytes ?? 0:N0} bytes"
    });
});
```

---

The Blazing.Mediator statistics system provides comprehensive monitoring and analysis capabilities to help you understand and optimize your application's mediator usage patterns. Configure the appropriate level of tracking for your environment and use the insights to improve performance and reliability.