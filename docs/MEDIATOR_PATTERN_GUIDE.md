# Blazing.Mediator - Complete Implementation Guide

## Overview

The Mediator pattern decouples components by having them communicate through a central mediator rather than directly with each other. This promotes loose coupling, better testability, and cleaner architecture.

`Blazing.Mediator` provides a lightweight implementation of the Mediator pattern for .NET applications that naturally implements **Command Query Responsibility Segregation (CQRS)** by separating read operations (queries) from write operations (commands). This separation allows for optimised data models, improved performance, and better scalability.

### Key Features

-   **Pure CQRS Implementation**: Clean separation of Commands and Queries with distinct interfaces
-   **Optional Middleware Pipeline**: Add cross-cutting concerns like logging, validation, and caching
-   **Conditional Middleware**: Execute middleware only for specific request types for optimal performance
-   **Auto-Discovery**: Automatic middleware and handler discovery with intelligent ordering
-   **Zero Configuration**: Works out of the box with minimal setup and automatic handler discovery
-   **High Performance**: Lightweight implementation optimised for speed with minimal overhead
-   **Fully Testable**: Built with testing in mind - easy to mock and unit test handlers
-   **Multiple Assembly Support**: Automatically scan and register handlers from multiple assemblies
-   **Type Safety**: Compile-time type checking for requests, handlers, and responses
-   **Comprehensive Documentation**: Complete guides, examples, and sample projects
-   **Integrated Debugging Tools**: Inspect Queries, Commands, Request pipeline Middleware, and Notification pipeline Middleware to quickly identify and resolve issues.
-   **Real-Time Statistics**: Monitor running Query and Command statistics to gain insights into application performance and usage patterns.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Core Concepts](#core-concepts)
3. [Creating Requests](#creating-requests)
4. [Implementing Handlers](#implementing-handlers)
5. [Setup and Registration](#setup-and-registration)
6. [Usage in APIs](#usage-in-apis)
7. [Middleware Pipeline](#middleware-pipeline)
8. [MediatorStatistics](#mediatorstatistics)
9. [Validation and Error Handling](#validation-and-error-handling)
10. [Testing Strategies](#testing-strategies)
11. [Sample Projects](#sample-projects)
12. [Complete Examples](#complete-examples)

## Quick Start

Get up and running with Blazing.Mediator in under 5 minutes:

### 1. Install the Package

#### Install the package via .NET CLI or the NuGet Package Manager.

Add the Blazing.Mediator NuGet package to your project.

##### .NET CLI

```bash
dotnet add package Blazing.Mediator
```

##### NuGet Package Manager

```bash
Install-Package Blazing.Mediator
```

#### Manually adding to your project

```xml
<PackageReference Include="Blazing.Mediator" Version="1.6.2" />
```

### 2. Create Your First Query

```csharp
// Request
public class GetUserQuery : IRequest<UserDto>
{
    public int UserId { get; set; }
}

// Handler
public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Your logic here
        return new UserDto { Id = request.UserId, Name = "John Doe" };
    }
}
```

### 3. Register Services

```csharp
// Program.cs - Basic registration
builder.Services.AddMediator(typeof(Program).Assembly);

// With auto-discovery for middleware
builder.Services.AddMediator(typeof(Program).Assembly, discoverMiddleware: true);
```

### 4. Use in Controller

```csharp
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id}")]
    public async Task<UserDto> GetUser(int id)
    {
        return await _mediator.Send(new GetUserQuery { UserId = id });
    }
}
```

That's it! You now have a working Mediator implementation. Continue reading for detailed explanations and advanced scenarios.

## Core Concepts

### CQRS Implementation

`Blazing.Mediator` inherently implements the **Command Query Responsibility Segregation (CQRS)** pattern by providing distinct interfaces for commands and queries:

-   **Commands**: Operations that change state (Create, Update, Delete) but typically don't return data
-   **Queries**: Operations that retrieve data without changing state (Read operations)

This separation enables:

-   **Performance Optimisation**: Queries can use optimised read models and caching
-   **Scalability**: Read and write operations can be scaled independently
-   **Security**: Different validation and authorisation rules for commands vs queries
-   **Maintainability**: Clear separation of concerns between data modification and retrieval

### Requests

-   **IRequest**: Marker interface for commands that don't return data (CQRS Commands)
-   **IRequest\<TResponse>**: Interface for queries that return data (CQRS Queries)

### Handlers

-   **IRequestHandler\<TRequest>**: Handle commands without return values (CQRS Command Handlers)
-   **IRequestHandler\<TRequest, TResponse>**: Handle queries with return values (CQRS Query Handlers)

### Mediator

-   **IMediator**: Central dispatcher that routes requests to appropriate handlers

### How the Mediator Pattern Works

The following diagram illustrates the basic flow of requests through the Blazing.Mediator system:

> **Note**: This is a simplified overview. For a detailed middleware pipeline flow diagram that includes exception handling and middleware execution order, see the [Middleware Pipeline Flow](#pipeline-flow) section.

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                            Blazing.Mediator Flow                                │
└─────────────────────────────────────────────────────────────────────────────────┘

                    ┌─────────────────┐
                    │   Client Code   │
                    │  (Controller,   │
                    │  Minimal API,   │
                    │   Service)      │
                    └─────────┬───────┘
                              │
                              │ 1. Send Request
                              ▼
                    ┌─────────────────┐
                    │   IMediator     │
                    │  (Dispatcher)   │
                    └─────────┬───────┘
                              │
                              │ 2. Route to Handler
                              ▼
         ┌────────────────────┴────────────────────┐
         │                                         │
         ▼                                         ▼
┌─────────────────┐                       ┌─────────────────┐
│  Command        │                       │  Query          │
│  Handler        │                       │  Handler        │
│  (Write Side)   │                       │  (Read Side)    │
│                 │                       │                 │
│ IRequestHandler │                       │ IRequestHandler │
│ <TRequest>      │                       │ <TRequest,      │
│                 │                       │  TResponse>     │
└─────────┬───────┘                       └─────────┬───────┘
          │                                         │
          │ 3. Execute Business Logic               │ 3. Query Data
          ▼                                         ▼
┌─────────────────┐                       ┌─────────────────┐
│  Database/      │                       │  Database/      │
│  External       │                       │  Cache/         │
│  Services       │                       │  Read Model     │
│  (Write)        │                       │  (Read)         │
└─────────┬───────┘                       └─────────┬───────┘
          │                                         │
          │ 4. Return (void/Unit)                   │ 4. Return Data
          ▼                                         ▼
┌─────────────────┐                       ┌─────────────────┐
│  No Response    │                       │  Response DTO   │
│  (Command       │                       │  (Query         │
│   Completed)    │                       │   Result)       │
└─────────────────┘                       └─────────────────┘

═══════════════════════════════════════════════════════════════════════════════════

Example Request Types:

Commands (IRequest):                      Queries (IRequest<TResponse>):
├── CreateUserCommand                     ├── GetUserByIdQuery → UserDto
├── UpdateProductCommand                  ├── GetProductsQuery → List<ProductDto>
├── DeleteOrderCommand                    ├── GetOrderHistoryQuery → PagedResult<OrderDto>
└── UpdateUserPasswordCommand             └── GetUserStatisticsQuery → StatisticsDto

═══════════════════════════════════════════════════════════════════════════════════
```

### Benefits of the Mediator Pattern

The Mediator pattern provides several key architectural benefits:

#### Loose Coupling

-   Controllers and services don't need to know about specific handler implementations
-   Dependencies are managed through interfaces rather than concrete classes
-   Easy to swap out handlers without affecting client code
-   Promotes clean separation between presentation and business logic layers

#### Enhanced Testability

-   Handlers can be mocked independently for unit testing
-   Business logic is isolated in focused, testable units
-   Integration tests can verify the complete request/response flow
-   Dependency injection makes testing scenarios straightforward

#### Single Responsibility Principle

-   Each handler has one clear responsibility
-   Business logic is organised into discrete, focused units
-   Easier to understand and maintain individual components
-   Reduces complexity by avoiding monolithic service classes

#### CQRS Implementation

-   Clear separation between Commands (write operations) and Queries (read operations)
-   Optimised data models for different use cases
-   Different validation and security rules for reads vs writes
-   Enables different scaling strategies for read and write operations

#### Improved Scalability

-   Read and write operations can be scaled independently
-   Query handlers can use optimized read models or caching
-   Command handlers can focus on business rules and data consistency
-   Supports distributed architectures and microservices patterns

#### Better Maintainability

-   Clear request/response flow through the system
-   Centralised request routing and handling
-   Consistent error handling and validation patterns
-   Easier to add new features without modifying existing code

## Setup and Registration

### Basic Registration

The simplest way to register Blazing.Mediator is to scan a single assembly:

```csharp
// Program.cs
builder.Services.AddMediator(typeof(Program).Assembly);
```

### Multi-Assembly Registration

For larger applications with multiple projects, register handlers from all relevant assemblies:

```csharp
// Register handlers from multiple assemblies
builder.Services.AddMediator(
    typeof(Program).Assembly,                    // Current assembly (API)
    typeof(GetUserHandler).Assembly,             // Application layer
    typeof(User).Assembly                        // Domain layer (if needed)
);

// With auto-discovery for middleware
builder.Services.AddMediator(
    discoverMiddleware: true,
    typeof(Program).Assembly,                    // Current assembly (API)
    typeof(GetUserHandler).Assembly,             // Application layer
    typeof(LoggingMiddleware<,>).Assembly        // Infrastructure layer
);
```

### Alternative Registration Methods

```csharp
// Method 1: Using assembly marker types
services.AddMediator(
    typeof(GetUserHandler),
    typeof(CreateOrderHandler),
    typeof(UpdateProductHandler)
);

// Method 1a: With auto-discovery
services.AddMediator(
    discoverMiddleware: true,
    typeof(GetUserHandler),
    typeof(CreateOrderHandler),
    typeof(UpdateProductHandler)
);

// Method 2: Using assembly references
services.AddMediator(
    Assembly.GetExecutingAssembly(),
    typeof(ExternalHandler).Assembly
);

// Method 2a: With auto-discovery
services.AddMediator(
    discoverMiddleware: true,
    Assembly.GetExecutingAssembly(),
    typeof(ExternalHandler).Assembly
);

// Method 3: Scan calling assembly automatically
services.AddMediatorFromCallingAssembly();

// Method 3a: Scan calling assembly with auto-discovery
services.AddMediatorFromCallingAssembly(discoverMiddleware: true);

// Method 4: Scan with filter
services.AddMediatorFromLoadedAssemblies(assembly =>
    assembly.FullName.StartsWith("MyCompany.") &&
    assembly.FullName.Contains(".Application"));

// Method 4a: Scan with filter and auto-discovery
services.AddMediatorFromLoadedAssemblies(
    discoverMiddleware: true,
    assembly => assembly.FullName.StartsWith("MyCompany.") &&
               assembly.FullName.Contains(".Application"));

// Simple overload methods for convenience - add auto-discovery to existing methods
services.AddMediatorFromCallingAssembly(discoverMiddleware: true);
services.AddMediatorFromLoadedAssemblies(discoverMiddleware: true);
```

### Complete Application Setup

#### For Controllers (Recommended for CRUD APIs)

```csharp
using Blazing.Mediator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Mediator with multiple assemblies
builder.Services.AddMediator(
    typeof(Program).Assembly,                    // Current assembly
    typeof(GetUserHandler).Assembly             // Application layer assembly
);

// Add your other services (DbContext, repositories, etc.)
// builder.Services.AddDbContext<AppDbContext>(...);
// builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

#### For Minimal APIs (Recommended for Simple APIs)

```csharp
using Blazing.Mediator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Mediator (same registration regardless of API style)
builder.Services.AddMediator(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Define endpoints using minimal API style
var api = app.MapGroup("/api/users").WithTags("Users");

api.MapGet("/{id}", async (int id, IMediator mediator) =>
    await mediator.Send(new GetUserQuery { UserId = id }));

api.MapPost("/", async (CreateUserCommand command, IMediator mediator) =>
    await mediator.Send(command));

app.Run();
```

## Creating Requests

The foundation of CQRS is the clear separation between commands (write operations) and queries (read operations). This separation allows for different optimization strategies and architectural patterns.

### Queries (CQRS Read Side - Return Data)

Queries are read-only operations that retrieve data without modifying state. They can be optimized for specific read scenarios, use caching, and access denormalized read models.

```csharp
// Get single user - Simple query
public class GetUserByIdQuery : IRequest<UserDto>
{
    public int UserId { get; set; }
}

// Get multiple users with pagination - Complex query with filtering
public class GetUsersQuery : IRequest<PagedResult<UserDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SearchTerm { get; set; }
    public bool IncludeInactive { get; set; } = false;
}

// Complex query with multiple parameters - Reporting scenario
public class GetUserOrdersQuery : IRequest<List<OrderDto>>
{
    public int UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public OrderStatus? Status { get; set; }
}

// Analytical query - Read-optimized for reporting
public class GetUserStatisticsQuery : IRequest<UserStatisticsDto>
{
    public int UserId { get; set; }
    public int MonthsBack { get; set; } = 12;
}
```

### Commands (CQRS Write Side - Don't Return Data)

Commands represent business operations that change the system state. They focus on business intent rather than technical data manipulation.

```csharp
// Create user - Business operation
public class CreateUserCommand : IRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
}

// Update user - Focused on business intent
public class UpdateUserCommand : IRequest
{
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

// Delete user - Clear business operation
public class DeleteUserCommand : IRequest
{
    public int UserId { get; set; }
    public string Reason { get; set; } // Audit trail
}

// Domain-specific command
public class ActivateUserAccountCommand : IRequest
{
    public int UserId { get; set; }
    public string ActivationToken { get; set; }
}
```

### Commands with Return Values (Hybrid Approach)

Sometimes commands need to return minimal data (like generated IDs) while still maintaining CQRS principles.

```csharp
// Create user and return the created user's ID
public class CreateUserWithIdCommand : IRequest<int>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

// Update user and return success status
public class UpdateUserWithResultCommand : IRequest<OperationResult>
{
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

// Result object for commands
public class OperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public Dictionary<string, object> Data { get; set; }
}
```

## Implementing Handlers

CQRS handlers are optimized for their specific purpose: query handlers focus on efficient data retrieval while command handlers focus on business logic and state changes.

### Query Handlers (CQRS Read Side)

Query handlers are optimized for data retrieval and can use different data sources, caching strategies, and read models.

```csharp
public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserReadRepository _userRepository; // Read-optimized repository
    private readonly IMemoryCache _cache; // Caching for performance

    public GetUserByIdHandler(IUserReadRepository userRepository, IMemoryCache cache)
    {
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // Check cache first (read-side optimization)
        var cacheKey = $"user:{request.UserId}";
        if (_cache.TryGetValue(cacheKey, out UserDto cachedUser))
        {
            return cachedUser;
        }

        // Fetch from read-optimized data source
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        var userDto = user.ToDto(); // Use extension method for mapping

        // Cache the result
        _cache.Set(cacheKey, userDto, TimeSpan.FromMinutes(5));

        return userDto;
    }
}

public class GetUsersHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IUserReadRepository _userRepository;

    public GetUsersHandler(IUserReadRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Use read-optimised repository with specialised query methods
        var users = await _userRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.IncludeInactive);

        // Use extension method for mapping to paginated DTO
        return users.Items.ToPagedDto(users.TotalCount, request.Page, request.PageSize);
    }
}

// Analytical query handler - can use different data source
public class GetUserStatisticsHandler : IRequestHandler<GetUserStatisticsQuery, UserStatisticsDto>
{
    private readonly IAnalyticsRepository _analyticsRepository; // Specialised analytics data source
    private readonly ILogger<GetUserStatisticsHandler> _logger;

    public GetUserStatisticsHandler(IAnalyticsRepository analyticsRepository, ILogger<GetUserStatisticsHandler> logger)
    {
        _analyticsRepository = analyticsRepository;
        _logger = logger;
    }

    public async Task<UserStatisticsDto> Handle(GetUserStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating statistics for user {UserId}", request.UserId);

        // Can use materialized views, data warehouse, or other read-optimized sources
        return await _analyticsRepository.GetUserStatisticsAsync(request.UserId, request.MonthsBack);
    }
}
```

### Command Handlers (CQRS Write Side)

Command handlers focus on business logic, validation, and state changes. They use write-optimized repositories and domain models.

```csharp
public class CreateUserHandler : IRequestHandler<CreateUserCommand>
{
    private readonly IUserWriteRepository _userRepository; // Write-optimized repository
    private readonly IValidator<CreateUserCommand> _validator;
    private readonly IDomainEventDispatcher _eventDispatcher; // For domain events
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(
        IUserWriteRepository userRepository,
        IValidator<CreateUserCommand> validator,
        IDomainEventDispatcher eventDispatcher,
        ILogger<CreateUserHandler> logger)
    {
        _userRepository = userRepository;
        _validator = validator;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Business validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        _logger.LogInformation("Creating user with email {Email}", request.Email);

        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            _logger.LogWarning("Attempted to create user with existing email {Email}", request.Email);
            throw new ConflictException($"User with email {request.Email} already exists");
        }

        // Create domain entity with business logic
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            CreatedAt = DateTime.UtcNow
        };

        // Save using write-optimized repository
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Dispatch domain events (CQRS often uses event sourcing)
        await _eventDispatcher.DispatchAsync(new UserCreatedEvent(user.Id, user.Email));

        _logger.LogInformation("User created successfully with ID {UserId}", user.Id);
    }
}

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IUserWriteRepository _userRepository;
    private readonly IValidator<UpdateUserCommand> _validator;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public UpdateUserHandler(
        IUserWriteRepository userRepository,
        IValidator<UpdateUserCommand> validator,
        IDomainEventDispatcher eventDispatcher)
    {
        _userRepository = userRepository;
        _validator = validator;
        _eventDispatcher = eventDispatcher;
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Business validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        _logger.LogInformation("Updating user {UserId}", request.UserId);

        // Get existing user
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        // Check if email is being changed and if it already exists
        if (user.Email != request.Email && await _userRepository.EmailExistsAsync(request.Email, request.UserId))
        {
            _logger.LogWarning("Attempted to update user {UserId} with existing email {Email}", request.UserId, request.Email);
            throw new ConflictException($"User with email {request.Email} already exists");
        }

        // Update the user
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.DateOfBirth = request.DateOfBirth;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        // Dispatch domain event
        await _eventDispatcher.DispatchAsync(new UserUpdatedEvent(user.Id));
    }
}

// Domain-specific command handler
public class ActivateUserAccountHandler : IRequestHandler<ActivateUserAccountCommand>
{
    private readonly IUserWriteRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public ActivateUserAccountHandler(
        IUserWriteRepository userRepository,
        ITokenService tokenService,
        IDomainEventDispatcher eventDispatcher)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task Handle(ActivateUserAccountCommand request, CancellationToken cancellationToken)
    {
        // Business logic for activation
        var isValidToken = await _tokenService.ValidateActivationTokenAsync(
            request.UserId,
            request.ActivationToken);

        if (!isValidToken)
            throw new InvalidTokenException("Invalid or expired activation token");

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        // Domain method
        user.ActivateAccount();

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        // Domain event
        await _eventDispatcher.DispatchAsync(new UserAccountActivatedEvent(user.Id));
    }
}
```

### Command Handlers with Return Values

```csharp
public class CreateUserWithIdHandler : IRequestHandler<CreateUserWithIdCommand, int>
{
    private readonly IUserRepository _userRepository;

    public CreateUserWithIdHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<int> Handle(CreateUserWithIdCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return user.Id; // Return the generated ID
    }
}
```

## MediatorStatistics

The `MediatorStatistics` class provides comprehensive analysis and monitoring capabilities for your Blazing.Mediator implementation. This powerful feature allows you to discover all CQRS types in your application, track runtime execution statistics, and gain insights into your mediator usage patterns. Whether you're debugging handler registration issues, monitoring performance, or documenting your application's architecture, MediatorStatistics offers both compact and detailed analysis modes to meet your specific needs.

### Overview

MediatorStatistics offers three main capabilities:

1. **Runtime Statistics** - Track execution counts for queries, commands, and notifications
2. **Query Analysis** - Discover all `IQuery<TResponse>` (`IRequest<TResponse>`) implementations in your application
3. **Command Analysis** - Discover all `ICommand` / (`IRequest`) and `ICommand<TResponse>` (`IRequest<TResponse>`) implementations in your application

**NOTE:** Query and Command analysis methods require that you follow the `xxxQuery`, `xxxCommand`, and `xxxHandler` naming conventions for best results.

### Setup and Registration

The `MediatorStatistics` service is automatically registered when you call `AddMediator()`:

```csharp
services.AddMediator(typeof(MyQuery).Assembly);
// MediatorStatistics is automatically registered with ConsoleStatisticsRenderer
```

You can provide a custom statistics renderer:

```csharp
services.AddSingleton<IStatisticsRenderer, MyCustomRenderer>();
services.AddMediator(typeof(MyQuery).Assembly);
```

### Monitoring Runtime Statistics

The `MediatorStatistics` class automatically tracks execution counts for queries, commands, and notifications. You can access the current statistics at any time:

```csharp
public class StatisticsMonitor
{
    private readonly MediatorStatistics _statistics;

    public StatisticsMonitor(MediatorStatistics statistics)
    {
        _statistics = statistics;
    }

    public void PrintStats()
    {
        // Print current statistics
        _statistics.ReportStatistics();
    }
}
```

#### ReportStatistics

The `ReportStatistics` method displays current runtime statistics:

```csharp
public void ShowCurrentStatistics()
{
    // Display current statistics
    _stats.ReportStatistics();

    // Output example:
    // Mediator Statistics:
    // Queries: 15
    // Commands: 8
    // Notifications: 3
}
```

### Runtime Statistics Tracking

The statistics automatically track execution counts through the following internal methods:

-   **IncrementQuery** - Called automatically when a query is executed
-   **IncrementCommand** - Called automatically when a command is executed
-   **IncrementNotification** - Called automatically when a notification is published

These methods are called internally by the mediator and provide real-time usage tracking.

### Query & Command Analysis Properties

The `QueryCommandAnalysis` record provides comprehensive information about discovered queries and commands in your application:

| Property           | Type                  | Description                                                                     |
| ------------------ | --------------------- | ------------------------------------------------------------------------------- |
| `Type`             | `Type`                | The actual .NET Type being analyzed                                             |
| `ClassName`        | `string`              | The clean class name without generic parameters (e.g., "GetUserQuery")          |
| `TypeParameters`   | `string`              | String representation of generic type parameters (e.g., "<T, U>")               |
| `Assembly`         | `string`              | The name of the assembly containing this type                                   |
| `Namespace`        | `string`              | The namespace of the type (or "Unknown" if null)                                |
| `ResponseType`     | `Type?`               | The response type for queries/commands that return data, null for void commands |
| `PrimaryInterface` | `string`              | The primary interface implemented (IQuery<T>, ICommand, IRequest<T>, etc.)      |
| `IsResultType`     | `bool`                | True if the response type implements IResult interface (ASP.NET Core)           |
| `HandlerStatus`    | `HandlerStatus`       | The status of handlers for this request type (Single, Missing, Multiple)        |
| `HandlerDetails`   | `string`              | Detailed information about the handlers (handler name or error message)         |
| `Handlers`         | `IReadOnlyList<Type>` | List of handler types registered for this request                               |

#### HandlerStatus Enum

| Value      | ASCII Marker | Description                                        |
| ---------- | ------------ | -------------------------------------------------- |
| `Single`   | `+`          | Exactly one handler is registered (ideal state)    |
| `Missing`  | `!`          | No handler is registered for this request type     |
| `Multiple` | `#`          | Multiple handlers are registered (potential issue) |

### Analyzing Queries and Commands

The `MediatorStatistics` class provides convenient methods to analyze queries and commands:

```csharp
public class MyService
{
    private readonly MediatorStatistics _statistics;

    public MyService(MediatorStatistics statistics)
    {
        _statistics = statistics;
    }

    public void Analyze()
    {
        // Analyze queries
        var queryAnalysis = _statistics.AnalyzeQueries();

        // Analyze commands
        var commandAnalysis = _statistics.AnalyzeCommands();
    }
}
```

#### AnalyzeQueries

The `AnalyzeQueries` method scans your application to discover all query implementations. It supports both simple and detailed output modes:

```csharp
public async Task AnalyzeApplicationQueries()
{
	// Simple analysis (default: isDetailed = true)
	var queryAnalysis = _stats.AnalyzeQueries(_serviceProvider);

	// Compact analysis (shows only basic information)
	var compactAnalysis = _stats.AnalyzeQueries(_serviceProvider, isDetailed: false);

	// Detailed analysis (shows all properties)
	var detailedAnalysis = _stats.AnalyzeQueries(_serviceProvider, isDetailed: true);

	Console.WriteLine($"Total Queries Discovered: {queryAnalysis.Count}");

	foreach (var assembly in queryAnalysis.GroupBy(q => q.Assembly))
	{
		Console.WriteLine($"Assembly: {assembly.Key}");

		foreach (var ns in assembly.GroupBy(q => q.Namespace))
		{
			Console.WriteLine($"  Namespace: {ns.Key}");

			foreach (var query in ns)
			{
				Console.WriteLine($"    Query: {query.ClassName}");
				Console.WriteLine($"    Response Type: {query.ResponseType?.Name}");
				Console.WriteLine($"    Full Type: {query.Type.FullName}");
			}
		}
	}
}
```

#### AnalyzeCommands

The `AnalyzeCommands` method discovers all command implementations with the same flexible output options:

```csharp
public async Task AnalyzeApplicationCommands()
{
    // Simple analysis (default: isDetailed = true)
    var commandAnalysis = _stats.AnalyzeCommands(_serviceProvider);

    // Compact analysis (shows only basic information)
    var compactAnalysis = _stats.AnalyzeCommands(_serviceProvider, isDetailed: false);

    // Detailed analysis (shows all properties)
    var detailedAnalysis = _stats.AnalyzeCommands(_serviceProvider, isDetailed: true);

    Console.WriteLine($"Total Commands Discovered: {commandAnalysis.Count}");

    foreach (var assembly in commandAnalysis.GroupBy(c => c.Assembly))
    {
        Console.WriteLine($"Assembly: {assembly.Key}");

        foreach (var ns in assembly.GroupBy(c => c.Namespace))
        {
            Console.WriteLine($"  Namespace: {ns.Key}");

            foreach (var command in ns)
            {
                Console.WriteLine($"    Command: {command.ClassName}");
                Console.WriteLine($"    Response Type: {command.ResponseType?.Name}");
                Console.WriteLine($"    Full Type: {command.Type.FullName}");
            }
        }
    }
}
```

#### Output Modes

The analysis methods support two output modes controlled by the `isDetailed` parameter:

##### Compact Mode (`isDetailed: false`)

Shows essential information in a concise format:

```
* QUERIES DISCOVERED:
  * Assembly: ECommerce.Api
    * Namespace: ECommerce.Api.Application.Queries
      + GetProductQuery : IRequest<ProductDto>
      + GetProductsQuery : IQuery<PagedResult<ProductDto>>
      ! GetCategoriesQuery : IRequest<List<CategoryDto>>

* COMMANDS DISCOVERED:
  * Assembly: ECommerce.Api
    * Namespace: ECommerce.Api.Application.Commands
      + CreateOrderCommand : ICommand<OrderResult>
      + UpdateProductCommand : ICommand
      # DeleteProductCommand : IRequest
```

##### Detailed Mode (`isDetailed: true` - Default)

Shows comprehensive information with all properties:

```
* QUERIES DISCOVERED:
  * Assembly: ECommerce.Api
    * Namespace: ECommerce.Api.Application.Queries
      + GetProductQuery : IRequest<ProductDto>
        │ Type:        ECommerce.Api.Application.Queries.GetProductQuery
        │ Returns:     ProductDto
        │ Handler:     GetProductQueryHandler
        │ Status:      Single
        │ Assembly:    ECommerce.Api
        │ Namespace:   ECommerce.Api.Application.Queries
        │ Handler(s):  1 registered
        └─ Result Type: NO (standard type)

      + GetProductsQuery : IQuery<PagedResult<ProductDto>>
        │ Type:        ECommerce.Api.Application.Queries.GetProductsQuery
        │ Returns:     PagedResult<ProductDto> (IResult)
        │ Handler:     GetProductsQueryHandler
        │ Status:      Single
        │ Assembly:    ECommerce.Api
        │ Namespace:   ECommerce.Api.Application.Queries
        │ Handler(s):  1 registered
        └─ Result Type: YES (implements IResult)

      ! GetCategoriesQuery : IRequest<List<CategoryDto>>
        │ Type:        ECommerce.Api.Application.Queries.GetCategoriesQuery
        │ Returns:     List<CategoryDto>
        │ Handler:     No handler registered
        │ Status:      Missing
        │ Assembly:    ECommerce.Api
        │ Namespace:   ECommerce.Api.Application.Queries
        │ Handler(s):  0 registered
        └─ Result Type: NO (standard type)

* COMMANDS DISCOVERED:
  * Assembly: ECommerce.Api
    * Namespace: ECommerce.Api.Application.Commands
      + CreateOrderCommand : ICommand<OrderResult>
        │ Type:        ECommerce.Api.Application.Commands.CreateOrderCommand
        │ Returns:     OrderResult
        │ Handler:     CreateOrderCommandHandler
        │ Status:      Single
        │ Assembly:    ECommerce.Api
        │ Namespace:   ECommerce.Api.Application.Commands
        │ Handler(s):  1 registered
        └─ Result Type: NO (standard type)

      + UpdateProductCommand : ICommand
        │ Type:        ECommerce.Api.Application.Commands.UpdateProductCommand
        │ Returns:     void
        │ Handler:     UpdateProductCommandHandler
        │ Status:      Single
        │ Assembly:    ECommerce.Api
        │ Namespace:   ECommerce.Api.Application.Commands
        │ Handler(s):  1 registered
        └─ Result Type: NO (standard type)

      # DeleteProductCommand : IRequest
        │ Type:        ECommerce.Api.Application.Commands.DeleteProductCommand
        │ Returns:     void
        │ Handler:     2 handlers: DeleteProductHandler, AuditDeleteProductHandler
        │ Status:      Multiple
        │ Assembly:    ECommerce.Api
        │ Namespace:   ECommerce.Api.Application.Commands
        │ All Types:   [DeleteProductHandler, AuditDeleteProductHandler]
        │ Handler(s):  2 registered
        └─ Result Type: NO (standard type)

LEGEND:
  + = Handler found (Single)    ! = No handler (Missing)    # = Multiple handlers
  │ = Property details          └─ = Additional information
===============================================
```

## Validation and Error Handling

Proper validation and error handling are crucial for robust applications. Here are common patterns used with Blazing.Mediator:

### Input Validation

#### Using FluentValidation

```csharp
// Install FluentValidation
// <PackageReference Include="FluentValidation" Version="11.8.0" />

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid");
    }
}

// Handler with validation
public class CreateUserHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly IValidator<CreateUserCommand> _validator;
    private readonly IUserRepository _userRepository;

    public CreateUserHandler(IValidator<CreateUserCommand> validator, IUserRepository userRepository)
    {
        _validator = validator;
        _userRepository = userRepository;
    }

    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Process the command
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        return user.Id;
    }
}
```

### Custom Exception Handling

```csharp
// Custom exceptions
public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures have occurred.")
    {
        Errors = failures;
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

// Global exception handling middleware
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException validationEx => new
            {
                StatusCode = 400,
                Message = "Validation failed",
                Errors = validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            },
            NotFoundException notFoundEx => new
            {
                StatusCode = 404,
                Message = notFoundEx.Message
            },
            _ => new
            {
                StatusCode = 500,
                Message = "An error occurred while processing your request"
            }
        };

        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

## Testing Strategies

Testing handlers is straightforward because they're isolated and have clear dependencies. We prefer to use **Shouldly** for assertions as it provides more readable and expressive test assertions.

### Unit Testing Handlers

```csharp
[Test]
public async Task Handle_ValidRequest_ReturnsUser()
{
    // Arrange
    var mockRepository = new Mock<IUserRepository>();
    var expectedUser = new User { Id = 1, FirstName = "John", LastName = "Doe" };

    mockRepository.Setup(r => r.GetByIdAsync(1))
              .ReturnsAsync(expectedUser);

    var handler = new GetUserHandler(mockRepository.Object);
    var query = new GetUserByIdQuery { UserId = 1 };

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert using Shouldly for more expressive assertions
    result.Id.ShouldBe(1);
    result.FirstName.ShouldBe("John");
    mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
}
```

### Integration Testing with TestServer

```csharp
public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UsersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Test]
    public async Task GetUser_ExistingUser_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/users/1");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserDto>(content);

        user.Id.ShouldBe(1); // Using Shouldly for cleaner assertions
    }
}
```

## Usage in APIs

The Blazing.Mediator library works seamlessly with both modern Minimal APIs and traditional MVC Controllers. This section demonstrates both approaches to show the flexibility of the mediator pattern.

### Minimal API Usage (Modern Approach)

Minimal APIs provide a lightweight, functional approach to building HTTP APIs. This example shows how the UserManagement.Api sample implements CQRS with minimal APIs.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Mediator (same registration regardless of API style)
builder.Services.AddMediator(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Define API endpoints using minimal API style
var api = app.MapGroup("/api/users").WithTags("Users");

api.MapGet("/{id}", async (int id, IMediator mediator) =>
{
    try
    {
        var query = new GetUserByIdQuery { UserId = id };
        var user = await mediator.Send(query);
        return Results.Ok(user);
    }
    catch (NotFoundException)
    {
        return Results.NotFound();
    }
})
.WithName("GetUser")
.Produces<UserDto>()
.Produces(404);

api.MapGet("/", async (
    IMediator mediator,
    int page = 1,
    int pageSize = 10,
    string searchTerm = "",
    bool includeInactive = false) =>
{
    var query = new GetUsersQuery
    {
        Page = page,
        PageSize = pageSize,
        SearchTerm = searchTerm,
        IncludeInactive = includeInactive
    };

    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("GetUsers")
.Produces<PagedResult<UserDto>>();

// Command endpoints (CQRS writes)
api.MapPost("/", async (CreateUserCommand command, IMediator mediator) =>
{
    try
    {
        await mediator.Send(command);
        return Results.Created("/api/users/0", null);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(ex.Errors.Select(e => e.ErrorMessage));
    }
    catch (ConflictException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
})
.WithName("CreateUser")
.Accepts<CreateUserCommand>("application/json")
.Produces(201)
.Produces(400);

app.Run();
```

### Controller Usage (Traditional Approach)

Controllers provide a more structured, object-oriented approach. This example shows how the ECommerce.Api sample implements CQRS with traditional controllers.

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var query = new GetUserByIdQuery { UserId = id };
            var user = await _mediator.Send(query);
            return Ok(user);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = null)
    {
        var query = new GetUsersQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return CreatedAtAction(nameof(GetUser), new { id = 0 }, null);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserCommand command)
    {
        if (id != command.UserId)
            return BadRequest("ID mismatch");

        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var command = new DeleteUserCommand { UserId = id };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
```

### Controller with Error Handling

Both approaches support comprehensive error handling. Here's an advanced controller example from the ECommerce.Api sample:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder([FromBody] CreateOrderCommand command)
    {
        try
        {
            _logger.LogInformation("Creating order for user {UserId}", command.UserId);

            var response = await _mediator.Send(command);

            _logger.LogInformation("Order {OrderId} created successfully", response.OrderId);

            return CreatedAtAction(nameof(GetOrder), new { id = response.OrderId }, response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for create order: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(ex.Errors);
        }
        catch (InsufficientStockException ex)
        {
            _logger.LogWarning("Insufficient stock for order: {Message}", ex.Message);
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating order");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        try
        {
            var query = new GetOrderByIdQuery { OrderId = id };
            var order = await mediator.Send(query);
            return Ok(order);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
```

## Middleware Pipeline

The middleware pipeline in Blazing.Mediator provides a powerful way to add cross-cutting concerns like logging, validation, and caching, and authorization to your CQRS implementation without modifying your core business logic.

### Overview

Middleware components execute before and after your request handlers, providing a clean separation of concerns. The pipeline is completely optional - use it only when needed.

#### Key Middleware Features

-   ✅ **Optional**: Middleware is completely optional - use it only when needed
-   ✅ **Type-Safe**: Full generic type support with compile-time checking
-   ✅ **Ordered Execution**: Control middleware execution order with priorities
-   ✅ **Conditional**: Execute middleware only for specific request types
-   ✅ **Composable**: Chain multiple middleware components together
-   ✅ **Async Support**: Full async/await support throughout the pipeline
-   ✅ **Pipeline Inspection**: `IMiddlewarePipelineInspector` interface for debugging and monitoring middleware execution
-   ✅ **Full DI Support**: Complete dependency injection support for middleware components

### Middleware Types

There are two main types of middleware in Blazing.Mediator:

#### 1. Standard Middleware (IRequestMiddleware)

Standard middleware executes for **all requests** that match its generic type signature.

```csharp
// Query middleware - executes for all queries
public class GeneralLoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<GeneralLoggingMiddleware<TRequest, TResponse>> _logger;

    public GeneralLoggingMiddleware(ILogger<GeneralLoggingMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public int Order => 0; // Execution order (lower numbers execute first)

    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var startTime = DateTime.UtcNow;

        // Pre-processing logic
        _logger.LogInformation("🔍 REQUEST: {RequestType} started at {StartTime}",
            requestType, startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

        try
        {
            // Call next middleware or handler
            var response = await next();

            // Post-processing logic
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            _logger.LogInformation("🔍 RESPONSE: {RequestType} completed successfully in {Duration}ms",
                requestType, duration.TotalMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            _logger.LogError(ex, "🔍 ERROR: {RequestType} failed after {Duration}ms",
                requestType, duration.TotalMilliseconds);
            throw;
        }
    }
}

// Command middleware - executes for all commands
public class GeneralCommandLoggingMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    private readonly ILogger<GeneralCommandLoggingMiddleware<TRequest>> _logger;

    public GeneralCommandLoggingMiddleware(ILogger<GeneralCommandLoggingMiddleware<TRequest>> logger)
    {
        _logger = logger;
    }

    public int Order => 0;

    public async Task HandleAsync(
        TRequest request,
        RequestHandlerDelegate next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var startTime = DateTime.UtcNow;

        _logger.LogInformation("🔍 COMMAND: {RequestType} started at {StartTime}",
            requestType, startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

        try
        {
            await next();

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            _logger.LogInformation("🔍 COMMAND COMPLETED: {RequestType} completed successfully in {Duration}ms",
                requestType, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            _logger.LogError(ex, "🔍 COMMAND ERROR: {RequestType} failed after {Duration}ms",
                requestType, duration.TotalMilliseconds);
            throw;
        }
    }
}
```

#### 2. Conditional Middleware (IConditionalMiddleware)

Conditional middleware executes **only when** the `ShouldExecute` method returns `true`. This is perfect for performance optimization when you only want middleware to run for specific request types.

```csharp
// Order-specific logging middleware
public class OrderLoggingMiddleware<TRequest, TResponse> : IConditionalMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<OrderLoggingMiddleware<TRequest, TResponse>> _logger;

    public OrderLoggingMiddleware(ILogger<OrderLoggingMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public int Order => 1; // Execute after general logging middleware

    public bool ShouldExecute(TRequest request)
    {
        // Only execute for order-related requests
        var requestType = request.GetType().Name;
        return requestType.Contains("Order", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType().Name;
        var startTime = DateTime.UtcNow;

        // Log the request
        _logger.LogInformation("🛒 ORDER REQUEST: {RequestType} started at {StartTime}",
            requestType, startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

        try
        {
            var response = await next();

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Log successful response
            _logger.LogInformation("🛒 ORDER RESPONSE: {RequestType} completed successfully in {Duration}ms",
                requestType, duration.TotalMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Log error
            _logger.LogError(ex, "🛒 ORDER ERROR: {RequestType} failed after {Duration}ms",
                requestType, duration.TotalMilliseconds);

            throw;
        }
    }
}

// Product-specific logging middleware
public class ProductLoggingMiddleware<TRequest, TResponse> : IConditionalMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ProductLoggingMiddleware<TRequest, TResponse>> _logger;

    public ProductLoggingMiddleware(ILogger<ProductLoggingMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public int Order => 2; // Executes after order middleware

    public bool ShouldExecute(TRequest request)
    {
        // Only execute for product-related requests
        var requestType = request.GetType().Name;
        return requestType.Contains("Product", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType().Name;
        var startTime = DateTime.UtcNow;

        _logger.LogInformation("📦 PRODUCT REQUEST: {RequestType} started at {StartTime}",
            requestType, startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

        try
        {
            var response = await next();

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            _logger.LogInformation("📦 PRODUCT RESPONSE: {RequestType} completed successfully in {Duration}ms",
                requestType, duration.TotalMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            _logger.LogError(ex, "📦 PRODUCT ERROR: {RequestType} failed after {Duration}ms",
                requestType, duration.TotalMilliseconds);

            throw;
        }
    }
}
```

### Pipeline Flow

The middleware pipeline executes in a specific order, wrapping around your request handlers. Here's a detailed flow diagram that shows how requests move through the middleware pipeline, including exception handling:

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                        Middleware Pipeline Flow                                 │
│                     (with Exception Handling)                                   │
└─────────────────────────────────────────────────────────────────────────────────┘

📥 Incoming Request
    │
    ├─── 🔧 Middleware 1 (Order: 0) ─── LoggingMiddleware
    │    │                              ├─ Pre: Log request start
    │    │                              ├─ Execution: await next()
    │    │                              └─ Post: Log request completion/error
    │    │
    │    ├─── 🔧 Middleware 2 (Order: 1) ─── ValidationMiddleware
    │    │    │                              ├─ Pre: Validate request
    │    │    │                              ├─ Execution: await next()
    │    │    │                              └─ Post: Handle validation errors
    │    │    │
    │    │    ├─── 🔧 Middleware 3 (Order: 2) ─── CachingMiddleware (Conditional)
    │    │    │    │                              ├─ Pre: Check cache
    │    │    │    │                              ├─ Execution: await next() (if not cached)
    │    │    │    │                              └─ Post: Store in cache
    │    │    │    │
    │    │    │    └─── 🎯 REQUEST HANDLER ─── Business Logic
    │    │    │         │                      ├─ Query: Read data
    │    │    │         │                      ├─ Command: Write data
    │    │    │         │                      └─ Return response/void
    │    │    │         │
    │    │    │    ┌─── 📤 SUCCESS RESPONSE
    │    │    │    │    │
    │    │    │    │    ├─ Cache response (if applicable)
    │    │    │    │    ├─ Log completion
    │    │    │    │    └─ Return to client
    │    │    │    │
    │    │    │    └─── ❌ EXCEPTION PATH
    │    │    │         │
    │    │    │         ├─ Middleware 3: Handle/log cache errors
    │    │    │         │               └─ throw; (preserve stack trace)
    │    │    │         │
    │    │    │         ├─ Middleware 2: Handle validation errors
    │    │    │         │               ├─ Transform ValidationException
    │    │    │         │               └─ throw; (preserve stack trace)
    │    │    │         │
    │    │    │         └─ Middleware 1: Log all errors
    │    │    │                         ├─ Log error details
    │    │    │                         ├─ Log execution time
    │    │    │                         └─ throw; (preserve stack trace)
    │    │    │
    │    │    └─── 🔄 UNWIND STACK (Post-processing in reverse order)
    │    │
    │    └─── 🔄 UNWIND STACK (Post-processing in reverse order)
    │
    └─── 📤 Final Response or Exception to Client

═══════════════════════════════════════════════════════════════════════════════════

Key Pipeline Characteristics:

✅ Execution Order: Lower Order values execute first (outer middleware)
✅ Exception Flow: Exceptions bubble up through middleware in reverse order
✅ Post-Processing: Happens in reverse order (LIFO - Last In, First Out)
✅ Conditional Execution: Middleware can skip execution based on request type
✅ Error Preservation: Use throw; to preserve original stack traces
✅ Pipeline Inspection: Use IMiddlewarePipelineInspector for debugging

Example Exception Flow:
┌─ Handler throws ValidationException
├─ CachingMiddleware: Skips caching, lets exception bubble up
├─ ValidationMiddleware: Catches ValidationException, transforms to HTTP 400
├─ LoggingMiddleware: Logs error details and timing
└─ Exception returned to client as proper HTTP response

═══════════════════════════════════════════════════════════════════════════════════
```

### Simplified Flow Summary

For a basic understanding, the middleware pipeline follows this pattern:

```
📥 Request
    ↓
🔧 Middleware 1 (Order: 0) - Pre-processing
    ↓
🔧 Middleware 2 (Order: 1) - Pre-processing
    ↓
🔧 Middleware 3 (Order: 2) - Pre-processing
    ↓
🎯 Request Handler - Business Logic
    ↓
🔧 Middleware 3 (Order: 2) - Post-processing
    ↓
🔧 Middleware 2 (Order: 1) - Post-processing
    ↓
🔧 Middleware 1 (Order: 0) - Post-processing
    ↓
📤 Response
```

### Configuration

#### Basic Configuration (No Middleware)

```csharp
// Program.cs - No middleware
builder.Services.AddMediator(typeof(Program).Assembly);
```

#### Standard Middleware Configuration

```csharp
// Program.cs - Standard middleware for all requests
builder.Services.AddMediator(config =>
{
    // Add standard middleware that logs all requests
    config.AddMiddleware<GeneralLoggingMiddleware<,>>();
    config.AddMiddleware<GeneralCommandLoggingMiddleware<>>();
}, typeof(Program).Assembly);
```

#### Conditional Middleware Configuration

```csharp
// Program.cs - Conditional middleware for performance
builder.Services.AddMediator(config =>
{
    // Add conditional middleware - only logs specific request types
    config.AddMiddleware<OrderLoggingMiddleware<,>>();
    config.AddMiddleware<ProductLoggingMiddleware<,>>();
}, typeof(Program).Assembly);
```

#### Advanced Configuration with Multiple Middleware Types

```csharp
// Program.cs - Mixed middleware approach
builder.Services.AddMediator(config =>
{
    // Global validation middleware (standard)
    config.AddMiddleware<ValidationMiddleware<,>>();

    // Conditional logging for performance
    config.AddMiddleware<OrderLoggingMiddleware<,>>();
    config.AddMiddleware<ProductLoggingMiddleware<,>>();

    // Global caching middleware (standard)
    config.AddMiddleware<CachingMiddleware<,>>();
}, typeof(Program).Assembly);
```

#### Auto-Discovery Middleware Configuration

Blazing.Mediator supports automatic middleware discovery to simplify configuration and reduce boilerplate code. Instead of manually registering each middleware type, you can enable auto-discovery to automatically find and register all middleware implementations in the specified assemblies.

##### Basic Auto-Discovery (All Middleware)

```csharp
// Program.cs - Auto-discover all middleware in the current assembly
builder.Services.AddMediator(typeof(Program).Assembly, discoverMiddleware: true);

// Even simpler - auto-discover from calling assembly
builder.Services.AddMediatorFromCallingAssembly(discoverMiddleware: true);
```

##### Granular Auto-Discovery (New in v1.6.0)

Starting with v1.6.0, you can separately control auto-discovery for request middleware and notification middleware:

```csharp
// Program.cs - Separate control over middleware auto-discovery
builder.Services.AddMediator(
    configureMiddleware: null,
    discoverMiddleware: true,             // Auto-discover request middleware
    discoverNotificationMiddleware: false, // Manual registration for notification middleware
    typeof(Program).Assembly
);

// Or use the dedicated notification middleware method
builder.Services.AddMediatorWithNotificationMiddleware(
    discoverNotificationMiddleware: true,
    typeof(Program).Assembly
);
```

##### Auto-Discovery with Multiple Assemblies

```csharp
// Program.cs - Auto-discover middleware from multiple assemblies
builder.Services.AddMediator(
    discoverMiddleware: true,
    typeof(Program).Assembly,                    // Current assembly (API)
    typeof(GetUserHandler).Assembly,             // Application layer
    typeof(LoggingMiddleware<,>).Assembly        // Infrastructure layer
);
```

##### Auto-Discovery with Manual Configuration

```csharp
// Program.cs - Mix auto-discovery with manual configuration
builder.Services.AddMediator(config =>
{
    // Manually add specific middleware with custom configuration
    config.AddMiddleware<CustomAuthorizationMiddleware<,>>();

    // Or add middleware that requires special setup
    config.AddMiddleware<DatabaseTransactionMiddleware<,>>();
},
discoverMiddleware: true, // Auto-discover other middleware
typeof(Program).Assembly);
```

##### Auto-Discovery Best Practices

**✅ Automatic Order Detection**: Auto-discovery respects middleware ordering through multiple mechanisms:

```csharp
// Method 1: Static Order property (recommended)
public class LoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static int Order => 1; // Static property for compile-time order

    // Implementation...
}

// Method 2: Instance Order property (fallback)
public class ValidationMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 5; // Instance property for runtime order

    // Implementation...
}

// Method 3: No Order property (uses sequential discovery order)
public class CachingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // No Order property - will be assigned sequential order during discovery

    // Implementation...
}
```

**✅ Discovery Behavior**: Auto-discovery finds all classes implementing:

-   `IRequestMiddleware<TRequest, TResponse>` (for queries with responses)
-   `IRequestMiddleware<TRequest>` (for commands without responses)
-   `IConditionalMiddleware<TRequest, TResponse>` (conditional middleware for queries)
-   `IConditionalMiddleware<TRequest>` (conditional middleware for commands)

**✅ Assembly Scanning**: Only scans the assemblies you specify - no performance impact from scanning all loaded assemblies.

##### Complete Auto-Discovery Example

Here's a complete example showing auto-discovery in action:

```csharp
// Program.cs
using Blazing.Mediator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Auto-discover handlers and middleware from multiple assemblies
builder.Services.AddMediator(
    discoverMiddleware: true, // Enable auto-discovery
    typeof(Program).Assembly,                    // API layer
    typeof(GetUserHandler).Assembly,             // Application layer
    typeof(LoggingMiddleware<,>).Assembly        // Infrastructure layer
);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Example Middleware Auto-Discovered**:

```csharp
// Infrastructure/Middleware/LoggingMiddleware.cs
public class LoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static int Order => 1; // Auto-discovered with order 1

    // Implementation...
}

// Infrastructure/Middleware/ValidationMiddleware.cs
public class ValidationMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 2; // Auto-discovered with order 2

    // Implementation...
}

// Application/Middleware/CachingMiddleware.cs
public class CachingMiddleware<TRequest, TResponse> : IConditionalMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 10; // Auto-discovered with order 10

    public bool ShouldExecute(TRequest request)
    {
        return request.GetType().Name.EndsWith("Query");
    }

    // Implementation...
}
```

**Middleware Execution Order** (auto-discovered):

1. `LoggingMiddleware` (Order: 1)
2. `ValidationMiddleware` (Order: 2)
3. `CachingMiddleware` (Order: 10, conditional)

This auto-discovery approach significantly reduces configuration boilerplate while maintaining full control over middleware ordering and behavior.

### Advanced Middleware Examples

#### Validation Middleware

```csharp
public class ValidationMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ValidationMiddleware<TRequest, TResponse>> _logger;

    public ValidationMiddleware(IServiceProvider serviceProvider, ILogger<ValidationMiddleware<TRequest, TResponse>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public int Order => -1; // Execute early in the pipeline

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Try to get a validator for this request type
        var validatorType = typeof(IValidator<>).MakeGenericType(typeof(TRequest));
        var validator = _serviceProvider.GetService(validatorType) as IValidator;

        if (validator != null)
        {
            _logger.LogDebug("Validating request of type {RequestType}", typeof(TRequest).Name);

            var validationResult = await validator.ValidateAsync(new ValidationContext<TRequest>(request), cancellationToken);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                _logger.LogWarning("Validation failed for {RequestType}: {Errors}",
                    typeof(TRequest).Name, string.Join(", ", errors));

                throw new ValidationException(validationResult.Errors);
            }
        }

        return await next();
    }
}
```

#### Caching Middleware

```csharp
public class CachingMiddleware<TRequest, TResponse> : IConditionalMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingMiddleware<TRequest, TResponse>> _logger;

    public CachingMiddleware(IMemoryCache cache, ILogger<CachingMiddleware<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public int Order => 10; // Execute late in the pipeline

    public bool ShouldExecute(TRequest request)
    {
        // Only execute for query operations (not commands)
        return request is IRequest<TResponse> &&
               request.GetType().Name.EndsWith("Query", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔍 Checking cache for query: {RequestType}", typeof(TRequest).Name);

        var cacheKey = GenerateCacheKey(request);

        // Check cache first
        if (_cache.TryGetValue(cacheKey, out TResponse? cachedResponse))
        {
            _logger.LogInformation("Cache hit for {RequestType}", typeof(TRequest).Name);
            return cachedResponse!;
        }

        // Execute query and cache result
        var response = await next();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(1)
        };

        _cache.Set(cacheKey, response, cacheOptions);
        _logger.LogInformation("Cached response for {RequestType}", typeof(TRequest).Name);

        return response;
    }

    private string GenerateCacheKey(TRequest request)
    {
        var requestType = typeof(TRequest).Name;
        var requestJson = JsonSerializer.Serialize(request);
        var hash = requestJson.GetHashCode();
        return $"{requestType}_{hash}";
    }
}
```

#### Type-Constrained Middleware Registration

```csharp
// Program.cs - Register type-constrained middleware
builder.Services.AddMediator(config =>
{
    // Validation middleware only processes commands (ICommand, ICommand<T>)
    config.AddMiddleware<ValidationMiddleware<>>();
    config.AddMiddleware<ValidationMiddleware<,>>();

    // Caching middleware only processes queries (IQuery<T>)
    config.AddMiddleware<CachingMiddleware<,>>();

    // General middleware processes all requests
    config.AddMiddleware<LoggingMiddleware<,>>();
    config.AddMiddleware<GeneralCommandLoggingMiddleware<>>();

}, typeof(Program).Assembly);
```

#### Benefits of Type-Constrained Middleware

1. **Performance Optimization**: Middleware only executes for appropriate request types
2. **Type Safety**: Compile-time verification that middleware constraints are satisfied
3. **Clear Intent**: Explicit declaration of which request types middleware should process
4. **Reduced Overhead**: No runtime type checking needed - constraints are validated at registration
5. **CQRS Clarity**: Clear separation between command processing and query processing middleware

#### Type Constraint Examples

```csharp
// Command-only middleware
public class AuditMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : ICommand
{
    // Only processes commands - never queries
}

// Query-only middleware
public class QueryMetricsMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IQuery<TResponse>
{
    // Only processes queries - never commands
}

// Specific interface constraint
public class OrderMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IOrderRequest<TResponse>
{
    // Only processes requests implementing IOrderRequest<T>
}

// Multiple constraints
public class ComplexMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : class, IQuery<TResponse>, new()
{
    // Only processes queries that are reference types with parameterless constructor
}
```

#### Complete Type-Constrained Example

Here's a complete example showing how different middleware types work together:

```csharp
// CQRS Command (write operation)
public class CreateOrderCommand : ICommand<int>
{
    public int CustomerId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public string CustomerEmail { get; set; } = string.Empty;
}

// CQRS Query (read operation)
public class GetOrderQuery : IQuery<OrderDto>
{
    public int OrderId { get; set; }
}

// Registration
builder.Services.AddMediator(config =>
{
    // Command processing pipeline
    config.AddMiddleware<ErrorHandlingMiddleware<>>();           // Order: int.MinValue (all requests)
    config.AddMiddleware<ValidationMiddleware<>>();              // Order: 100 (commands only)
    config.AddMiddleware<AuditMiddleware<>>();                   // Order: 200 (commands only)

    // Query processing pipeline
    config.AddMiddleware<ErrorHandlingMiddleware<,>>();          // Order: int.MinValue (all requests)
    config.AddMiddleware<CachingMiddleware<,>>();                // Order: 50 (queries only)
    config.AddMiddleware<QueryMetricsMiddleware<,>>();           // Order: 75 (queries only)

    // General pipeline
    config.AddMiddleware<LoggingMiddleware<,>>();                // Order: 10 (all requests)
    config.AddMiddleware<GeneralCommandLoggingMiddleware<>>();   // Order: 10 (commands only)

}, typeof(Program).Assembly);

// Execution flow for CreateOrderCommand:
// 1. ErrorHandlingMiddleware (int.MinValue) ✅
// 2. LoggingMiddleware (10) ✅
// 3. GeneralCommandLoggingMiddleware (10) ✅ (commands only)
// 4. ValidationMiddleware (100) ✅ (commands only)
// 5. AuditMiddleware (200) ✅ (commands only)
// 6. CreateOrderHandler ✅

// Execution flow for GetOrderQuery:
// 1. ErrorHandlingMiddleware (int.MinValue) ✅
// 2. LoggingMiddleware (10) ✅
// 3. CachingMiddleware (50) ✅ (queries only)
// 4. QueryMetricsMiddleware (75) ✅ (queries only)
// 5. GetOrderHandler ✅

// Note: ValidationMiddleware and AuditMiddleware are SKIPPED for queries
// Note: CachingMiddleware and QueryMetricsMiddleware are SKIPPED for commands
```

This approach provides significant performance benefits by avoiding unnecessary middleware execution while maintaining clear separation of concerns between command and query processing pipelines.

### Middleware Best Practices

#### 1. Performance Considerations

-   **Use Conditional Middleware**: For performance-critical applications, use conditional middleware to avoid unnecessary processing
-   **Order Matters**: Put expensive middleware later in the pipeline (higher Order values)
-   **Async All The Way**: Always use async/await in middleware

#### 2. Error Handling

-   **Let Exceptions Bubble**: Don't catch exceptions unless you're handling them specifically
-   **Log Errors**: Always log errors with sufficient context
-   **Preserve Stack Traces**: Use `throw;` instead of `throw ex;`

#### 3. Logging Guidelines

-   **Use Structured Logging**: Include relevant properties in log messages
-   **Be Mindful of Sensitive Data**: Don't log passwords, tokens, or personal information
-   **Use Log Levels Appropriately**: Information for normal flow, Warning for business issues, Error for exceptions

### Pipeline Debugging and Monitoring

Blazing.Mediator provides comprehensive debugging and monitoring capabilities through the `IMiddlewarePipelineInspector` interface. This is essential for understanding middleware execution order, troubleshooting pipeline issues, and monitoring performance.

#### Pipeline Inspector Interface

The `IMiddlewarePipelineInspector` interface provides several methods for inspecting the middleware pipeline:

```csharp
public interface IMiddlewarePipelineInspector
{
    // Get basic middleware types
    IReadOnlyList<Type> GetRegisteredMiddleware();

    // Get middleware with configuration
    IReadOnlyList<(Type Type, object? Configuration)> GetMiddlewareConfiguration();

    // Get detailed middleware info with order values
    IReadOnlyList<(Type Type, int Order, object? Configuration)> GetDetailedMiddlewareInfo(IServiceProvider? serviceProvider = null);

    // NEW: Advanced middleware analysis
    IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider);
}
```

#### Using AnalyzeMiddleware for Advanced Debugging

The new `AnalyzeMiddleware` method provides the most comprehensive pipeline analysis, returning detailed information about each middleware component:

```csharp
public class DebugService
{
    private readonly IMiddlewarePipelineInspector _pipelineInspector;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DebugService> _logger;

    public DebugService(
        IMiddlewarePipelineInspector pipelineInspector,
        IServiceProvider serviceProvider,
        ILogger<DebugService> logger)
    {
        _pipelineInspector = pipelineInspector;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void AnalyzePipeline()
    {
        // Get detailed middleware analysis
        var middlewareAnalysis = _pipelineInspector.AnalyzeMiddleware(_serviceProvider);

        _logger.LogInformation("📊 Middleware Pipeline Analysis");
        _logger.LogInformation("═══════════════════════════════════════");

        foreach (var middleware in middlewareAnalysis)
        {
            _logger.LogInformation(
                "🔧 {Order,5} | {ClassName}{TypeParameters}",
                middleware.OrderDisplay,
                middleware.ClassName,
                middleware.TypeParameters);
        }

        _logger.LogInformation("═══════════════════════════════════════");
        _logger.LogInformation("📈 Total middleware components: {Count}", middlewareAnalysis.Count);
    }
}
```

## Sample Projects

The library includes eight comprehensive sample projects demonstrating different approaches:

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

## Complete Examples

This section provides complete, runnable examples that demonstrate various aspects of Blazing.Mediator implementation. These examples are designed to be copied and adapted for your own projects.

### Complete CRUD API with Validation

Here's a complete example of a User Management API with full CRUD operations, validation, and error handling:

#### 1. Domain Models and DTOs

```csharp
// Domain/User.cs
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

// DTOs/UserDto.cs
public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public int Age => DateTime.Now.Year - DateOfBirth.Year;
    public bool IsActive { get; set; }
}

// DTOs/PagedResult.cs
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

#### 2. CQRS Operations

```csharp
// Queries/GetUserByIdQuery.cs
public class GetUserByIdQuery : IRequest<UserDto>
{
    public int UserId { get; set; }
}

// Queries/GetUsersQuery.cs
public class GetUsersQuery : IRequest<PagedResult<UserDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool IncludeInactive { get; set; } = false;
}

// Commands/CreateUserCommand.cs
public class CreateUserCommand : IRequest<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

// Commands/UpdateUserCommand.cs
public class UpdateUserCommand : IRequest
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

// Commands/DeleteUserCommand.cs
public class DeleteUserCommand : IRequest
{
    public int UserId { get; set; }
}
```

#### 3. Validation with FluentValidation

```csharp
// Validators/CreateUserCommandValidator.cs
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid")
            .MaximumLength(254).WithMessage("Email cannot exceed 254 characters");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Date of birth cannot be more than 120 years ago");
    }
}

// Validators/UpdateUserCommandValidator.cs
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid")
            .MaximumLength(254).WithMessage("Email cannot exceed 254 characters");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Date of birth cannot be more than 120 years ago");
    }
}
```

#### 4. Handlers Implementation

```csharp
// Handler for GetUserByIdQuery
public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive
        };
    }
}

