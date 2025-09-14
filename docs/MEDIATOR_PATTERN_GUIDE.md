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
11. [Advanced Scenarios](#advanced-scenarios)
12. [Best Practices](#best-practices)
13. [Common Mistakes](#common-mistakes)
14. [Troubleshooting](#troubleshooting)
15. [Sample Projects](#sample-projects)
16. [Complete Examples](#complete-examples)

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
<PackageReference Include="Blazing.Mediator" Version="1.6.1" />
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

        // Create domain entity with business logic
        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.DateOfBirth);

        // Save using write-optimized repository
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Dispatch domain events (CQRS often uses event sourcing)
        await _eventDispatcher.DispatchAsync(new UserCreatedEvent(user.Id, user.Email));

        _logger.LogInformation("User {UserId} created successfully", user.Id);
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

        // Get domain entity for business operations
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        // Use domain methods that encapsulate business logic
        user.UpdatePersonalInfo(request.FirstName, request.LastName, request.Email);

        // Save changes
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
            Email = request.Email
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

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Mediator
builder.Services.AddMediator(typeof(Program).Assembly);

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Define API endpoints
var api = app.MapGroup("/api/users").WithTags("Users");

// Query endpoints (CQRS reads)
api.MapGet("/{id:int}", async (int id, IMediator mediator) =>
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

    [HttpPost("with-id")]
    public async Task<ActionResult<int>> CreateUserWithId([FromBody] CreateUserWithIdCommand command)
    {
        try
        {
            var userId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetUser), new { id = userId }, userId);
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

            return CreatedAtAction(
                nameof(GetOrder),
                new { id = response.OrderId },
                response);
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

    public int Order => 1; // Execution order

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
            // Serialize and log request details (be careful with sensitive data in production)
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            _logger.LogInformation("🛒 ORDER REQUEST DATA: {RequestData}", requestJson);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("🛒 Could not serialize order request: {Error}", ex.Message);
        }

        TResponse response;
        try
        {
            // Execute the next middleware or handler
            response = await next();

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Log successful response
            _logger.LogInformation("🛒 ORDER RESPONSE: {RequestType} completed successfully in {Duration}ms",
                requestType, duration.TotalMilliseconds);

            try
            {
                var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                _logger.LogInformation("🛒 ORDER RESPONSE DATA: {ResponseData}", responseJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("🛒 Could not serialize order response: {Error}", ex.Message);
            }

            return response;
        }
        catch (Exception ex)
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Log error
            _logger.LogError(ex, "🛒 ORDER ERROR: {RequestType} failed after {Duration}ms with error: {ErrorMessage}",
                requestType, duration.TotalMilliseconds, ex.Message);

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
    typeof(OrderLoggingMiddleware<,>).Assembly,  // Application layer
    typeof(ValidationMiddleware<,>).Assembly     // Infrastructure layer
);

// With granular control (v1.6.0+)
builder.Services.AddMediator(
    discoverMiddleware: true,                     // Auto-discover request middleware
    discoverNotificationMiddleware: true,         // Auto-discover notification middleware
    typeof(Program).Assembly,
    typeof(OrderLoggingMiddleware<,>).Assembly,
    typeof(ValidationMiddleware<,>).Assembly
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
        // Only cache query operations (not commands)
        return request is IRequest<TResponse> &&
               request.GetType().Name.EndsWith("Query", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Generate cache key based on request type and properties
        var cacheKey = GenerateCacheKey(request);

        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out TResponse? cachedResponse))
        {
            _logger.LogInformation("Cache hit for {RequestType}", typeof(TRequest).Name);
            return cachedResponse!;
        }

        // Execute handler and cache the result
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
        // Simple cache key generation - improve this for production use
        var requestType = typeof(TRequest).Name;
        var requestJson = JsonSerializer.Serialize(request);
        var hash = requestJson.GetHashCode();
        return $"{requestType}_{hash}";
    }
}
```

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

#### MiddlewareAnalysis Properties

The `MiddlewareAnalysis` record provides detailed information about each middleware component:

```csharp
public record MiddlewareAnalysis(
    Type Type,                  // The full middleware type
    int Order,                  // Numeric execution order
    string OrderDisplay,        // Formatted order string (e.g., "int.MinValue", "100")
    string ClassName,           // Clean class name without generic suffixes
    string TypeParameters,      // Generic type parameters (e.g., "<TRequest, TResponse>")
    object? Configuration       // Optional configuration object
);
```

#### Example Output

```
📊 Middleware Pipeline Analysis
═══════════════════════════════════════
🔧 int.MinValue | ErrorHandlingMiddleware<TRequest, TResponse>
🔧    -1 | ValidationMiddleware<TRequest, TResponse>
🔧     0 | LoggingMiddleware<TRequest, TResponse>
🔧     1 | MetricsMiddleware<TRequest, TResponse>
🔧    10 | CachingMiddleware<TRequest, TResponse>
🔧 int.MaxValue | FinalProcessingMiddleware<TRequest, TResponse>
═══════════════════════════════════════
📈 Total middleware components: 6
```

#### Accessing the Pipeline Inspector

The pipeline inspector is automatically registered when you add Blazing.Mediator to your services:

```csharp
// In your service constructor
public class MyService
{
    public MyService(IMiddlewarePipelineInspector pipelineInspector)
    {
        // Inspector is automatically available
    }
}

// Or resolve it directly from the service provider
var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
```

#### Best Practices for Pipeline Debugging

1. **Use in Development**: Leverage pipeline analysis during development to understand middleware execution
2. **Monitoring Integration**: Include pipeline analysis in health checks and monitoring dashboards
3. **Performance Tracking**: Monitor middleware order and execution for performance optimization
4. **Troubleshooting**: Use when debugging unexpected middleware behavior or execution order
5. **Documentation**: Generate pipeline documentation automatically using the analysis output

## MediatorStatistics

The `MediatorStatistics` class provides comprehensive analysis and monitoring capabilities for your Blazing.Mediator implementation. This powerful feature allows you to discover all CQRS types in your application and track runtime execution statistics.

### Overview

MediatorStatistics offers three main capabilities:

1. **Query Analysis** - Discover all `IQuery<TResponse>` implementations in your application
2. **Command Analysis** - Discover all `ICommand` and `ICommand<TResponse>` implementations in your application
3. **Runtime Statistics** - Track execution counts for queries, commands, and notifications

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

### Core Features

#### AnalyzeQueries

The `AnalyzeQueries` method scans your application to discover all query implementations:

```csharp
public class MediatorAnalysisService
{
    private readonly MediatorStatistics _stats;
    private readonly IServiceProvider _serviceProvider;

    public MediatorAnalysisService(MediatorStatistics stats, IServiceProvider serviceProvider)
    {
        _stats = stats;
        _serviceProvider = serviceProvider;
    }

    public async Task AnalyzeApplicationQueries()
    {
        var queryAnalysis = _stats.AnalyzeQueries(_serviceProvider);

        Console.WriteLine($"Total Queries Discovered: {queryAnalysis.TotalQueries}");

        foreach (var assembly in queryAnalysis.QueriesByAssembly)
        {
            Console.WriteLine($"Assembly: {assembly.Assembly}");

            foreach (var ns in assembly.Namespaces)
            {
                Console.WriteLine($"  Namespace: {ns.Namespace}");

                foreach (var query in ns.Queries)
                {
                    Console.WriteLine($"    Query: {query.ClassName}");
                    Console.WriteLine($"    Response Type: {query.ResponseType}");
                    Console.WriteLine($"    Full Type: {query.FullTypeName}");
                }
            }
        }
    }
}
```

#### AnalyzeCommands

The `AnalyzeCommands` method discovers all command implementations:

```csharp
public async Task AnalyzeApplicationCommands()
{
    var commandAnalysis = _stats.AnalyzeCommands(_serviceProvider);

    Console.WriteLine($"Total Commands Discovered: {commandAnalysis.TotalCommands}");

    foreach (var assembly in commandAnalysis.CommandsByAssembly)
    {
        Console.WriteLine($"Assembly: {assembly.Assembly}");

        foreach (var ns in assembly.Namespaces)
        {
            Console.WriteLine($"  Namespace: {ns.Namespace}");

            foreach (var command in ns.Commands)
            {
                Console.WriteLine($"    Command: {command.ClassName}");
                Console.WriteLine($"    Response Type: {command.ResponseType}");
                Console.WriteLine($"    Full Type: {command.FullTypeName}");
            }
        }
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

### Example Output

#### Query Analysis Output

```
🔍 QUERIES DISCOVERED:
  📦 Assembly: ECommerce.Api
    📁 ECommerce.Api.Application.Queries
      🔍 GetProductsQuery<TResponse> → PagedResult<ProductDto>
      🔍 GetProductByIdQuery → ProductDto
    📁 ECommerce.Api.Features.Orders
      🔍 GetOrderHistoryQuery → List<OrderDto>
```

#### Command Analysis Output

```
⚡ COMMANDS DISCOVERED:
  📦 Assembly: ECommerce.Api
    📁 ECommerce.Api.Application.Commands
      ⚡ CreateProductCommand → Guid
      ⚡ UpdateProductCommand → void
    📁 ECommerce.Api.Features.Orders
      ⚡ ProcessOrderCommand → OrderResult
```

### Practical Usage Examples

#### Health Check Integration

```csharp
public class MediatorHealthCheck : IHealthCheck
{
    private readonly MediatorStatistics _stats;
    private readonly IServiceProvider _serviceProvider;

    public MediatorHealthCheck(MediatorStatistics stats, IServiceProvider serviceProvider)
    {
        _stats = stats;
        _serviceProvider = serviceProvider;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var queries = _stats.AnalyzeQueries(_serviceProvider);
            var commands = _stats.AnalyzeCommands(_serviceProvider);

            var data = new Dictionary<string, object>
            {
                ["TotalQueries"] = queries.TotalQueries,
                ["TotalCommands"] = commands.TotalCommands,
                ["QueriesExecuted"] = _stats.QueryCount,
                ["CommandsExecuted"] = _stats.CommandCount,
                ["NotificationsPublished"] = _stats.NotificationCount
            };

            return Task.FromResult(HealthCheckResult.Healthy("Mediator is healthy", data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Mediator health check failed", ex));
        }
    }
}
```

#### API Endpoints for Monitoring

```csharp
[ApiController]
[Route("api/[controller]")]
public class MediatorAnalysisController : ControllerBase
{
    private readonly MediatorStatistics _stats;
    private readonly IServiceProvider _serviceProvider;

    public MediatorAnalysisController(MediatorStatistics stats, IServiceProvider serviceProvider)
    {
        _stats = stats;
        _serviceProvider = serviceProvider;
    }

    [HttpGet("queries")]
    public IActionResult GetQueries()
    {
        var analysis = _stats.AnalyzeQueries(_serviceProvider);
        return Ok(analysis);
    }

    [HttpGet("commands")]
    public IActionResult GetCommands()
    {
        var analysis = _stats.AnalyzeCommands(_serviceProvider);
        return Ok(analysis);
    }

    [HttpGet("statistics")]
    public IActionResult GetStatistics()
    {
        return Ok(new
        {
            Queries = _stats.QueryCount,
            Commands = _stats.CommandCount,
            Notifications = _stats.NotificationCount,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("comprehensive-report")]
    public IActionResult GetComprehensiveReport()
    {
        var queries = _stats.AnalyzeQueries(_serviceProvider);
        var commands = _stats.AnalyzeCommands(_serviceProvider);

        return Ok(new
        {
            Analysis = new
            {
                Queries = queries,
                Commands = commands
            },
            Statistics = new
            {
                QueryCount = _stats.QueryCount,
                CommandCount = _stats.CommandCount,
                NotificationCount = _stats.NotificationCount
            },
            GeneratedAt = DateTime.UtcNow
        });
    }
}
```

### Custom Statistics Renderer

Create custom renderers for different output formats:

```csharp
public class JsonStatisticsRenderer : IStatisticsRenderer
{
    private readonly ILogger<JsonStatisticsRenderer> _logger;

    public JsonStatisticsRenderer(ILogger<JsonStatisticsRenderer> logger)
    {
        _logger = logger;
    }

    public void RenderStatistics(int queryCount, int commandCount, int notificationCount)
    {
        var statistics = new
        {
            QueryCount = queryCount,
            CommandCount = commandCount,
            NotificationCount = notificationCount,
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(statistics, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        _logger.LogInformation("Mediator Statistics: {Statistics}", json);
    }
}

// Register the custom renderer
services.AddSingleton<IStatisticsRenderer, JsonStatisticsRenderer>();
```

### Best Practices

1. **Development Analysis**: Use `AnalyzeQueries` and `AnalyzeCommands` during development to understand your CQRS structure
2. **Monitoring Integration**: Include statistics in health checks and monitoring dashboards
3. **Performance Tracking**: Monitor execution counts to identify heavily used patterns
4. **Documentation**: Auto-generate documentation from discovered queries and commands
5. **Custom Renderers**: Create specialized renderers for different environments (console, logging, APIs)

## Sample Projects

The Blazing.Mediator library includes two comprehensive sample projects that demonstrate different architectural approaches and middleware usage patterns. Both projects showcase real-world implementations of CQRS with the Mediator pattern.

### ECommerce.Api - Traditional Controller Architecture

**📁 Location**: `src/samples/ECommerce.Api/`

This sample demonstrates a traditional e-commerce API using Controllers with conditional middleware for optimal performance.

#### Key Features

-   **Product Management**: CRUD operations for products with stock management
-   **Order Processing**: Complete order lifecycle from creation to completion
-   **Conditional Middleware**: Performance-optimized logging for specific request types
-   **Entity Framework**: In-memory database for development, SQL Server for production
-   **FluentValidation**: Comprehensive validation using FluentValidation library
-   **Error Handling**: Robust error handling with proper HTTP status codes

#### Architecture Overview

```
ECommerce.Api/
├── Application/           # Application layer (CQRS & business logic)
│   ├── Commands/          # Write operations (CQRS Commands)
│   │   ├── CancelOrderCommand.cs
│   │   ├── CreateOrderCommand.cs
│   │   ├── CreateProductCommand.cs
│   │   ├── DeactivateProductCommand.cs
│   │   ├── ProcessOrderCommand.cs
│   │   ├── ProcessOrderResponse.cs
│   │   ├── UpdateOrderStatusCommand.cs
│   │   ├── UpdateProductCommand.cs
│   │   └── UpdateProductStockCommand.cs
│   ├── Queries/           # Read operations (CQRS Queries)
│   │   ├── GetCustomerOrdersQuery.cs
│   │   ├── GetLowStockProductsQuery.cs
│   │   ├── GetOrderByIdQuery.cs
│   │   ├── GetOrdersQuery.cs
│   │   ├── GetOrderStatisticsQuery.cs
│   │   ├── GetProductByIdQuery.cs
│   │   └── GetProductsQuery.cs
│   ├── Handlers/          # Business logic handlers
│   │   ├── Commands/      # Command handlers
│   │   │   ├── CancelOrderHandler.cs
│   │   │   ├── CreateOrderHandler.cs
│   │   │   ├── CreateProductHandler.cs
│   │   │   ├── DeactivateProductHandler.cs
│   │   │   ├── ProcessOrderHandler.cs
│   │   │   ├── UpdateOrderStatusHandler.cs
│   │   │   ├── UpdateProductHandler.cs
│   │   │   └── UpdateProductStockHandler.cs
│   │   └── Queries/       # Query handlers
│   │       ├── GetCustomerOrdersHandler.cs
│   │       ├── GetLowStockProductsHandler.cs
│   │       ├── GetOrderByIdHandler.cs
│   │       ├── GetOrdersHandler.cs
│   │       ├── GetOrderStatisticsHandler.cs
│   │       ├── GetProductByIdHandler.cs
│   │       └── GetProductsHandler.cs
│   ├── Middleware/        # Conditional middleware
│   │   ├── OrderLoggingMiddleware.cs
│   │   └── ProductLoggingMiddleware.cs
│   ├── DTOs/              # Data transfer objects
│   │   ├── CreateOrderRequest.cs
│   │   ├── OperationResult.cs
│   │   ├── OrderDto.cs
│   │   ├── OrderItemDto.cs
│   │   ├── OrderItemRequest.cs
│   │   ├── OrderStatisticsDto.cs
│   │   ├── PagedResult.cs
│   │   ├── ProductDto.cs
│   │   └── ProductSalesDto.cs
│   ├── Mappings/          # Object mapping profiles
│   │   └── ECommerceMappingExtensions.cs
│   ├── Validators/        # FluentValidation validators
│   │   ├── CreateOrderCommandValidator.cs
│   │   ├── CreateProductCommandValidator.cs
│   │   ├── ProcessOrderCommandValidator.cs
│   │   ├── UpdateProductCommandValidator.cs
│   │   └── UpdateProductStockCommandValidator.cs
│   └── Exceptions/        # Custom exceptions
│       └── ValidationException.cs
├── Controllers/           # API Controllers (MVC approach)
│   ├── OrdersController.cs
│   └── ProductsController.cs
├── Domain/                # Domain layer
│   └── Entities/          # Domain entities
│       ├── Order.cs
│       ├── OrderItem.cs
│       ├── OrderStatus.cs
│       └── Product.cs
├── Infrastructure/        # Infrastructure layer
│   └── Data/              # Data access
│       └── ECommerceDbContext.cs
├── Endpoints/             # Minimal API endpoints (alternative to controllers)
│   └── ProductEndpoints.cs
├── Extensions/            # Service registration extensions
│   ├── ServiceCollectionExtensions.cs
│   └── WebApplicationExtensions.cs
├── Properties/            # Assembly properties
│   └── launchSettings.json
├── ECommerce.http         # HTTP test file
├── test-product.json      # Sample test data
├── Add-XmlDocs.ps1        # PowerShell script for XML documentation
├── Program.cs             # Application configuration & startup
├── appsettings.json       # Configuration settings
└── ECommerce.Api.csproj   # Project file
```

#### Sample Request/Response Flow

**Creating an Order:**

```http
POST /api/orders
Content-Type: application/json

{
  "customerId": 1,
  "customerEmail": "customer@example.com",
  "shippingAddress": "123 Main St, City, State",
  "items": [
    {
      "productId": 1,
      "quantity": 2,
      "unitPrice": 29.99
    }
  ]
}
```

**Middleware Output:**

```
🛒 ORDER REQUEST: CreateOrderCommand started at 2025-07-01 10:30:15.123
🛒 ORDER REQUEST DATA: {
  "customerId": 1,
  "customerEmail": "customer@example.com",
  "shippingAddress": "123 Main St, City, State",
  "items": [...]
}
🛒 ORDER RESPONSE: CreateOrderCommand completed successfully in 45ms
```

**Response:**

```json
{
    "success": true,
    "data": 12345,
    "message": "Order created successfully"
}
```

#### Testing the ECommerce API

Use the provided HTTP file for testing:

```http
### Get all products
GET http://localhost:5000/api/products

### Get specific product
GET http://localhost:5000/api/products/1

### Create new product
POST http://localhost:5000/api/products
Content-Type: application/json

{
  "name": "Wireless Headphones",
  "description": "High-quality wireless headphones",
  "price": 99.99,
  "stockQuantity": 50,
  "categoryId": 1
}

### Create order
POST http://localhost:5000/api/orders
Content-Type: application/json

{
  "customerId": 1,
  "customerEmail": "test@example.com",
  "shippingAddress": "123 Test Street, Test City",
  "items": [
    {
      "productId": 1,
      "quantity": 2,
      "unitPrice": 99.99
    }
  ]
}
```

### UserManagement.Api - Modern Minimal API Architecture

**📁 Location**: `src/samples/UserManagement.Api/`

This sample demonstrates a modern user management API using Minimal APIs with comprehensive standard middleware.

#### Key Features

-   **User Management**: Complete CRUD operations for users
-   **Minimal APIs**: Modern .NET approach with functional endpoints
-   **Standard Middleware**: Comprehensive logging for all operations
-   **Clean Architecture**: Separation of concerns with clear layer boundaries
-   **Error Handling**: Centralised error handling with proper responses
-   **Swagger Integration**: Complete API documentation

#### Architecture Overview

```
UserManagement.Api/
├── Application/           # Application layer (CQRS & business logic)
│   ├── Commands/          # Write operations (CQRS Commands)
│   │   ├── ActivateUserAccountCommand.cs
│   │   ├── CreateUserCommand.cs
│   │   ├── CreateUserWithIdCommand.cs
│   │   ├── DeactivateUserAccountCommand.cs
│   │   ├── DeleteUserCommand.cs
│   │   ├── UpdateUserCommand.cs
│   │   └── UpdateUserWithResultCommand.cs
│   ├── Queries/           # Read operations (CQRS Queries)
│   │   ├── GetActiveUsersQuery.cs
│   │   ├── GetUserByIdQuery.cs
│   │   ├── GetUsersQuery.cs
│   │   ├── GetUserStatisticsQuery.cs
│   │   └── UserStatisticsDto.cs
│   ├── Handlers/          # Business logic handlers
│   │   ├── Commands/      # Command handlers
│   │   │   ├── ActivateUserAccountHandler.cs
│   │   │   ├── CreateUserHandler.cs
│   │   │   ├── CreateUserWithIdHandler.cs
│   │   │   ├── DeactivateUserAccountHandler.cs
│   │   │   ├── DeleteUserHandler.cs
│   │   │   ├── UpdateUserHandler.cs
│   │   │   └── UpdateUserWithResultHandler.cs
│   │   └── Queries/       # Query handlers
│   │       ├── GetActiveUsersHandler.cs
│   │       ├── GetUserByIdHandler.cs
│   │       ├── GetUsersHandler.cs
│   │       └── GetUserStatisticsHandler.cs
│   ├── Middleware/        # Standard middleware (logs all operations)
│   │   ├── GeneralLoggingMiddleware.cs
│   │   └── GeneralCommandLoggingMiddleware.cs
│   ├── DTOs/              # Data transfer objects
│   │   ├── OperationResult.cs
│   │   ├── PagedResult.cs
│   │   └── UserDto.cs
│   ├── Mappings/          # Object mapping profiles
│   │   └── UserMappingExtensions.cs
│   ├── Validators/        # FluentValidation validators
│   │   ├── CreateUserCommandValidator.cs
│   │   ├── CreateUserWithIdCommandValidator.cs
│   │   ├── UpdateUserCommandValidator.cs
│   │   └── UpdateUserWithResultCommandValidator.cs
│   └── Exceptions/        # Custom exceptions
│       ├── BusinessException.cs
│       ├── NotFoundException.cs
│       └── ValidationException.cs
├── Domain/                # Domain layer
│   └── Entities/          # Domain entities
│       └── User.cs
├── Infrastructure/        # Infrastructure layer
│   └── Data/              # Data access
│       └── UserManagementDbContext.cs
├── Endpoints/             # Minimal API endpoints
│   ├── UserCommandEndpoints.cs
│   └── UserQueryEndpoints.cs
├── Extensions/            # Service registration extensions
│   ├── ServiceCollectionExtensions.cs
│   └── WebApplicationExtensions.cs
├── Properties/            # Assembly properties
│   └── launchSettings.json
├── UserManagement.http    # HTTP test file
├── Program.cs             # Application configuration & startup
├── appsettings.json       # Configuration settings
└── UserManagement.Api.csproj # Project file
```

#### Sample Request/Response Flow

**Getting User by ID:**

```http
GET /api/users/123
```

**Middleware Output:**

```
🔍 REQUEST: GetUserByIdQuery started at 2025-07-01 10:30:20.456
🔍 REQUEST DATA: { "userId": 123 }
🔍 RESPONSE: GetUserByIdQuery completed successfully in 12ms
```

**Response:**

```json
{
    "id": 123,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "isActive": true,
    "createdAt": "2025-01-01T00:00:00Z"
}
```

**Creating a User:**

```http
POST /api/users
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com",
  "dateOfBirth": "1990-05-15"
}
```

**Middleware Output:**

```
🔍 COMMAND: CreateUserCommand started at 2025-07-01 10:31:15.789
🔍 COMMAND DATA: {
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com",
  "dateOfBirth": "1990-05-15"
}
🔍 COMMAND COMPLETED: CreateUserCommand completed successfully in 25ms
```

#### Testing the UserManagement API

Use the provided HTTP file for testing:

```http
### Get all users
GET http://localhost:5001/api/users

### Get specific user
GET http://localhost:5001/api/users/1

### Create new user
POST http://localhost:5001/api/users
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "dateOfBirth": "1985-03-20"
}

### Update user
PUT http://localhost:5001/api/users/1
Content-Type: application/json

{
  "userId": 1,
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@example.com"
}

### Delete user
DELETE http://localhost:5001/api/users/1
```

### Running the Sample Projects

#### Prerequisites

-   .NET 9.0 SDK
-   Visual Studio 2022 or VS Code
-   SQL Server LocalDB (for production mode) or uses In-Memory database (development mode)

#### Getting Started

1. **Clone the repository:**

    ```bash
    git clone https://github.com/gragra33/blazing.mediator.git
    cd blazing.mediator
    ```

2. **Run ECommerce API:**

    ```bash
    cd src/samples/ECommerce.Api
    dotnet run
    ```

    Navigate to: `https://localhost:5000/swagger`

3. **Run UserManagement API:**
    ```bash
    cd src/samples/UserManagement.Api
    dotnet run
    ```
    Navigate to: `https://localhost:5001/swagger`

#### Sample Comparison

| Feature          | ECommerce.Api          | UserManagement.Api     |
| ---------------- | ---------------------- | ---------------------- |
| **API Style**    | Controllers            | Minimal APIs           |
| **Middleware**   | Conditional            | Standard               |
| **Database**     | Entity Framework       | In-Memory/Repository   |
| **Validation**   | FluentValidation       | Built-in               |
| **Architecture** | Traditional Layers     | Clean Architecture     |
| **Use Case**     | Complex Business Logic | Simple CRUD Operations |
| **Performance**  | Optimized Middleware   | Comprehensive Logging  |

Both samples demonstrate the flexibility of Blazing.Mediator and show that the library works equally well with different architectural approaches and API styles.

## Complete Examples

This section provides complete, runnable examples that demonstrate various aspects of Blazing.Mediator implementation.

### Complete CRUD Implementation

Here's a complete implementation showing all aspects of a User management system:

#### 1. Domain Models

```csharp
// Domain/Entities/User.cs
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";
    public int Age => DateTime.Now.Year - DateOfBirth.Year;
}
```

#### 2. DTOs

```csharp
// Application/DTOs/UserDto.cs
public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Application/DTOs/CreateUserDto.cs
public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

// Application/DTOs/PagedResult.cs
public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

#### 3. Repository Interface

```csharp
// Application/Interfaces/IUserRepository.cs
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, string? searchTerm = null);
    Task<List<User>> GetAllActiveAsync();
    Task<int> AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
    Task<int> GetTotalCountAsync();
}
```

#### 4. Queries (CQRS Read Side)

```csharp
// Application/Queries/GetUserByIdQuery.cs
public class GetUserByIdQuery : IRequest<UserDto>
{
    public int UserId { get; set; }
}

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _repository;    private readonly ILogger<GetUserByIdHandler> _logger;

    public GetUserByIdHandler(IUserRepository repository, ILogger<GetUserByIdHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting user with ID {UserId}", request.UserId);

        var user = await _repository.GetByIdAsync(request.UserId);

        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", request.UserId);
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        var userDto = user.ToDto(); // Use extension method for mapping

        _logger.LogDebug("Successfully retrieved user {UserId}", request.UserId);
        return userDto;
    }
}

// Application/Queries/GetUsersQuery.cs
public class GetUsersQuery : IRequest<PagedResult<UserDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool IncludeInactive { get; set; } = false;
}

public class GetUsersHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IUserRepository _repository;
    private readonly ILogger<GetUsersHandler> _logger;

    public GetUsersHandler(IUserRepository repository, ILogger<GetUsersHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting users: Page {Page}, PageSize {PageSize}, SearchTerm: {SearchTerm}",
            request.Page, request.PageSize, request.SearchTerm);

        var users = await _repository.GetPagedAsync(request.Page, request.PageSize, request.SearchTerm);

        // Use extension method for mapping to paginated DTO
        var result = users.Items.ToPagedDto(users.TotalCount, request.Page, request.PageSize);

        _logger.LogDebug("Retrieved {Count} users out of {Total}", result.Items.Count, result.TotalCount);
        return result;
    }
}
```

#### 5. Commands (CQRS Write Side)

```csharp
// Application/Commands/CreateUserCommand.cs
public class CreateUserCommand : IRequest<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class CreateUserHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly IUserRepository _repository;
    private readonly IValidator<CreateUserCommand> _validator;
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(IUserRepository repository, IValidator<CreateUserCommand> validator, ILogger<CreateUserHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user with email {Email}", request.Email);

        // Validate the command
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for CreateUserCommand: {Errors}", errors);
            throw new ValidationException(validationResult.Errors);
        }

        // Check if email already exists
        if (await _repository.EmailExistsAsync(request.Email))
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
        var userId = await _repository.AddAsync(user);

        _logger.LogInformation("User created successfully with ID {UserId}", userId);
        return userId;
    }
}

