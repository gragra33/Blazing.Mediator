# Blazing.Mediator

A lightweight implementation of the Mediator pattern with built-in **Command Query Responsibility Segregation (CQRS)** support for .NET applications. Provides a simple, clean API for implementing CQRS patterns in your applications with optional middleware pipeline support.

## Key Features

The Blazing.Mediator library provides:

-   **Pure CQRS Implementation**: Built-in Command Query Responsibility Segregation with separate interfaces for commands (`IRequest`) and queries (`IRequest<TResponse>`)
-   **Lightweight & Fast**: Minimal overhead with efficient request dispatching
-   **Auto-Discovery**: Automatic request & notification middlewares and handler discovery with intelligent ordering
-   **Zero Configuration**: Works out of the box with minimal setup and automatic handler discovery
-   **Dependency Injection**: Full integration with .NET's built-in DI container
-   **Type Safety**: Compile-time type checking for requests and handlers
-   **Advanced Middleware Pipeline**: Optional middleware support with both standard and conditional middleware execution
    -   **Pipeline Inspection**: `IMiddlewarePipelineInspector` for advanced debugging and monitoring
    -   **Conditional Execution**: Execute middleware only for specific request types for optimal performance
    -   **Type-Constrained Middleware**: _**(NEW!)**_ Constrain middleware to specific interface types (e.g., `ICommand`, `IQuery`) for precise targeting
    -   **Ordered Execution**: Control middleware execution order with priority-based sequencing
    -   **Full DI Support**: Complete dependency injection support for middleware components
-   **Multiple Assembly Support**: Automatically scan and register handlers from multiple assemblies
-   **Error Handling**: Comprehensive error handling with detailed exception messages
-   **Async/Await Support**: Full asynchronous programming support throughout
-   **Testing Friendly**: Easy to mock and test individual handlers with full test coverage using Shouldly

### Streaming Features

-   **Native Streaming Support**: Built-in `IStreamRequest<T>` and `IStreamRequestHandler<T,TResponse>` for memory-efficient data streaming
-   **Real-time Data Processing**: Stream large datasets with `IAsyncEnumerable<T>` for optimal memory usage
-   **Stream Middleware Pipeline**: `IStreamRequestMiddleware<TRequest,TResponse>` for processing streaming requests with full pipeline support
-   **Performance Optimised**: Memory-efficient streaming without loading entire datasets into memory
-   **Backpressure Handling**: Natural flow control with async enumerable patterns
-   **Multiple Streaming Patterns**: Support for JSON streaming, Server-Sent Events (SSE), and real-time data feeds
-   **Interactive Streaming**: Perfect for real-time dashboards, live data feeds, progressive loading scenarios, and AI response streaming

### Notification Features

-   **Event-Driven Architecture**: Publish domain events with `INotification` and have multiple subscribers react to them
-   **Observer Pattern**: Multiple services can subscribe to the same notification without coupling using `INotificationHandler<T>`
-   **Subscription Management**: `INotificationSubscriber` interface for managing notification subscription lifecycle
-   **Asynchronous Processing**: All notifications are processed asynchronously for better performance
-   **Middleware Support**: Add cross-cutting concerns like logging and metrics to notification processing
    -   **Type-Constrained Notification Middleware**: _**(NEW!)**_ Constrain notification middleware to specific notification types for precise processing control
-   **New Middleware Analysis**: `INotificationMiddlewarePipelineInspector` for advanced debugging and monitoring
-   **Type Safety**: Strongly typed notifications with compile-time checking
-   **Testable Design**: Easy to test notification publishers and subscribers

### Analysis & Statistics Features _**(NEW)**_

-   **Application Discovery**: `MediatorStatistics.AnalyzeQueries()` and `AnalyzeCommands()` to discover all CQRS types in your application
-   **Runtime Statistics**: Built-in execution tracking with `ReportStatistics()` showing query, command, and notification counts
-   **Performance Monitoring**: Real-time usage statistics with automatic `IncrementQuery`, `IncrementCommand`, and `IncrementNotification` tracking
-   **Flexible Rendering**: Pluggable `IStatisticsRenderer` system for custom output (console, logging, APIs, monitoring dashboards)
-   **Health Check Integration**: Built-in support for monitoring endpoints and health checks with comprehensive mediator analysis
-   **Development Tooling**: Auto-discovery capabilities perfect for documentation generation and development-time analysis

## Table of Contents

<!-- TOC -->

