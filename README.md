# Blazing.Mediator

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
-   **Auto-Discovery**: Automatic handler and middleware discovery with intelligent type resolution
-   **High Performance**: Optimized for speed with minimal overhead and efficient resource usage

### **🏗️ Developer Experience**

-   **Fluent Configuration**: Modern, type-safe configuration API with comprehensive validation
-   **Zero Configuration**: Works out of the box with sensible defaults and automatic setup
-   **Testing Friendly**: Easy to mock and test with comprehensive test coverage
-   **Type Safety**: Compile-time type checking with generic constraints and validation

## 📚 Documentation

### **🏁 Getting Started**

-   **[Installation](docs/getting-started/installation.md)** - Package installation and setup
-   **[Quick Start](docs/getting-started/quick-start.md)** - Get up and running in minutes
-   **[Basic Usage](docs/getting-started/basic-usage.md)** - Fundamental concepts and examples
-   **[Configuration](docs/getting-started/configuration.md)** - Complete configuration guide

### **🧠 Core Concepts**

-   **[Mediator Pattern](docs/core-concepts/mediator-pattern.md)** - Understanding the mediator pattern
-   **[CQRS Implementation](docs/core-concepts/cqrs.md)** - Command Query Responsibility Segregation
-   **[Architecture Guide](docs/core-concepts/README.md)** - Architectural principles and best practices

### **🔧 Advanced Topics**

-   **[Middleware System](docs/features/middleware/README.md)** - Custom middleware development and patterns
-   **[Features Overview](docs/features/README.md)** - Advanced features and capabilities
-   **[Sample Projects](docs/samples/README.md)** - Complete working examples
-   **[API Reference](docs/api-reference/README.md)** - Complete API documentation

## ⚡ Quick Start

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

// Add Blazing.Mediator with automatic handler discovery
builder.Services.AddBlazingMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
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

```csharp
builder.Services.AddBlazingMediator(options =>
{
    // Enable advanced features
    options.EnableStatistics = true;
    options.EnableOpenTelemetry = true;
    options.EnableStreaming = true;
    options.EnableNotifications = true;

    // Configure middleware pipeline
    options.UseMiddleware<LoggingMiddleware>();
    options.UseMiddleware<ValidationMiddleware>();
    options.UseMiddleware<CachingMiddleware>();
});
```

### **🌊 Streaming Support**

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

### **📢 Event-Driven Architecture**

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

### **📊 Built-in Observability**

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