// Application/Commands/UpdateUserCommand.cs
public class UpdateUserCommand : IRequest
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IUserRepository _repository;
    private readonly IValidator<UpdateUserCommand> _validator;
    private readonly ILogger<UpdateUserHandler> _logger;

    public UpdateUserHandler(IUserRepository repository, IValidator<UpdateUserCommand> validator, ILogger<UpdateUserHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating user {UserId}", request.UserId);

        // Validate the command
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for UpdateUserCommand: {Errors}", errors);
            throw new ValidationException(validationResult.Errors);
        }

        // Get existing user
        var user = await _repository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("Attempted to update non-existent user {UserId}", request.UserId);
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Check if email is being changed and if it already exists
        if (user.Email != request.Email && await _repository.EmailExistsAsync(request.Email, request.UserId))
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

        await _repository.UpdateAsync(user);

        _logger.LogInformation("User {UserId} updated successfully", request.UserId);
    }
}

// Application/Commands/DeleteUserCommand.cs
public class DeleteUserCommand : IRequest
{
    public int UserId { get; set; }
}

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _repository;
    private readonly ILogger<DeleteUserHandler> _logger;

    public DeleteUserHandler(IUserRepository repository, ILogger<DeleteUserHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting user {UserId}", request.UserId);

        // Check if user exists
        var user = await _repository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("Attempted to delete non-existent user {UserId}", request.UserId);
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        await _repository.DeleteAsync(request.UserId);

        _logger.LogInformation("User {UserId} deleted successfully", request.UserId);
    }
}
```

#### 6. Validation

```csharp
// Application/Validators/CreateUserCommandValidator.cs
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("First name contains invalid characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Last name contains invalid characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Date of birth cannot be more than 120 years ago");
    }
}