// Handler for GetUsersQuery
public class GetUsersHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetPagedAsync(request.Page, request.PageSize, request.SearchTerm, request.IncludeInactive);

        return new PagedResult<UserDto>
        {
            Items = users.Items.Select(user => new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                IsActive = user.IsActive
            }).ToList(),
            TotalCount = users.TotalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

// Handler for CreateUserCommand
public class CreateUserHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<CreateUserCommand> _validator;

    public CreateUserHandler(IUserRepository userRepository, IValidator<CreateUserCommand> validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Validate request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        return user.Id;
    }
}

// Handler for UpdateUserCommand
public class UpdateUserHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<UpdateUserCommand> _validator;

    public UpdateUserHandler(IUserRepository userRepository, IValidator<UpdateUserCommand> validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Validate request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.DateOfBirth = request.DateOfBirth;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }
}

// Handler for DeleteUserCommand
public class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        await _userRepository.DeleteAsync(user);
    }
}
```

#### 5. Controller Implementation

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var query = new GetUserByIdQuery { UserId = id };
            var user = await _mediator.Send(query);
            return Ok(user);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = null)
    {
        var query = new GetUsersQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        try
        {
            var userId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetUser), new { id = userId }, null);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserCommand command)
    {
        if (id != command.UserId)
            return BadRequest("ID mismatch");

        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var command = new DeleteUserCommand { UserId = id };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
```

This complete example demonstrates:

-   ✅ **Full CRUD Operations** with proper HTTP status codes
-   ✅ **CQRS Implementation** with clear separation of commands and queries
-   ✅ **Comprehensive Validation** using FluentValidation
-   ✅ **Error Handling** with custom exceptions and proper responses
-   ✅ **Repository Pattern** with Entity Framework Core
-   ✅ **Pagination Support** for efficient data retrieval
-   ✅ **Search Functionality** with filtering capabilities
-   ✅ **API Documentation** with Swagger/OpenAPI
-   ✅ **Dependency Injection** throughout the application
-   ✅ **Clean Architecture** with proper separation of concerns

You can use this example as a foundation for your own applications, adapting the patterns and structure to meet your specific requirements.