```
BenchmarkDotNet=v0.13.10, OS=Windows 11
Intel Core i7-12700K, 1 CPU, 20 logical and 12 physical cores
.NET 9.0.0

| Method              | Mean      | Error    | StdDev   | Allocated |
|---------------------|-----------|----------|----------|-----------|
| Simple Request      | 89.32 ns  | 1.234 ns | 0.987 ns | 24 B      |
| Request with Cache  | 156.77 ns | 2.891 ns | 1.455 ns | 48 B      |
| Notification (3h)   | 267.44 ns | 4.123 ns | 2.876 ns | 96 B      |
| Streaming (1000)    | 2.847 μs  | 0.089 μs | 0.067 μs | 1.2 KB    |
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 9.0 SDK
3. Run `dotnet restore`
4. Run `dotnet test`

## ⭐ Show Your Support

If you find Blazing.Mediator useful, please give it a star on GitHub! It helps us understand that the project is valuable to the community.

[![GitHub stars](https://img.shields.io/github/stars/blazing-dev/Blazing.Mediator?style=social)](https://github.com/blazing-dev/Blazing.Mediator/stargazers)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**[📚 Explore the complete documentation →](docs/README.md)**

#### .NET CLI

```bash
dotnet add package Blazing.Mediator
```

#### NuGet Package Manager

```powershell
Install-Package Blazing.Mediator
```

#### Manually adding to your project

```xml
<PackageReference Include="Blazing.Mediator" Version="1.8.0" />
```

### Configuration

Configure the library in your `Program.cs` file. The `AddMediator` method will add the required services and automatically register request handlers from the specified assemblies.

#### Basic Registration

```csharp
using Blazing.Mediator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register Mediator with CQRS handlers using new fluent configuration
builder.Services.AddMediator(config =>
{
    config.AddAssembly(typeof(Program).Assembly);
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

#### With Middleware Pipeline

```csharp
using Blazing.Mediator;

// Register Mediator with middleware pipeline using fluent configuration
builder.Services.AddMediator(config =>
{
    config.WithStatisticsTracking()
          .WithMiddlewareDiscovery()
          .AddMiddleware<LoggingMiddleware<,>>()
          .AddMiddleware<ValidationMiddleware<,>>()
          .AddAssembly(typeof(Program).Assembly);
});
```

> **Note**: The older `AddMediator()` and `AddMediatorFromLoadedAssemblies()` methods with boolean parameters have been marked as obsolete and are being phased out. While they remain supported for backward compatibility, we recommend migrating to the new fluent configuration approach using `builder.Services.AddMediator(config => { ... })` for better type safety and enhanced functionality.

#### With Type-Constrained Middleware _**(NEW!)**_

```csharp
using Blazing.Mediator;

// Register Mediator with type-constrained middleware using fluent configuration
builder.Services.AddMediator(config =>
{
    config.WithStatisticsTracking()
          .WithMiddlewareDiscovery()
          // Add middleware that only processes ICommand requests
          .AddMiddleware<ValidationMiddleware<>>() // where TRequest : ICommand
          // Add middleware that only processes IQuery<TResponse> requests
          .AddMiddleware<CachingMiddleware<,>>() // where TRequest : IQuery<TResponse>
          // Add general middleware for all requests
          .AddMiddleware<LoggingMiddleware<,>>()
          .AddNotificationMiddleware<MyNotificationMiddleware>()
          .AddAssembly(typeof(Program).Assembly);
});
```

### Usage

#### Create a Query (CQRS Read Side)

```csharp
// Query - Request with response (Read operation)
public class GetUserByIdQuery : IRequest<UserDto>
{
    public int UserId { get; set; }
}

// Query Handler - Handles read operations
public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _repository;

    public GetUserByIdHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.UserId);
        return new UserDto { Id = user.Id, Name = user.Name, Email = user.Email };
    }
}
```

#### Create a Command (CQRS Write Side)

```csharp
// Command - Request without response (Write operation)
public class CreateUserCommand : IRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Command Handler - Handles write operations
public class CreateUserHandler : IRequestHandler<CreateUserCommand>
{
    private readonly IUserRepository _repository;

    public CreateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User { Name = request.Name, Email = request.Email };
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();
    }
}
```

#### Use in API Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    // Query endpoint (CQRS Read)
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var query = new GetUserByIdQuery { UserId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // Command endpoint (CQRS Write)
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        await _mediator.Send(command);
        return CreatedAtAction(nameof(GetUser), new { id = 0 }, null);
    }
}
```

## Give a ⭐

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

Also, if you find this library useful, and you're feeling really generous, then please consider [buying me a coffee ☕](https://bmc.link/gragra33).

## Documentation

For comprehensive documentation, examples, and advanced scenarios, see the [Mediator Pattern Implementation Guide](docs/MEDIATOR_PATTERN_GUIDE.md).

For streaming capabilities, real-time data processing, and advanced streaming patterns, see the [Mediator Streaming Guide](docs/MEDIATOR_STREAMING_GUIDE.md).

For event-driven architecture, domain events, and notification patterns, see the [Notification System Guide](docs/NOTIFICATION_GUIDE.md).

### CQRS Implementation

Blazing.Mediator naturally implements the **Command Query Responsibility Segregation (CQRS)** pattern:

**Commands (Write Operations)**

-   Use `IRequest` for operations that change state
-   Don't return data (void operations)
-   Focus on business intent and state changes
-   Examples: `CreateUserCommand`, `UpdateProductCommand`, `DeleteOrderCommand`

**Queries (Read Operations)**

-   Use `IRequest<TResponse>` for operations that retrieve data
-   Return data without changing state
-   Can be optimised with caching and read models
-   Examples: `GetUserByIdQuery`, `GetProductsQuery`, `GetOrderHistoryQuery`

This separation enables:

-   **Performance Optimisation**: Different strategies for reads vs writes
-   **Scalability**: Independent scaling of read and write operations
-   **Security**: Different validation and authorisation rules
-   **Maintainability**: Clear separation of concerns

### Middleware Pipeline

The optional middleware pipeline allows you to add cross-cutting concerns without modifying your core business logic:

```csharp
// Standard middleware - executes for all requests
public class LoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Pre-processing
        Console.WriteLine($"Processing {typeof(TRequest).Name}");

        var response = await next();

        // Post-processing
        Console.WriteLine($"Completed {typeof(TRequest).Name}");
        return response;
    }
}