// Application/Validators/UpdateUserCommandValidator.cs
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");



        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("First name contains invalid characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Last name contains invalid characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Date of birth cannot be more than 120 years ago");
    }
}
```

#### 7. Exception Classes

```csharp
// Application/Exceptions/NotFoundException.cs
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

// Application/Exceptions/ConflictException.cs
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }

    public ConflictException(string message, Exception innerException) : base(message, innerException) { }
}

// Application/Exceptions/ValidationException.cs
public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures have occurred.")
    {
        Errors = failures;
    }
}
```

#### 8. Mapping Extensions

Instead of AutoMapper, we use manual extension methods for mapping between domain entities and DTOs. This provides better performance, compile-time safety, and explicit control over the mapping logic.

```csharp
// Application/Mappings/UserMappingExtensions.cs
using Application.DTOs;
using Domain.Entities;

namespace Application.Mappings;

/// <summary>
/// Extension methods for mapping between User domain entities and DTOs.
/// Provides explicit, performant mapping without external dependencies.
/// </summary>
public static class UserMappingExtensions
{
    /// <summary>
    /// Converts a User domain entity to a UserDto.
    /// </summary>
    /// <param name="user">The user entity to convert.</param>
    /// <returns>A UserDto representation of the user.</returns>
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.GetFullName(), // Use domain logic
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            Age = user.GetAge(), // Calculated property
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    /// <summary>
    /// Converts a collection of User domain entities to a list of UserDtos.
    /// </summary>
    /// <param name="users">The collection of user entities to convert.</param>
    /// <returns>A list of UserDto representations.</returns>
    public static List<UserDto> ToDto(this IEnumerable<User> users)
    {
        return users.Select(u => u.ToDto()).ToList();
    }

