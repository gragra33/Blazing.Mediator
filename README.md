# Blazing.Mediator

A lightweight implementation of the Mediator pattern with built-in **Command Query Responsibility Segregation (CQRS)** support for .NET applications. Provides a simple, clean API for implementing CQRS patterns in your applications with optional middleware pipeline support.

## Key Features

The Blazing.Mediator library provides:

-   **🎯 Pure CQRS Implementation**: Built-in Command Query Responsibility Segregation with separate interfaces for commands (`IRequest`) and queries (`IRequest<TResponse>`)
-   **🚀 Lightweight & Fast**: Minimal overhead with efficient request dispatching
-   **🤖 Auto-Discovery**: Automatic middleware and handler discovery with intelligent ordering
-   **⚙️ Zero Configuration**: Works out of the box with minimal setup and automatic handler discovery
-   **⚙️ Dependency Injection**: Full integration with .NET's built-in DI container
-   **🔒 Type Safety**: Compile-time type checking for requests and handlers
-   **🔧 Advanced Middleware Pipeline**: Optional middleware support with both standard and conditional middleware execution
    -   **Pipeline Inspection**: `IMiddlewarePipelineInspector` for debugging and monitoring
    -   **Conditional Execution**: Execute middleware only for specific request types for optimal performance
    -   **Ordered Execution**: Control middleware execution order with priority-based sequencing
    -   **Full DI Support**: Complete dependency injection support for middleware components
-   **📦 Multiple Assembly Support**: Automatically scan and register handlers from multiple assemblies
-   **⚡ Error Handling**: Comprehensive error handling with detailed exception messages
-   **🔄 Async/Await Support**: Full asynchronous programming support throughout
-   **🧪 Testing Friendly**: Easy to mock and test individual handlers with full test coverage using Shouldly

### Key Streaming Features

-   **🌊 Native Streaming Support**: Built-in `IStreamRequest<T>` and `IStreamRequestHandler<T,TResponse>` for memory-efficient data streaming
-   **📡 Real-time Data Processing**: Stream large datasets with `IAsyncEnumerable<T>` for optimal memory usage
-   **🚀 Stream Middleware Pipeline**: `IStreamRequestMiddleware<TRequest,TResponse>` for processing streaming requests with full pipeline support
-   **⚡ Performance Optimised**: Memory-efficient streaming without loading entire datasets into memory
-   **🔄 Backpressure Handling**: Natural flow control with async enumerable patterns
-   **📊 Multiple Streaming Patterns**: Support for JSON streaming, Server-Sent Events (SSE), and real-time data feeds
-   **🎮 Interactive Streaming**: Perfect for real-time dashboards, live data feeds, progressive loading scenarios, and AI response streaming

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
-   [V1.2.0](#v120)
-   [V1.1.0](#v110)
-   [V1.0.0](#v100)
<!-- TOC -->

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
<PackageReference Include="Blazing.Mediator" Version="1.2.0" />
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
```

### Sample Projects

The library includes three comprehensive sample projects demonstrating different approaches:

1. **ECommerce.Api** - Demonstrates traditional Controller-based API with conditional middleware

    - Product and Order management
    - Conditional logging middleware for performance
    - Entity Framework integration
    - FluentValidation integration

2. **UserManagement.Api** - Demonstrates modern Minimal API approach with standard middleware

    - User management operations
    - Comprehensive logging middleware
    - Clean architecture patterns
    - Error handling examples

3. **Streaming.Api** - Demonstrates real-time data streaming with multiple implementation patterns
    - Memory-efficient `IAsyncEnumerable<T>` streaming with large datasets
    - JSON streaming and Server-Sent Events (SSE) endpoints
    - Multiple Blazor render modes (SSR, Auto, Static, WebAssembly)
    - Stream middleware pipeline with logging and performance monitoring
    - Interactive streaming controls and real-time data visualization
    - 6 different streaming examples from minimal APIs to interactive WebAssembly clients

## History

### V1.3.0

-   **🌊 Native Streaming Support**: Added comprehensive streaming capabilities with `IStreamRequest<T>` and `IStreamRequestHandler<T,TResponse>`
-   **📡 Stream Middleware Pipeline**: Full middleware support for streaming requests with `IStreamRequestMiddleware<TRequest,TResponse>`
-   **⚡ Memory-Efficient Processing**: Stream large datasets with `IAsyncEnumerable<T>` without loading entire datasets into memory
-   **📊 Multiple Streaming Patterns**: Support for JSON streaming, Server-Sent Events (SSE), and real-time data feeds
-   **🎮 Comprehensive Streaming Sample**: New Streaming.Api sample with 6 different streaming implementations across multiple Blazor render modes
-   **🧪 Complete Test Coverage**: 100% test coverage for streaming middleware infrastructure with comprehensive test suite
-   **📖 Streaming Documentation**: New [Mediator Streaming Guide](docs/MEDIATOR_STREAMING_GUIDE.md) with advanced streaming patterns and examples

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