// Conditional middleware - executes only for specific requests
public class OrderLoggingMiddleware<TRequest, TResponse> : IConditionalMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public bool ShouldExecute(TRequest request) =>
        request.GetType().Name.Contains("Order", StringComparison.OrdinalIgnoreCase);

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"🛒 Processing order: {typeof(TRequest).Name}");
        var response = await next();
        Console.WriteLine($"🛒 Order completed");
        return response;
    }
}

// Type-constrained middleware - executes only for commands (NEW!)
public class ValidationMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : ICommand  // Only processes commands, not queries
{
    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"📋 Validating command: {typeof(TRequest).Name}");

        // Validation logic here
        await ValidateAsync(request, cancellationToken);

        await next();
    }
}
```

### Sample Projects

The library includes seven comprehensive sample projects demonstrating different approaches:

## Sample Projects

The library includes nine comprehensive sample projects demonstrating different approaches:

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

4. **SimpleNotificationExample** _**(NEW!)**_ - Console application demonstrating recommended scoped notification patterns

    - Recommended approach using default scoped `IMediator` registration (not singleton) for proper resource management
    - Simple, straightforward notification subscribers
    - Multiple subscribers reacting to the same notification (`OrderCreatedNotification`)
    - Manual subscription management with runtime `IMediator.Subscribe()` control
    - Enhanced with `MediatorStatistics` analysis and notification tracking demonstrations
    - Clear documentation showing why this is the preferred pattern over complex background service approaches
    - Perfect starting point for understanding notification patterns in Blazing.Mediator

5. **TypedSimpleNotificationExample** _**(NEW!)**_ - Console application demonstrating type-constrained notification middleware with interface-based categorization

    - Type-constrained notification middleware processing only specific notification categories (Order, Customer, Inventory)
    - Interface-based notification categorization with `IOrderNotification`, `ICustomerNotification`, and `IInventoryNotification`
    - Selective middleware execution based on notification interface types for optimal performance
    - Dynamic pipeline analysis using `INotificationMiddlewarePipelineInspector` for runtime inspection
    - Comprehensive notification metrics tracking with success rates and performance timing
    - Visual distinction between different notification types in console output with category-specific processing
    - Advanced notification pipeline debugging and monitoring capabilities

6. **ECommerce.Api** - Demonstrates traditional Controller-based API with conditional middleware and notification system

    - Product and Order management with CQRS patterns
    - Comprehensive notification system with domain events
    - Real-time order status notifications and subscription management
    - Conditional logging middleware for performance optimization
    - Entity Framework integration with domain event publishing
    - FluentValidation integration with validation middleware
    - Background services for notification processing
    - **Mediator Statistics Endpoints**: Built-in API endpoints for monitoring mediator usage including query/command analysis, runtime statistics, and pipeline inspection

7. **UserManagement.Api** - Demonstrates modern Minimal API approach with standard middleware

    - User management operations
    - Comprehensive logging middleware
    - Clean architecture patterns
    - Error handling examples
    - **Mediator Statistics Endpoints**: Comprehensive API endpoints for analyzing mediator performance including query/command discovery, execution tracking, and detailed runtime statistics

8. **Streaming.Api** - Demonstrates real-time data streaming with multiple implementation patterns

    - Memory-efficient `IAsyncEnumerable<T>` streaming with large datasets
    - JSON streaming and Server-Sent Events (SSE) endpoints
    - Multiple Blazor render modes (SSR, Auto, Static, WebAssembly)
    - Stream middleware pipeline with logging and performance monitoring
    - Interactive streaming controls and real-time data visualization
    - 6 different streaming examples from minimal APIs to interactive WebAssembly clients

9. **OpenTelemetryExample** _**(NEW!)**_ - Comprehensive OpenTelemetry integration demonstration with modern cloud-native architecture

    - Full distributed tracing and metrics collection across web API server
    - Blazor WebAssembly client with real-time telemetry and performance monitoring & reporting dashboard
    - .NET Aspire support for local development with integrated observability dashboard and service discovery
    - OpenTelemetry middleware integration with automatic request/response tracing and performance metrics
    - Jaeger tracing visualization and Prometheus metrics collection with comprehensive telemetry data via Aspire Dashboard
    - Real-time performance monitoring and debugging capabilities with distributed correlation IDs
    - Production-ready observability patterns for microservices and cloud-native applications

## History

### V1.8.0

-   **OpenTelemetry Integration**: Full observability support with distributed tracing, metrics collection, and performance monitoring for enhanced debugging and monitoring capabilities
-   **Fluent Configuration API**: New modern fluent configuration approach using `builder.Services.AddMediator(config => { ... })` for improved type safety and enhanced functionality
-   **Legacy Method Deprecation**: Marked older `AddMediator()` and `AddMediatorFromLoadedAssemblies()` methods with boolean parameters as obsolete while maintaining backward compatibility during transition period
-   **OpenTelemetryExample Sample**: New comprehensive sample project demonstrating OpenTelemetry integration with web API server, Blazor client, and .NET Aspire support for modern cloud-native applications
-   **Enhanced Documentation**: Updated all documentation with new fluent configuration examples and comprehensive migration guidance from legacy registration methods
-   **Improved Developer Experience**: Streamlined configuration process with better IntelliSense support and compile-time validation through fluent API design

### V1.7.0

-   **Type-Constrained Middleware Support**: Enhanced middleware pipeline with generic type constraint validation for selective middleware execution based on interface types
-   **Request Middleware Type Constraints**: Added support for constraining request middleware to specific interface types (e.g., `ICommand`, `IQuery`) for precise middleware targeting
-   **Notification Middleware Type Constraints**: Extended type constraint support to notification middleware for selective notification processing based on interface implementations
-   **Enhanced CQRS Interface Support**: Improved middleware pipeline to respect typed constraints for compile-time middleware applicability
-   **TypedMiddlewareExample Sample**: New comprehensive sample project demonstrating type-constrained middleware with clear ICommand/IQuery distinction and selective validation
-   **Generic Constraint Validation**: Advanced generic type constraint checking with support for complex constraint scenarios including class, struct, and interface constraints
-   **Enhanced Type Safety**: Compile-time enforcement of middleware applicability through generic type constraints reducing runtime errors and improving performance
-   **Comprehensive Test Coverage**: Extensive test suite for type constraint validation covering edge cases, constraint inheritance, and complex generic scenarios
-   **Enhanced Sample Projects**: Updated ECommerce.Api and UserManagement.Api with comprehensive mediator statistics endpoints for runtime monitoring and analysis

### V1.6.2

-   **Enhanced Handler Analysis**: Updated `MediatorStatistics.AnalyzeQueries()` and `AnalyzeCommands()` with comprehensive handler detection and status reporting
-   **Handler Status Tracking**: New `HandlerStatus` enum with ASCII markers (`+` = found, `!` = missing, `#` = multiple) for easy visual identification
-   **Primary Interface Detection**: Enhanced `QueryCommandAnalysis` with `PrimaryInterface` property showing the main interface implemented (IQuery, ICommand, IRequest)
-   **IResult Detection**: New `IsResultType` property identifies ASP.NET Core IResult implementations for better API analysis
-   **Improved Statistics Display**: Enhanced console output in sample projects with multi-line, detailed analysis format for better readability
-   **Comprehensive Test Coverage**: Updated tests to cover all new `QueryCommandAnalysis` properties with full validation and edge case testing
-   **Documentation Enhancement**: Updated `MEDIATOR_PATTERN_GUIDE.md` with detailed `QueryCommandAnalysis` property table and enhanced example outputs