    /// <summary>
    /// Converts a collection of User domain entities to a paginated result with UserDtos.
    /// </summary>
    /// <param name="users">The collection of user entities to convert.</param>
    /// <param name="totalCount">The total number of users available.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated result containing UserDtos.</returns>
    public static PagedResult<UserDto> ToPagedDto(this IEnumerable<User> users, int totalCount, int page, int pageSize)
    {
        return new PagedResult<UserDto>
        {
            Items = users.ToDto(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Converts a CreateUserCommand to a User domain entity.
    /// </summary>
    /// <param name="command">The command containing user creation data.</param>
    /// <returns>A new User entity ready for persistence.</returns>
    public static User ToEntity(this CreateUserCommand command)
    {
        return User.Create(
            command.FirstName,
            command.LastName,
            command.Email,
            command.DateOfBirth);
    }
}
```

#### Benefits of Manual Mapping Extensions

✅ **Performance**: No reflection overhead - direct property assignment  
✅ **Compile-time Safety**: Compilation errors if properties don't match  
✅ **Explicit Control**: Clear, readable mapping logic with custom transformations  
✅ **No Dependencies**: Reduces external package dependencies  
✅ **IntelliSense Support**: Full IDE support with auto-completion  
✅ **Easy Testing**: Simple to test and mock mapping behavior  
✅ **Domain Logic Integration**: Can call domain methods for calculated properties

````

#### 9. API Controller

```csharp
// Controllers/UsersController.cs
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

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Get users with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="searchTerm">Search term for filtering</param>
    /// <param name="includeInactive">Include inactive users</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = null,
        [FromQuery] bool includeInactive = false)
    {
        var query = new GetUsersQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IncludeInactive = includeInactive
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="dto">User creation data</param>
    /// <returns>Created user ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<int>> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            var command = new CreateUserCommand
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                DateOfBirth = dto.DateOfBirth
            };

            var userId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetUser), new { id = userId }, userId);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            return BadRequest(new { message = "Validation failed", errors });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="command">User update data</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserCommand command)
    {
        if (id != command.UserId)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            return BadRequest(new { message = "Validation failed", errors });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
````

#### 10. Program Configuration

```csharp
// Program.cs
using Blazing.Mediator;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateUserCommandValidator).Assembly);

// Register Mediator with middleware
builder.Services.AddMediator(config =>
{
    // Add logging middleware for all requests
    config.AddMiddleware<GeneralLoggingMiddleware<,>>();
    config.AddMiddleware<GeneralCommandLoggingMiddleware<>>();
}, typeof(Program).Assembly);

// Register application services
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
```

This complete example demonstrates:

-   **CQRS Implementation**: Clear separation between commands and queries
-   **Validation**: Comprehensive input validation using FluentValidation
-   **Error Handling**: Proper exception handling with custom exceptions
-   **Logging**: Structured logging throughout the application
-   **Mapping**: Clean object mapping using AutoMapper
-   **API Design**: RESTful API design with proper HTTP status codes
-   **Middleware**: Custom middleware for cross-cutting concerns
-   **Testing Ready**: Easy to test with dependency injection

The example shows how all the concepts come together in a real-world application using Blazing.Mediator with CQRS patterns.
