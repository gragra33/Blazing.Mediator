# Blazing.Mediator

[![NuGet Version](https://img.shields.io/nuget/v/Blazing.Mediator.svg)](https://www.nuget.org/packages/Blazing.Mediator)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Blazing.Mediator.svg)](https://www.nuget.org/packages/Blazing.Mediator)
[![.NET 8+](https://img.shields.io/badge/.NET-8%2B-512BD4)](https://dotnet.microsoft.com/download)

## Table of Contents

- [Overview](#overview)
- [🚀 Key Features](#-key-features)
- [⚡ Quick Start](#-quick-start)
- [🌟 Feature Highlights](#-feature-highlights)
- [🎯 Why Choose Blazing.Mediator?](#-why-choose-blazingmediator)
- [📈 Performance Benchmarks](#-performance-benchmarks)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)
- [📚 Documentation](#-documentation)
- [Sample Projects](#sample-projects)
- [History](#history)

## Overview

A high-performance, feature-rich implementation of the Mediator pattern for .NET applications. Built for modern development with comprehensive CQRS support, advanced middleware pipelines, real-time streaming, event-driven architecture, and full observability.

## 🚀 Key Features

### **🎯 Core Capabilities**

-   **Pure CQRS Implementation**: Built-in Command Query Responsibility Segregation with separate interfaces for commands and queries
-   **Advanced Middleware Pipeline**: Powerful middleware system with conditional execution, type constraints, and ordered processing
-   **Native Streaming Support**: Memory-efficient data streaming with `IAsyncEnumerable<T>` for real-time processing
-   **Event-Driven Architecture**: Comprehensive notification system for domain events and observer patterns

### **⚡ Advanced Features**

-   **OpenTelemetry Integration**: Full observability with distributed tracing, metrics collection, and performance monitoring
-   **Statistics & Analytics**: Built-in execution tracking, performance monitoring, and health checks
-   **Environment-Aware Configuration**: Automatic configuration with environment-specific presets and JSON support
-   **Auto-Discovery**: Automatic handler and middleware discovery with intelligent type resolution
-   **High Performance**: Optimized for speed with minimal overhead and efficient resource usage

### **🏗️ Developer Experience**

-   **Fluent Configuration**: Modern, type-safe configuration API with comprehensive validation
-   **Zero Configuration**: Works out of the box with sensible defaults and automatic setup
-   **Configuration Diagnostics**: Real-time configuration validation for production safety
-   **Extensive Debug Logging**: Powerful debug logging with configurable log levels, performance tracking, and detailed execution flow analysis
-   **Testing Friendly**: Easy to mock and test with comprehensive test coverage
-   **Type Safety**: Compile-time type checking with generic constraints and validation

## ⚡ Quick Start

**Blazing.Mediator** supports both .NET 9 and .NET 10, providing a modern, high-performance mediator implementation for current and next-generation .NET applications. The library leverages the latest language features and runtime optimizations to deliver exceptional performance and developer experience across both target frameworks.

### Installation

```bash
# .NET CLI
dotnet add package Blazing.Mediator

# Package Manager Console
Install-Package Blazing.Mediator
```

### Basic Setup

```csharp
using Blazing.Mediator;

var builder = WebApplication.CreateBuilder(args);

// With auto-discovery for middleware using fluent configuration
builder.Services.AddMediator(config =>
{
    config.WithMiddlewareDiscovery()
          .AddAssembly(typeof(Program).Assembly);
});

var app = builder.Build();
app.Run();
```

### Define Requests and Handlers

```csharp
// Query (Read operation) - Generic approach
public class GetUserQuery : IRequest<UserDto>
{
    public int UserId { get; init; }
}

// Alternative: CQRS naming (same functionality)
public class GetUserQuery : IQuery<UserDto>
{
    public int UserId { get; init; }
}

public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
// Alternative: public class GetUserHandler : IQueryHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Your logic here
        return new UserDto { Id = request.UserId, Name = "John Doe" };
    }
}

// Command (Write operation) - Generic approach
public class CreateUserCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public class CreateUserHandler : IRequestHandler<CreateUserCommand, int>
{
    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Your logic here
        return 1; // Return new user ID
    }
}
```

### 🏷️ CQRS Naming Conventions

Blazing.Mediator provides **both generic and CQRS-specific interfaces** for maximum flexibility:

| **CQRS Interface**     | **Generic Equivalent** | **Use Case**                          |
| ---------------------- | ---------------------- | ------------------------------------- |
| `IQuery<TResponse>`    | `IRequest<TResponse>`  | Read operations that return data      |
| `ICommand`             | `IRequest`             | Write operations with no return value |
| `ICommand<TResponse>`  | `IRequest<TResponse>`  | Write operations that return data     |
| `IQueryHandler<T,R>`   | `IRequestHandler<T,R>` | Handles queries                       |
| `ICommandHandler<T>`   | `IRequestHandler<T>`   | Handles void commands                 |
| `ICommandHandler<T,R>` | `IRequestHandler<T,R>` | Handles commands with return values   |

**Both approaches work identically** - choose based on your team's preferences:

-   **Generic**: `IRequest` for flexibility and simplicity
-   **CQRS**: `IQuery`/`ICommand` for explicit business intent

### Use in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id}")]
    public async Task<UserDto> GetUser(int id)
    {
        return await _mediator.Send(new GetUserQuery { UserId = id });
    }

    [HttpPost]
    public async Task<int> CreateUser(CreateUserCommand command)
    {
        return await _mediator.Send(command);
    }
}
```

## 🌟 Feature Highlights

### **🔧 Advanced Configuration**

Blazing.Mediator provides a powerful fluent configuration API that adapts to your environment automatically. Configure your mediator with environment-aware presets, JSON configuration support, and comprehensive validation. The configuration system includes intelligent discovery options for middleware and handlers, advanced statistics tracking, and seamless OpenTelemetry integration. Whether you're running in development, staging, or production, the configuration system ensures optimal settings for your deployment environment.

```csharp
builder.Services.AddMediator(config =>
{
    config.WithStatisticsTracking()
          .WithOpenTelemetryIntegration()
          .WithMiddlewareDiscovery()
          .WithNotificationHandlerDiscovery()
          .AddAssembly(typeof(Program).Assembly);
});
```

For comprehensive configuration patterns and advanced scenarios, see the **[Configuration Guide](docs/MEDIATOR_CONFIGURATION.md)**.

### **Multi-Assembly Configuration**

For applications with multiple assemblies and complex domain structures, Blazing.Mediator provides powerful multi-project configuration patterns that automatically discover handlers and middleware across assembly boundaries.

#### Manual Assembly Registration
```csharp
services.AddMediator(config =>
{
    // Enable comprehensive statistics tracking across all assemblies
    config.WithStatisticsTracking();
    
    // Register common middleware that works across all assemblies
    config.AddMiddleware(typeof(Common.Middleware.GlobalLoggingMiddleware<>));
    config.AddMiddleware(typeof(Common.Middleware.AuditMiddleware<>));
    config.AddMiddleware(typeof(Common.Middleware.TransactionMiddleware<>));
    
    // Register domain-specific middleware
    config.AddMiddleware(typeof(Products.Middleware.ProductValidationMiddleware<>));
    config.AddMiddleware(typeof(Orders.Middleware.OrderValidationMiddleware<>));
    
    // Register notification middleware
    config.AddNotificationMiddleware<Common.Middleware.GlobalNotificationMiddleware>();
    config.AddNotificationMiddleware(typeof(Common.Middleware.DomainEventMiddleware<>));
    
},
// Discover from all related assemblies
typeof(Common.Domain.BaseEntity).Assembly,        // Common
typeof(Products.Domain.Product).Assembly,         // Products  
typeof(Users.Domain.User).Assembly,               // Users
typeof(Orders.Domain.Order).Assembly,             // Orders
typeof(Program).Assembly                           // Main
);
```

#### Fully Automated Multi-Assembly Discovery
```csharp
// Register Blazing.Mediator with comprehensive discovery across all assemblies
services.AddMediator(config =>
{
    // Enable detailed statistics tracking for comprehensive analysis
    config.WithStatisticsTracking();

    // Enable middleware discovery
    config.WithMiddlewareDiscovery();

    // Enable notification middleware discovery
    config.WithNotificationMiddlewareDiscovery();

    // Register assemblies for handler and middleware discovery
    config.AddAssemblies(
            typeof(Common.Domain.BaseEntity).Assembly, // Common
            typeof(Products.Domain.Product).Assembly, // Products  
            typeof(Users.Domain.User).Assembly, // Users
            typeof(Orders.Domain.Order).Assembly, // Orders
            typeof(Program).Assembly // Main
        );
});
```

This automated approach provides:
- **Zero Configuration**: Automatic discovery eliminates manual middleware registration
- **Cross-Assembly Analysis**: Built-in support for analyzing components across multiple projects  
- **Domain Separation**: Clean boundaries while maintaining shared infrastructure
- **Development Efficiency**: Comprehensive debugging tools for multi-project solutions

See the **[AnalyzerExample](src/samples/AnalyzerExample/)** for a complete demonstration of multi-assembly configuration with 92 components across 5 projects, including intentionally missing handlers to showcase the powerful debugging capabilities of the analyzer tools.

### **🌊 Streaming Support**

Native streaming support enables memory-efficient processing of large datasets through `IAsyncEnumerable<T>`. Perfect for real-time data feeds, large file processing, and server-sent events. The streaming infrastructure includes full middleware pipeline support, allowing you to apply cross-cutting concerns like logging, validation, and metrics to your streaming operations. This eliminates the need to load entire datasets into memory, providing superior performance for data-intensive applications.

```csharp
// Streaming request
public class GetDataStream : IStreamRequest<DataItem>
{
    public string Source { get; init; } = string.Empty;
}

// Streaming handler
public class GetDataStreamHandler : IStreamRequestHandler<GetDataStream, DataItem>
{
    public async IAsyncEnumerable<DataItem> Handle(GetDataStream request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var item in ProcessDataAsync(request.Source, cancellationToken))
        {
            yield return item;
        }
    }
}

// Use in controller
[HttpGet("stream")]
public async IAsyncEnumerable<DataItem> StreamData([EnumeratorCancellation] CancellationToken cancellationToken)
{
    await foreach (var item in _mediator.SendStream(new GetDataStream { Source = "database" }, cancellationToken))
    {
        yield return item;
    }
}
```

For detailed streaming patterns and real-world examples, see the **[Streaming Guide](docs/MEDIATOR_STREAMING_GUIDE.md)**.

### **📢 Event-Driven Architecture**

Build robust event-driven systems with comprehensive notification support featuring both automatic handler discovery and manual subscription patterns. The notification system supports multiple handlers per event, complete middleware pipelines, and type-constrained processing for optimal performance. Whether you need decoupled domain events, observer patterns, or pub/sub messaging, the notification infrastructure provides the flexibility to implement complex event-driven architectures with ease.

```csharp
// Domain event
public class UserCreatedNotification : INotification
{
    public int UserId { get; init; }
    public string Email { get; init; } = string.Empty;
}

// Multiple handlers for the same event
public class EmailWelcomeHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Send welcome email
    }
}

public class AnalyticsHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Track user registration
    }
}

// Publish event
await _mediator.Publish(new UserCreatedNotification { UserId = userId, Email = email });
```

For complete event-driven patterns and notification strategies, see the **[Notification System Guide](docs/MEDIATOR_NOTIFICATION_GUIDE.md)**.

### **📊 Built-in Observability**

Comprehensive observability is built into every aspect of Blazing.Mediator, providing deep insights into your application's behavior. Full OpenTelemetry integration delivers distributed tracing, metrics collection, and performance monitoring for cloud-native applications. The statistics system tracks execution patterns, success rates, and performance metrics across all operations. Combined with extensive debug logging infrastructure, you get complete visibility into request handling, middleware execution, and notification processing for both development debugging and production monitoring.

```csharp
// Enable OpenTelemetry integration
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddBlazingMediatorInstrumentation())
    .WithMetrics(metrics => metrics.AddBlazingMediatorInstrumentation());

// Access built-in statistics
public class HealthController : ControllerBase
{
    private readonly IStatisticsService _statistics;

    [HttpGet("stats")]
    public async Task<MetricsSummary> GetStats()
    {
        return await _statistics.GetSummaryAsync();
    }
}
```

For complete observability implementation and cloud-native monitoring, see the **[OpenTelemetry Integration Guide](docs/MEDIATOR_OPEN_TELEMETRY_GUIDE.md)**, **[Statistics Guide](docs/MEDIATOR_STATISTICS_GUIDE.md)**, and **[Logging Guide](docs/MEDIATOR_LOGGING_GUIDE.md)**.

## 🎯 Why Choose Blazing.Mediator?

### **🚀 Performance First**

-   Minimal overhead with optimized execution paths
-   Efficient memory usage with streaming support
-   High-throughput request processing capabilities

### **🏗️ Production Ready**

-   Comprehensive error handling and validation
-   Built-in health checks and monitoring
-   Battle-tested in enterprise environments

### **🧪 Developer Friendly**

-   Extensive documentation and examples
-   Strong typing with compile-time validation
-   Easy testing with mocking support

### **🔄 Modern Architecture**

-   Clean CQRS implementation
-   Event-driven patterns
-   Microservice-ready design

## 📈 Performance Benchmarks

- BenchmarkDotNet=v0.15.2, OS=Windows 11
- AMD Ryzen 7 3700X, 1 CPU, 16 logical and 8 physical cores
- .NET 9.0.9

| Method                         | Mean     | Error     | StdDev    | Gen0   | Allocated |
|------------------------------- |---------:|----------:|----------:|-------:|----------:|
| SendRequests                   | 1.709 μs | 0.0097 μs | 0.0076 μs | 0.2155 |   1.77 KB |
| SendRequestsWithTelemetry      | 2.271 μs | 0.0439 μs | 0.0488 μs | 0.2823 |   2.32 KB |
| PublishToHandlers              | 2.002 μs | 0.0083 μs | 0.0073 μs | 0.2670 |   2.21 KB |
| PublishToHandlersWithTelemetry | 2.356 μs | 0.0183 μs | 0.0153 μs | 0.3510 |   2.89 KB |
| PublishToSubscribers           | 2.062 μs | 0.0121 μs | 0.0113 μs | 0.2670 |    2.2 KB |
| PublishToSubscribersTelemetry  | 2.544 μs | 0.0129 μs | 0.0115 μs | 0.3510 |   2.88 KB |

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 9.0 or .NET 10.0 SDK
3. Run `dotnet restore`
4. Run `dotnet test`

## ⭐ Show Your Support

If you find Blazing.Mediator useful, please give it a star on GitHub! It helps us understand that the project is valuable to the community.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Give a ⭐

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

Also, if you find this library useful, and you're feeling really generous, then please consider [buying me a coffee ☕](https://bmc.link/gragra33).

## 📚 Documentation

Comprehensive guides and documentation are available to help you master Blazing.Mediator:

- **[Mediator Pattern Guide](docs/MEDIATOR_PATTERN_GUIDE.md)** - Complete implementation guide with CQRS patterns, middleware pipelines, and testing strategies
- **[Streaming Guide](docs/MEDIATOR_STREAMING_GUIDE.md)** - Advanced streaming capabilities with `IAsyncEnumerable<T>` for real-time data processing  
- **[Notification System Guide](docs/MEDIATOR_NOTIFICATION_GUIDE.md)** - Event-driven architecture with domain events, observer patterns, and automatic/manual handlers
- **[OpenTelemetry Integration Guide](docs/MEDIATOR_OPEN_TELEMETRY_GUIDE.md)** - Full observability support with distributed tracing, metrics collection, and cloud-native monitoring
- **[Logging Guide](docs/MEDIATOR_LOGGING_GUIDE.md)** - Comprehensive debug logging infrastructure with configurable levels and performance tracking
- **[Statistics Guide](docs/MEDIATOR_STATISTICS_GUIDE.md)** - Advanced statistics tracking with detailed performance metrics and runtime insights
- **[Configuration Guide](docs/MEDIATOR_CONFIGURATION.md)** - In-depth guide to advanced configuration features, environment-specific settings, and JSON integration

### CQRS Implementation

Blazing.Mediator naturally implements the **Command Query Responsibility Segregation (CQRS)** pattern with comprehensive interface support:

#### Core Interfaces

**Generic Interfaces (Foundation)**
- `IRequest` - Commands that don't return data (void operations)
- `IRequest<TResponse>` - Queries/Commands that return data
- `IRequestHandler<TRequest>` - Handlers for void operations
- `IRequestHandler<TRequest, TResponse>` - Handlers for operations returning data

**CQRS Semantic Interfaces**
- `ICommand` - Write operations with no return value (extends `IRequest`)
- `ICommand<TResponse>` - Write operations returning data (extends `IRequest<TResponse>`)
- `IQuery<TResponse>` - Read operations returning data (extends `IRequest<TResponse>`)
- `ICommandHandler<TRequest>` - Command handlers (extends `IRequestHandler<TRequest>`)
- `ICommandHandler<TRequest, TResponse>` - Command handlers with return values (extends `IRequestHandler<TRequest, TResponse>`)
- `IQueryHandler<TRequest, TResponse>` - Query handlers (extends `IRequestHandler<TRequest, TResponse>`)

#### Streaming Support
- `IStreamRequest<TResponse>` - Streaming requests returning `IAsyncEnumerable<TResponse>`
- `IStreamRequestHandler<TRequest, TResponse>` - Handlers for streaming operations
- `IStreamRequestMiddleware<TRequest, TResponse>` - Middleware for streaming pipelines

#### Notification System (Dual Pattern Support)

**Automatic Handler Pattern (Publish-Subscribe)**
- `INotification` - Marker interface for domain events
- `INotificationHandler<TNotification>` - Automatic handlers discovered via DI
- `INotificationMiddleware` - Middleware for notification pipelines

**Manual Subscriber Pattern (Observer)**
- `INotificationSubscriber<TNotification>` - Manual subscribers requiring explicit registration
- Runtime subscription control via `mediator.Subscribe()` and `mediator.Unsubscribe()`

#### Type-Constrained Middleware Support
- **Request Middleware**: Can be constrained to `ICommand`, `IQuery<T>` or other interfaces
- **Notification Middleware**: Can be constrained to specific notification categories
- **Performance Benefits**: Only executes middleware relevant to the request type

#### Handler Registration Flexibility
Both approaches work identically - choose based on your team's preferences:
- **Generic**: `IRequest`/`IRequestHandler` for flexibility and simplicity
- **CQRS**: `IQuery`/`ICommand`/`IQueryHandler`/`ICommandHandler` for explicit business intent

## Sample Projects

The library includes comprehensive sample projects demonstrating different approaches:

1. **Blazing.Mediator.Examples** - Complete feature showcase and migration guide from MediatR

    - All core Blazing.Mediator features with side-by-side MediatR comparisons
    - Request/Response patterns (Ping/Pong), void commands (Jing), and notifications (Pinged)
    - Streaming examples with `IAsyncEnumerable<T>` for real-time data processing
    - Middleware pipeline demonstrations replacing MediatR pipeline behaviors
    - Performance optimizations and migration patterns
    - Perfect starting point for new users and MediatR migration

2. **MiddlewareExample** - Console application demonstrating comprehensive middleware pipeline and inspection capabilities

    - E-commerce scenario with CQRS patterns and auto-registration functionality
    - Advanced middleware pipeline with ordered execution and concrete/generic middleware with conditional operation
    - Simple error handling and multi-validation middleware examples
    - FluentValidation integration with error handling and retry patterns
    - Enhanced `IMiddlewarePipelineInspector` for debugging and monitoring middleware execution
    - `MiddlewarePipelineAnalyzer` helper class for runtime pipeline analysis and introspection
    - detailed readme documentation included

3. **TypedMiddlewareExample** _**(NEW!)**_ - Console application demonstrating type-constrained middleware with CQRS interface distinction

    - Clear separation between `ICommand` and `IQuery` interfaces with type-specific middleware
    - Validation middleware that only processes commands, bypassing queries entirely
    - Query-specific logging middleware demonstrating type constraints in action
    - Visual distinction between command and query processing in console output
    - Comprehensive examples showing how type constraints improve performance and maintainability
    - Perfect demonstration of selective middleware execution based on interface types

4. **NotificationSubscriberExample** - Console application demonstrating manual notification subscription pattern required for client applications

    - **Manual Subscription Pattern**: Required for Blazor WebAssembly, MAUI, WinForms, WPF, and Console applications
    - **Client App Compatibility**: Shows proper implementation for client applications where automatic handler discovery is not available
    - **Scoped Lifecycle Management**: Demonstrates proper DI container integration with scoped services
    - **Auto-Discovered Middleware**: Middleware pipeline with validation, logging, metrics, and audit middleware
    - **Simple Subscriber Classes**: Clean, testable notification handlers using `INotificationSubscriber<T>`
    - **Order Processing Workflow**: Email notifications and inventory management in e-commerce scenario
    - **AOT Compatibility**: Works with ahead-of-time compilation requirements

5. **TypedNotificationSubscriberExample** _**(NEW!)**_ - Console application demonstrating type-constrained notification middleware with manual subscription pattern

    - **Type-Constrained Middleware**: Selective middleware execution based on notification interface types (`IOrderNotification`, `ICustomerNotification`, `IInventoryNotification`)
    - **Manual Subscription Required**: Uses `INotificationSubscriber<T>` pattern requiring explicit subscription
    - **Interface-Based Categorization**: Business event categorization with compile-time type safety
    - **Performance Optimized**: Middleware only processes relevant notification types for optimal performance
    - **Dynamic Pipeline Analysis**: Runtime inspection using `INotificationMiddlewarePipelineInspector`
    - **Comprehensive Metrics**: Success rates, timing, and performance tracking for each notification type
    - **Visual Type Distinction**: Clear console output showing category-specific processing with icons

6. **NotificationHandlerExample** _**(NEW!)**_ - Console application demonstrating automatic notification handler discovery pattern

    - **Automatic Handler Discovery**: Zero-configuration notification handlers using `INotificationHandler<T>`
    - **Multiple Handler Pattern**: Multiple handlers processing the same notification independently  
    - **Complete Middleware Pipeline**: Validation, logging, and metrics with automatic ordering
    - **Business Operations**: Email, inventory, audit, and shipping handlers in e-commerce scenario
    - **Compile-time Registration**: Better performance and reliability than runtime subscription
    - **Error Isolation**: Each handler's errors don't affect others
    - **Scalable Architecture**: Easy to extend by simply implementing the interface

7. **NotificationHybridExample** _**(NEW!)**_ - Console application demonstrating hybrid notification pattern combining automatic and manual approaches

    - **Hybrid Architecture**: Combines `INotificationHandler<T>` (automatic) with `INotificationSubscriber<T>` (manual)
    - **Maximum Flexibility**: Use automatic handlers for core logic, manual subscribers for optional features
    - **Unified Middleware Pipeline**: Single pipeline processes both handler types
    - **Performance Optimization**: Automatic handlers have zero overhead, manual subscribers provide dynamic control
    - **Best of Both Worlds**: Zero-configuration for core functionality, explicit control for complex scenarios
    - **Real-world Application**: E-commerce scenario with email automation and optional inventory/audit features

8. **TypedNotificationHandlerExample** _**(NEW!)**_ - Console application demonstrating type-constrained notification middleware with automatic handler discovery

    - **Type-Constrained Middleware**: Selective middleware execution based on notification interface types
    - **Automatic Handler Discovery**: Zero-configuration handlers with multi-type support
    - **Interface-Based Categorization**: Order, Customer, and Inventory notification categories with type safety
    - **Performance Optimized**: Middleware only executes for relevant notification types
    - **Comprehensive Pipeline Analysis**: Runtime inspection with `INotificationMiddlewarePipelineInspector`
    - **Visual Type Distinction**: Clear console output showing type-based processing
    - **Advanced Type Constraints**: Compile-time enforcement with generic constraints

9. **TypedNotificationHybridExample** _**(NEW!)**_ - Console application demonstrating the ultimate typed hybrid notification pattern

    - **Typed Hybrid Pattern**: Combines automatic handlers, manual subscribers, AND type-constrained middleware
    - **Ultimate Performance**: Type-constrained middleware only processes relevant notifications
    - **Maximum Flexibility**: All three notification approaches in one unified system
    - **Category-Specific Processing**: Order, Customer, Inventory middleware with compile-time safety
    - **Multi-Interface Support**: Single notifications implementing multiple interfaces
    - **Advanced Architecture**: Production-ready patterns for complex enterprise applications
    - **Complete Observability**: Comprehensive metrics and pipeline analysis

10. **ECommerce.Api** - Demonstrates traditional Controller-based API with conditional middleware and notification system

    - Product and Order management with CQRS patterns
    - Comprehensive notification system with domain events
    - Real-time order status notifications and subscription management
    - Conditional logging middleware for performance optimization
    - Entity Framework integration with domain event publishing
    - FluentValidation integration with validation middleware
    - Background services for notification processing
    - **Mediator Statistics Endpoints**: Built-in API endpoints for monitoring mediator usage including query/command analysis, runtime statistics, and pipeline inspection

11. **UserManagement.Api** - Demonstrates modern Minimal API approach with standard middleware

    - User management operations
    - Comprehensive logging middleware
    - Clean architecture patterns
    - Error handling examples
    - **Mediator Statistics Endpoints**: Comprehensive API endpoints for analyzing mediator performance including query/command discovery, execution tracking, and detailed runtime statistics

12. **Streaming.Api** - Demonstrates real-time data streaming with multiple implementation patterns

    - Memory-efficient `IAsyncEnumerable<T>` streaming with large datasets
    - JSON streaming and Server-Sent Events (SSE) endpoints
    - Multiple Blazor render modes (SSR, Auto, Static, WebAssembly)
    - Stream middleware pipeline with logging and performance monitoring
    - Interactive streaming controls and real-time data visualization
    - 6 different streaming examples from minimal APIs to interactive WebAssembly clients

13. **OpenTelemetryExample** _**(NEW!)**_ - Comprehensive OpenTelemetry integration demonstration with modern cloud-native architecture

    - Full distributed tracing and metrics collection across web API server
    - Blazor WebAssembly client with real-time telemetry and performance monitoring & reporting dashboard
    - .NET Aspire support for local development with integrated observability dashboard and service discovery
    - OpenTelemetry middleware integration with automatic request/response tracing and performance metrics
    - Jaeger tracing visualization and Prometheus metrics collection with comprehensive telemetry data via Aspire Dashboard
    - Real-time performance monitoring and debugging capabilities with distributed correlation IDs
    - Production-ready observability patterns for microservices and cloud-native applications

14. **ConfigurationExample** _**(NEW!)**_ - Comprehensive demonstration of Configuration Features with environment-aware settings

    - Environment-aware configuration with automatic preset selection based on deployment environment
    - JSON configuration support with environment-specific overrides and perfect DRY implementation
    - Fluent preset integration solving original static factory method limitations with seamless chaining
    - Configuration diagnostics with real-time configuration diagnostics and validation reporting
    - Environment-specific validation preventing misconfiguration in production environments
    - Advanced configuration layering combining presets with JSON overrides intelligently
    - Production safety guards with intelligent validation for deployment environments
    - Complete configuration management patterns for enterprise applications

15. **AnalyzerExample** _**(NEW!)**_ - Comprehensive multi-assembly analyzer demonstration showcasing debugging capabilities across complex domain architectures

    - **Multi-Assembly Analysis**: Complete analysis across 92 mediator components in 5 projects (Common, Products, Users, Orders, Main) demonstrating real-world scale complexity
    - **Missing Handler Detection**: Intentionally missing handlers highlighted in red console output to showcase debugging benefits and identify components needing implementation
    - **Cross-Assembly Type Normalization**: Advanced type formatting with backtick removal, clean generic syntax, and proper namespace identification across multiple projects
    - **Extension Methods Showcase**: Demonstrates new `MiddlewareAnalysisExtensions` and `QueryCommandAnalysisExtensions` for comprehensive type normalization and clean output formatting
    - **Pipeline Analysis Tools**: Complete middleware pipeline inspection across assemblies with detailed ordering, constraints, and configuration analysis
    - **Domain Architecture**: Real-world multi-project structure with shared infrastructure, domain-specific modules, and clean architectural boundaries
    - **Debug Tooling Excellence**: Visual identification of missing components, assembly distribution statistics, and comprehensive runtime analysis for complex solutions
    - **Developer Productivity**: Essential tools for maintaining and debugging large-scale applications with multiple assemblies and complex mediator usage patterns

All of the Example Console applications demonstrate comprehensive **MediatorStatistics** analysis and **middleware analyzers** with detailed performance statistics, execution tracking, and pipeline inspection capabilities. These examples showcase real-time monitoring of queries, commands, and notifications with success rates, timing metrics, and handler discovery analysis. For complete details on implementing statistics tracking and performance monitoring in your applications, see the **[Statistics Guide](docs/MEDIATOR_STATISTICS_GUIDE.md)**.

## History

### V2.0.0

-   **.NET 10 Support**: Now supports .NET 10 with multi-targeting for both .NET 9 and .NET 10, providing developers with the latest framework features and performance improvements while maintaining backward compatibility
-   **OpenTelemetry Middleware Pipeline Enhancement**: Updated OpenTelemetry middleware information traces to show full generic signature for requests, streaming, and notifications
-   **Bug Fix - Duplicate OpenTelemetry Middleware Traces**: Fixed issue where generic middleware types (e.g., `ErrorHandlingMiddleware<TRequest, TResponse>`) appeared multiple times in telemetry traces due to open generic type definitions being instantiated as multiple closed generic types during execution
-   **Documentation Updates**: Updated all documentation guides to reflect .NET 10 support alongside .NET 9, including Configuration Guide, Logging Guide, Notification Guide, OpenTelemetry Guide, Pattern Guide, Statistics Guide, and Streaming Guide

### V1.8.1

-   **MiddlewareAnalysisExtensions**: New extension methods providing comprehensive type normalization for middleware analysis with clean formatting across assemblies
-   **QueryCommandAnalysisExtensions**: New extension methods for normalizing query and command analysis output with proper generic type formatting
-   **Bug Fix - RegistrationService**: Fixed edge case bug in RegistrationService with generic type definitions that could cause registration failures
-   **AnalyzerExample Sample**: New comprehensive multi-assembly sample demonstrating analyzer capabilities across 92 mediator components in 5 projects with intentionally missing handlers to showcase debugging benefits
-   **Enhanced Type Normalization**: Improved cross-assembly type formatting with backtick removal, clean generic syntax, and proper namespace identification
-   **Comprehensive Test Coverage**: Added extensive test coverage for new extension methods and normalization functionality
-   **Developer Experience**: Enhanced debugging capabilities with visual missing handler identification and comprehensive pipeline analysis tools

### V1.8.0

-   **OpenTelemetry Integration**: Full observability support with distributed tracing, metrics collection, and performance monitoring for enhanced debugging and monitoring capabilities with seamless integration for modern cloud-native applications
-   **Extensive Debug Logging**: Comprehensive debug logging infrastructure with configurable log levels, performance tracking, and detailed execution flow analysis for enhanced troubleshooting and monitoring
-   **Enhanced Statistics**: Advanced statistics tracking with detailed performance metrics, execution counters, pipeline analysis, and comprehensive runtime insights for production monitoring and optimization
-   **Fluent Configuration API**: New modern fluent configuration approach using `builder.Services.AddMediator(config => { ... })` for improved type safety, enhanced functionality, and streamlined developer experience with IntelliSense support
-   **Environment-Aware Configuration**: Advanced configuration management with automatic environment detection, preset application, and JSON configuration support for production-ready deployment patterns
-   **Configuration Diagnostics**: Real-time configuration diagnostics, validation reporting, and environment-specific validation for production safety and troubleshooting capabilities
-   **Legacy Method Deprecation**: Marked older `AddMediator()` and `AddMediatorFromLoadedAssemblies()` methods with boolean parameters as obsolete while maintaining backward compatibility during transition period with comprehensive migration guidance
-   **Enhanced Notification System**: Added comprehensive automatic notification handler system alongside existing manual subscriber pattern for maximum architectural flexibility
-   **Type-Constrained Middleware**: Advanced middleware system supporting generic type constraints for selective execution based on interface types (e.g., `ICommand`, `IQuery<T>`, `IOrderNotification`)
-   **New Sample Projects**: Added six comprehensive sample projects (OpenTelemetryExample, NotificationHandlerExample, NotificationHybridExample, TypedNotificationHandlerExample, TypedNotificationHybridExample, TypedNotificationSubscriberExample) demonstrating new fluent configuration, Telemetry, Statistics, Logging, automatic handlers, hybrid patterns, and type-constrained middleware
-   **ConfigurationExample Sample**: Demonstrates configuration features with environment-aware settings, JSON configuration, preset integration, and advanced diagnostics capabilities
-   **OpenTelemetryExample Sample**: New comprehensive sample project demonstrating OpenTelemetry integration with web API server, Blazor client, and .NET Aspire support for modern cloud-native applications with real-time telemetry visualization
-   **New Documentation Guides**: Added comprehensive [OpenTelemetry Integration Guide](docs/MEDIATOR_OPEN_TELEMETRY_GUIDE.md), [Mediator Statistics Configuration Guide
](docs/MEDIATOR_STATISTICS_GUIDE.md), [Mediator Logging Guide](docs/MEDIATOR_LOGGING_GUIDE.md), and [Mediator Configuration Guide](docs/MEDIATOR_CONFIGURATION.md) with detailed implementation examples, best practices, and troubleshooting scenarios
-   **Enhanced Documentation**: Updated all documentation with new fluent configuration examples, OpenTelemetry integration patterns, logging configuration, notification system patterns, and comprehensive migration guidance from legacy registration methods
-   **Improved Developer Experience**: Streamlined configuration process with better IntelliSense support, compile-time validation through fluent API design, enhanced debugging capabilities, and comprehensive observability features