### V1.6.1

-   **MediatorStatistics Analysis**: New `MediatorStatistics.AnalyzeQueries()` and `AnalyzeCommands()` methods for comprehensive CQRS type discovery and analysis
-   **Runtime Statistics**: Enhanced `ReportStatistics()` functionality with automatic execution tracking via `IncrementQuery`, `IncrementCommand`, and `IncrementNotification`
-   **Statistics Monitoring**: Built-in performance monitoring and usage analytics with flexible `IStatisticsRenderer` system for custom output formats
-   **Application Insights**: Complete application discovery capabilities perfect for health checks, monitoring dashboards, and development tooling

### V1.6.0

-   **Enhanced Auto-Discovery**: `AddMediator` now separates request and notification middleware auto-discovery with new `discoverMiddleware` and `discoverNotificationMiddleware` parameters for granular control
-   **New Middleware Analysis**: Added `AnalyzeMiddleware` method to both `IMiddlewarePipelineInspector` and `INotificationMiddlewarePipelineInspector` for advanced pipeline debugging and monitoring
-   **Pipeline Enhancement**: Updated `NotificationPipelineBuilder` with improved middleware management and analysis capabilities
-   **Enhanced Testing**: Comprehensive new test coverage for `AnalyzeMiddleware` functionality and middleware discovery patterns
-   **Simple Notification Example**: New `SimpleNotificationExample` sample project demonstrating recommended scoped notification patterns with clear documentation and best practices
-   **CQRS Naming Support**: Added `IQuery`, `IQueryHandler`, `ICommand`, and `ICommandHandler` interfaces as semantic wrappers around `IRequest` and `IRequestHandler` for clearer CQRS pattern implementation