-   [Quick Start](#quick-start)
-   [Installation](#installation)
    -   [.NET CLI](#net-cli)
    -   [NuGet Package Manager](#nuget-package-manager)
-   [Configuration](#configuration)
    -   [Basic Registration](#basic-registration)
    -   [With Middleware Pipeline](#with-middleware-pipeline)
-   [Usage](#usage)
    -   [Create a Query (CQRS Read Side)](#create-a-query-cqrs-read-side)
    -   [Create a Command (CQRS Write Side)](#create-a-command-cqrs-write-side)
    -   [Use in API Controllers](#use-in-api-controllers)
-   [Give a ⭐](#give-a-)
-   [Documentation](#documentation)
-   [CQRS Implementation](#cqrs-implementation)
-   [Middleware Pipeline](#middleware-pipeline)
-   [Sample Projects](#sample-projects)
-   [History](#history)
    -   [V1.7.0](#v170)
    -   [V1.6.2](#v162)
    -   [V1.6.1](#v161)
    -   [V1.6.0](#v160)
    -   [V1.5.0](#v150)

## Quick Start

### Installation

Add the [Blazing.Mediator](https://www.nuget.org/packages/Blazing.Mediator) NuGet package to your project.

Install the package via .NET CLI or the NuGet Package Manager.

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
<PackageReference Include="Blazing.Mediator" Version="1.7.0" />
```

### Configuration

Configure the library in your `Program.cs` file. The `AddMediator` method will add the required services and automatically register request handlers from the specified assemblies.

#### Basic Registration

```csharp
using Blazing.Mediator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register Mediator with CQRS handlers
builder.Services.AddMediator(typeof(Program).Assembly);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

#### With Middleware Pipeline

```csharp
using Blazing.Mediator;

// Register Mediator with optional middleware pipeline
builder.Services.AddMediator(config =>
{
    // Add logging middleware for all requests
    config.AddMiddleware<LoggingMiddleware<,>>();

    // Add conditional middleware for specific request types
    config.AddMiddleware<ValidationMiddleware<,>>();
}, typeof(Program).Assembly);
```

#### With Type-Constrained Middleware _**(NEW!)**_

```csharp
using Blazing.Mediator;

// Register Mediator with type-constrained middleware
builder.Services.AddMediator(config =>
{
    // Add middleware that only processes ICommand requests
    config.AddMiddleware<ValidationMiddleware<>>(); // where TRequest : ICommand

    // Add middleware that only processes IQuery<TResponse> requests  
    config.AddMiddleware<CachingMiddleware<,>>(); // where TRequest : IQuery<TResponse>

    // Add general middleware for all requests
    config.AddMiddleware<LoggingMiddleware<,>>();
}, typeof(Program).Assembly);
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

7. **UserManagement.Api** - Demonstrates modern Minimal API approach with standard middleware

    - User management operations
    - Comprehensive logging middleware
    - Clean architecture patterns
    - Error handling examples

8. **Streaming.Api** - Demonstrates real-time data streaming with multiple implementation patterns
    - Memory-efficient `IAsyncEnumerable<T>` streaming with large datasets
    - JSON streaming and Server-Sent Events (SSE) endpoints
    - Multiple Blazor render modes (SSR, Auto, Static, WebAssembly)
    - Stream middleware pipeline with logging and performance monitoring
    - Interactive streaming controls and real-time data visualization
    - 6 different streaming examples from minimal APIs to interactive WebAssembly clients

## History

### V1.7.0

-   **Type-Constrained Middleware Support**: Enhanced middleware pipeline with generic type constraint validation for selective middleware execution based on interface types
-   **Request Middleware Type Constraints**: Added support for constraining request middleware to specific interface types (e.g., `ICommand`, `IQuery`) for precise middleware targeting
-   **Notification Middleware Type Constraints**: Extended type constraint support to notification middleware for selective notification processing based on interface implementations
-   **Enhanced CQRS Interface Support**: Improved middleware pipeline to respect typed constraints for compile-time middleware applicability
-   **TypedMiddlewareExample Sample**: New comprehensive sample project demonstrating type-constrained middleware with clear ICommand/IQuery distinction and selective validation
-   **Generic Constraint Validation**: Advanced generic type constraint checking with support for complex constraint scenarios including class, struct, and interface constraints
-   **Enhanced Type Safety**: Compile-time enforcement of middleware applicability through generic type constraints reducing runtime errors and improving performance
-   **Comprehensive Test Coverage**: Extensive test suite for type constraint validation covering edge cases, constraint inheritance, and complex generic scenarios

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