### V1.5.0

-   **Expanded Middleware Order Range**: Expanded ordered middleware range from -999/999 to int.MinValue/int.MaxValue for greater flexibility
-   **Enhanced Pipeline Inspection**: Enhanced `IMiddlewarePipelineInspector` with sample usage in `MiddlewareExample` sample project
-   **New MiddlewareExample Project**: New `MiddlewareExample` project to demonstrate the simple yet powerful pipeline capabilities - includes `ErrorHandlingMiddleware` & `ValidationMiddleware` implementations. Documentation included.

### V1.4.2

-   **Middleware Order Fix**: Fixed middleware order to follow registration order rather than `Order` property for more predictable behavior
-   **Enhanced Testing**: Updated tests with stricter middleware order validation checks
-   **New Examples Project**: Added comprehensive `Blazing.Mediator.Examples` project with detailed README showcasing all features and MediatR migration patterns
-   **Benchmarking**: New `Blazing.Mediator.Benchmarks` project for performance testing and optimisation

### V1.4.1

-   **Missing Interface Fix**: Added missing `IConditionalStreamRequestMiddleware` interface for conditional stream middleware support
-   **ECommerce.Api Enhancement**: Minor fix to `ECommerce.Api.Controllers.SimulateBulkOrder` method for improved bulk order simulation
-   **PowerShell Testing Script**: Added new `test-notifications-endpoints.ps1` script for comprehensive notification system testing and demonstration
-   **Documentation Updates**: Updated `NOTIFICATION_GUIDE.md` with detailed PowerShell script usage instructions and automated testing workflows

### V1.4.0

-   **Notification System**: Added comprehensive notification system with observer pattern implementation
-   **Event-Driven Architecture**: Introduced `INotification` and `INotificationHandler<T>` for domain event publishing and handling
-   **Subscription Management**: Added `INotificationSubscriber` interface for managing notification subscription lifecycle
-   **Notification Middleware**: Full middleware pipeline support for notification processing with cross-cutting concerns
-   **Complete Test Coverage**: Comprehensive test coverage for notification infrastructure with extensive test suite
-   **Notification Documentation**: New [Notification System Guide](docs/NOTIFICATION_GUIDE.md) with comprehensive examples and patterns
-   **Enhanced Samples**: Updated ECommerce.Api sample with notification system, domain events, and background services

### V1.3.0

-   **Native Streaming Support**: Added comprehensive streaming capabilities with `IStreamRequest<T>` and `IStreamRequestHandler<T,TResponse>`
-   **Stream Middleware Pipeline**: Full middleware support for streaming requests with `IStreamRequestMiddleware<TRequest,TResponse>`
-   **Memory-Efficient Processing**: Stream large datasets with `IAsyncEnumerable<T>` without loading entire datasets into memory
-   **Multiple Streaming Patterns**: Support for JSON streaming, Server-Sent Events (SSE), and real-time data feeds
-   **Comprehensive Streaming Sample**: New Streaming.Api sample with 6 different streaming implementations across multiple Blazor render modes
-   **Complete Test Coverage**: 100% test coverage for streaming middleware infrastructure with comprehensive test suite
-   **Streaming Documentation**: New [Mediator Streaming Guide](docs/MEDIATOR_STREAMING_GUIDE.md) with advanced streaming patterns and examples

### V1.2.0

-   Added automatic middleware discovery functionality for simplified configuration
-   Enhanced `AddMediator` method with `discoverMiddleware` parameter using method overloads
-   Automatic registration of all middleware implementations from specified assemblies
-   Support for middleware ordering through static/instance Order properties
-   Backward compatibility maintained with existing registration methods
-   Comprehensive documentation updates with auto-discovery examples

### V1.1.0

-   Enhanced middleware pipeline with conditional middleware support
-   Added `IMiddlewarePipelineInspector` for debugging and monitoring middleware execution
-   Full dependency injection support for middleware components
-   Performant middleware with conditional execution and optional priority-based execution
-   Enhanced pipeline inspection capabilities
-   Full test coverage with Shouldly assertions (replacing FluentAssertions)
-   Cleaned up samples and added middleware
-   Improved documentation with detailed examples and usage patterns

### V1.0.0

-   Initial release of Blazing.Mediator
-   Full CQRS support with separate Command and Query interfaces
-   Dependency injection integration with automatic handler registration
-   Multiple assembly scanning support
-   Comprehensive documentation and sample projects
-   .NET 9.0 support with nullable reference types
-   Extensive test coverage with unit and integration tests
