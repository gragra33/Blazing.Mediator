# Blazing.Mediator - Implementation Guide

## Overview

The Mediator pattern decouples components by having them communicate through a central mediator rather than directly with each other. This promotes loose coupling, better testability, and cleaner architecture.

`Blazing.Mediator` provides a lightweight implementation of the Mediator pattern for .NET applications that naturally implements **Command Query Responsibility Segregation (CQRS)** by separating read operations (queries) from write operations (commands). This separation allows for optimized data models, improved performance, and better scalability.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Installation](#installation)
3. [Core Concepts](#core-concepts)
4. [Creating Requests](#creating-requests)
5. [Implementing Handlers](#implementing-handlers)
6. [Setup and Registration](#setup-and-registration)
7. [Usage in APIs](#usage-in-apis)
8. [Validation and Error Handling](#validation-and-error-handling)
9. [Testing Strategies](#testing-strategies)
10. [Advanced Scenarios](#advanced-scenarios)
11. [Best Practices](#best-practices)
12. [Common Mistakes](#common-mistakes)
13. [Troubleshooting](#troubleshooting)
14. [Complete Examples](#complete-examples)

## Quick Start

Get up and running with Blazing.Mediator in under 5 minutes:

### 1. Install the Package

```xml
<PackageReference Include="Blazing.Mediator" Version="1.0.0" />
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
// Program.cs
builder.Services.AddMediator(typeof(Program).Assembly);
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

## Installation

### Option 1: Add as Project Reference

```xml
<ProjectReference Include="..\..\Blazing.Mediator\Blazing.Mediator.csproj" />
```

### Option 2: Package Reference (if published)

```xml
<PackageReference Include="Blazing.Mediator" Version="1.0.0" />
```

## Core Concepts

### CQRS Implementation

`Blazing.Mediator` inherently implements the **Command Query Responsibility Segregation (CQRS)** pattern by providing distinct interfaces for commands and queries:

-   **Commands**: Operations that change state (Create, Update, Delete) but typically don't return data
-   **Queries**: Operations that retrieve data without changing state (Read operations)

This separation enables:

-   **Performance Optimization**: Queries can use optimized read models and caching
-   **Scalability**: Read and write operations can be scaled independently
-   **Security**: Different validation and authorization rules for commands vs queries
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

The following diagram illustrates the flow of requests through the Blazing.Mediator system:

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
-   Business logic is organized into discrete, focused units
-   Easier to understand and maintain individual components
-   Reduces complexity by avoiding monolithic service classes

#### CQRS Implementation

-   Clear separation between Commands (write operations) and Queries (read operations)
-   Optimized data models for different use cases
-   Different validation and security rules for reads vs writes
-   Enables different scaling strategies for read and write operations

#### Improved Scalability

-   Read and write operations can be scaled independently
-   Query handlers can use optimized read models or caching
-   Command handlers can focus on business rules and data consistency
-   Supports distributed architectures and microservices patterns

#### Better Maintainability

-   Clear request/response flow through the system
-   Centralized request routing and handling
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
```

### Alternative Registration Methods

```csharp
// Method 1: Using assembly marker types
services.AddMediator(
    typeof(GetUserHandler),
    typeof(CreateOrderHandler),
    typeof(UpdateProductHandler)
);

// Method 2: Using assembly references
services.AddMediator(
    Assembly.GetExecutingAssembly(),
    typeof(ExternalHandler).Assembly
);

// Method 3: Scan calling assembly automatically
services.AddMediatorFromCallingAssembly();

// Method 4: Scan with filter
services.AddMediatorFromLoadedAssemblies(assembly =>
    assembly.FullName.StartsWith("MyCompany.") &&
    assembly.FullName.Contains(".Application"));
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
    private readonly IMapper _mapper;

    public GetUserByIdHandler(IUserReadRepository userRepository, IMemoryCache cache, IMapper mapper)
    {
        _userRepository = userRepository;
        _cache = cache;
        _mapper = mapper;
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

        var userDto = _mapper.Map<UserDto>(user);

        // Cache the result
        _cache.Set(cacheKey, userDto, TimeSpan.FromMinutes(5));

        return userDto;
    }
}

public class GetUsersHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IUserReadRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUsersHandler(IUserReadRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Use read-optimized repository with specialized query methods
        var users = await _userRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.IncludeInactive);

        var userDtos = _mapper.Map<List<UserDto>>(users.Items);

        return new PagedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = users.TotalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

// Analytical query handler - can use different data source
public class GetUserStatisticsHandler : IRequestHandler<GetUserStatisticsQuery, UserStatisticsDto>
{
    private readonly IAnalyticsRepository _analyticsRepository; // Specialized analytics data source
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

Testing handlers is straightforward because they're isolated and have clear dependencies:

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

    // Assert
    Assert.That(result.Id, Is.EqualTo(1));
    Assert.That(result.FirstName, Is.EqualTo("John"));
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

        Assert.That(user.Id, Is.EqualTo(1));
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

### Choosing Between Minimal APIs and Controllers

**Use Minimal APIs when:**

-   Building simple, focused APIs
-   Preferring functional programming style
-   Wanting minimal ceremony and boilerplate
-   Building microservices with few endpoints
-   Starting new projects with .NET 6+

**Use Controllers when:**

-   Building complex APIs with many endpoints
-   Preferring object-oriented structure
-   Needing advanced features like model binding, filters
-   Working with existing controller-based codebases
-   Requiring fine-grained control over HTTP behavior

Both approaches work equally well with the Blazing.Mediator library and CQRS patterns.

## Advanced Scenarios

### Composite Handlers

```csharp
// Handler that calls multiple other handlers
public class ProcessOrderHandler : IRequestHandler<ProcessOrderCommand, ProcessOrderResponse>
{
    private readonly IMediator _mediator;

    public ProcessOrderHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<ProcessOrderResponse> Handle(ProcessOrderCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Create the order
        var createOrderCommand = new CreateOrderCommand
        {
            UserId = request.UserId,
            Items = request.Items
        };
        var orderId = await _mediator.Send(createOrderCommand, cancellationToken);

        // Step 2: Process payment
        var processPaymentCommand = new ProcessPaymentCommand
        {
            OrderId = orderId,
            PaymentMethod = request.PaymentMethod
        };
        var paymentResult = await _mediator.Send(processPaymentCommand, cancellationToken);

        // Step 3: Update inventory
        var updateInventoryCommand = new UpdateInventoryCommand
        {
            OrderId = orderId
        };
        await _mediator.Send(updateInventoryCommand, cancellationToken);

        // Step 4: Send confirmation email
        var sendEmailCommand = new SendOrderConfirmationEmailCommand
        {
            OrderId = orderId
        };
        await _mediator.Send(sendEmailCommand, cancellationToken);

        return new ProcessOrderResponse
        {
            OrderId = orderId,
            PaymentId = paymentResult.PaymentId
        };
    }
}
```

### Background Processing

```csharp
// Handler for background tasks
public class SendWelcomeEmailHandler : IRequestHandler<SendWelcomeEmailCommand>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendWelcomeEmailHandler> _logger;

    public SendWelcomeEmailHandler(IEmailService emailService, ILogger<SendWelcomeEmailHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(SendWelcomeEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _emailService.SendWelcomeEmailAsync(request.UserId, request.Email);
            _logger.LogInformation("Welcome email sent to user {UserId}", request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to user {UserId}", request.UserId);
            // Don't rethrow - this is a background operation
        }
    }
}

// Usage in another handler
public class CreateUserHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly IUserRepository _userRepository;
    private readonly IMediator _mediator;

    public CreateUserHandler(IUserRepository userRepository, IMediator mediator)
    {
        _userRepository = userRepository;
        _mediator = mediator;
    }

    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Create user
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Send welcome email asynchronously (fire and forget)
        _ = Task.Run(async () =>
        {
            var emailCommand = new SendWelcomeEmailCommand
            {
                UserId = user.Id,
                Email = user.Email
            };
            await _mediator.Send(emailCommand);
        });

        return user.Id;
    }
}
```

## Best Practices

### 1. Request Naming Conventions (CQRS Best Practices)

-   **Queries**: Use descriptive names ending with "Query" that describe what data is being retrieved (e.g., `GetUserByIdQuery`, `GetActiveUsersQuery`, `GetMonthlyRevenueQuery`)
-   **Commands**: Use verb-noun patterns ending with "Command" that describe the business intent (e.g., `CreateUserCommand`, `ActivateUserAccountCommand`, `ProcessOrderCommand`)

### 2. Handler Organization (Separate Read and Write Concerns)

```
Application/
├── Commands/           # Write operations (CQRS Commands)
│   ├── CreateUser/
│   │   ├── CreateUserCommand.cs
│   │   ├── CreateUserHandler.cs
│   │   └── CreateUserValidator.cs
│   └── UpdateUser/
│       ├── UpdateUserCommand.cs
│       ├── UpdateUserHandler.cs
│       └── UpdateUserValidator.cs
└── Queries/            # Read operations (CQRS Queries)
    ├── GetUser/
    │   ├── GetUserQuery.cs
    │   └── GetUserHandler.cs
    └── GetUsers/
        ├── GetUsersQuery.cs
        └── GetUsersHandler.cs
```

### 3. Keep Handlers Focused and Small

Each handler should have a single responsibility:

```csharp
// ✅ Good - focused handler
public class CreateUserHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<CreateUserCommand> _validator;

    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        // 2. Create entity
        var user = new User(request.FirstName, request.LastName, request.Email);

        // 3. Persist
        await _userRepository.AddAsync(user);

        return user.Id;
    }
}
```

### 4. Use DTOs for Responses

Don't expose domain entities directly:

```csharp
// ✅ Good - use DTOs
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ❌ Avoid - exposing domain entities
public class User // Domain entity
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; } // Sensitive data!
    public DateTime CreatedAt { get; set; }
}
```

### 5. Implement Proper Error Handling

Use consistent error handling patterns:

```csharp
public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);

        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        return _mapper.Map<UserDto>(user);
    }
}
```

### 6. Use Cancellation Tokens

Always accept and use cancellation tokens:

```csharp
public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
{
    // Pass cancellation token to all async operations
    var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
    var orders = await _orderRepository.GetByUserIdAsync(request.UserId, cancellationToken);

    return new UserDto { /* ... */ };
}
```

### 7. Separate Validation Logic

Use dedicated validators instead of inline validation:

```csharp
// ✅ Good - separate validator
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(BeUniqueEmail)
            .WithMessage("Email already exists");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        // Custom validation logic
        return await _userRepository.IsEmailUniqueAsync(email);
    }
}
```

## Common Mistakes

Understanding common pitfalls helps new users avoid frustrating debugging sessions:

### 1. Forgetting to Register Handlers

**Problem**: `InvalidOperationException: No service for type 'IRequestHandler<MyQuery, MyResult>' has been registered.`

**Solution**: Ensure all assemblies containing handlers are registered:

```csharp
// ❌ Wrong - missing assembly with handlers
builder.Services.AddMediator(typeof(Program).Assembly);

// ✅ Correct - include all assemblies
builder.Services.AddMediator(
    typeof(Program).Assembly,
    typeof(GetUserHandler).Assembly  // Include application layer
);
```

### 2. Inconsistent Request/Response Types

**Problem**: Handler return type doesn't match request definition.

```csharp
// ❌ Wrong - mismatched types
public class GetUserQuery : IRequest<UserDto> { }

public class GetUserHandler : IRequestHandler<GetUserQuery, String> // Wrong return type
{
    public async Task<String> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return "some string"; // Should return UserDto
    }
}
```

**Solution**: Ensure types match exactly:

```csharp
// ✅ Correct - matching types
public class GetUserQuery : IRequest<UserDto> { }

public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return new UserDto(); // Correct return type
    }
}
```

### 3. Multiple Handlers for Same Request

**Problem**: Multiple handlers registered for the same request type causes ambiguity.

**Solution**: Ensure only one handler per request type:

```csharp
// ❌ Wrong - multiple handlers
public class GetUserHandler1 : IRequestHandler<GetUserQuery, UserDto> { }
public class GetUserHandler2 : IRequestHandler<GetUserQuery, UserDto> { } // Duplicate!

// ✅ Correct - one handler per request
public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto> { }
```

### 4. Blocking Async Operations

**Problem**: Using `.Result` or `.Wait()` can cause deadlocks.

```csharp
// ❌ Wrong - blocking async
public class SomeService
{
    public UserDto GetUser(int id)
    {
        return _mediator.Send(new GetUserQuery { Id = id }).Result; // Deadlock risk!
    }
}
```

**Solution**: Use async/await throughout:

```csharp
// ✅ Correct - async all the way
public class SomeService
{
    public async Task<UserDto> GetUserAsync(int id)
    {
        return await _mediator.Send(new GetUserQuery { Id = id });
    }
}
```

### 5. Fat Controllers vs Thin Controllers

**Problem**: Putting business logic in controllers defeats the purpose of the mediator pattern.

```csharp
// ❌ Wrong - business logic in controller
[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    // Validation logic
    if (string.IsNullOrEmpty(request.Email))
        return BadRequest("Email is required");

    // Business logic
    if (await _userService.EmailExistsAsync(request.Email))
        return BadRequest("Email already exists");

    // Data access logic
    var user = new User { Email = request.Email };
    await _userRepository.AddAsync(user);

    return Ok(user.Id);
}
```

**Solution**: Keep controllers thin, put logic in handlers:

```csharp
// ✅ Correct - thin controller
[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserCommand command)
{
    try
    {
        var userId = await _mediator.Send(command);
        return Ok(userId);
    }
    catch (ValidationException ex)
    {
        return BadRequest(ex.Errors);
    }
}
```

## Troubleshooting

### Handler Not Found Exception

**Error**: `InvalidOperationException: No service for type 'IRequestHandler<...>' has been registered`

**Causes & Solutions**:

1. **Handler not in registered assembly**:

    ```csharp
    // Check if handler's assembly is registered
    builder.Services.AddMediator(typeof(YourHandler).Assembly);
    ```

2. **Handler class not public**:

    ```csharp
    // ❌ Wrong
    internal class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>

    // ✅ Correct
    public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
    ```

3. **Missing parameterless constructor or unresolvable dependencies**:
    ```csharp
    // ✅ Ensure handler can be instantiated by DI
    public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
    {
        private readonly IUserRepository _repository;

        public GetUserHandler(IUserRepository repository)
        {
            _repository = repository;
        }
    }
    ```

### Circular Dependencies

**Error**: `InvalidOperationException: A circular dependency was detected`

**Solution**: Avoid handlers calling other handlers directly. Use composition or domain services:

```csharp
// ❌ Wrong - circular dependency risk
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IMediator _mediator; // Avoid this pattern

    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Don't call other handlers from within handlers
        var user = await _mediator.Send(new GetUserQuery { Id = request.UserId });
        // ...
    }
}

// ✅ Correct - use direct services
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;

    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        // ...
    }
}
```

### Performance Issues

**Problem**: Slow response times or high memory usage.

**Solutions**:

1. **Use cancellation tokens**:

    ```csharp
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
    ```

2. **Optimize database queries**:

    ```csharp
    // Use projection instead of full entities
    var userDto = await _context.Users
        .Where(u => u.Id == request.Id)
        .Select(u => new UserDto { Id = u.Id, Name = u.Name })
        .FirstOrDefaultAsync(cancellationToken);
    ```

3. **Implement caching for queries**:
    ```csharp
    public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
    {
        private readonly IMemoryCache _cache;

        public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"user-{request.Id}";
            if (_cache.TryGetValue(cacheKey, out UserDto cachedUser))
                return cachedUser;

            var user = await _repository.GetByIdAsync(request.Id);
            _cache.Set(cacheKey, user, TimeSpan.FromMinutes(5));
            return user;
        }
    }
    ```

### Debugging Tips

1. **Enable detailed logging**:

    ```csharp
    // In appsettings.Development.json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Blazing.Mediator": "Debug"
        }
      }
    }
    ```

2. **Use middleware to log requests**:

    ```csharp
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Processing request: {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await _next(context);

            _logger.LogInformation("Completed request: {Method} {Path} - Status: {StatusCode}",
                context.Request.Method, context.Request.Path, context.Response.StatusCode);
        }
    }
    ```

## Best Practices

### 1. Request Naming Conventions (CQRS Best Practices)

-   **Queries**: Use descriptive names ending with "Query" that describe what data is being retrieved (e.g., `GetUserByIdQuery`, `GetActiveUsersQuery`, `GetMonthlyRevenueQuery`)
-   **Commands**: Use verb-noun patterns ending with "Command" that describe the business intent (e.g., `CreateUserCommand`, `ActivateUserAccountCommand`, `ProcessOrderCommand`)

### 2. Handler Organization (Separate Read and Write Concerns)

```
Application/
├── Commands/           # Write operations (CQRS Commands)
│   ├── CreateUser/
│   │   ├── CreateUserCommand.cs
│   │   ├── CreateUserHandler.cs
│   │   └── CreateUserValidator.cs
│   └── UpdateUser/
│       ├── UpdateUserCommand.cs
│       ├── UpdateUserHandler.cs
│       └── UpdateUserValidator.cs
└── Queries/            # Read operations (CQRS Queries)
    ├── GetUser/
    │   ├── GetUserQuery.cs
    │   └── GetUserHandler.cs
    └── GetUsers/
        ├── GetUsersQuery.cs
        └── GetUsersHandler.cs
```

### 3. Keep Handlers Focused and Small

Each handler should have a single responsibility:

```csharp
// ✅ Good - focused handler
public class CreateUserHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<CreateUserCommand> _validator;

    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        // 2. Create entity
        var user = new User(request.FirstName, request.LastName, request.Email);

        // 3. Persist
        await _userRepository.AddAsync(user);

        return user.Id;
    }
}
```

### 4. Use DTOs for Responses

Don't expose domain entities directly:

```csharp
// ✅ Good - use DTOs
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ❌ Avoid - exposing domain entities
public class User // Domain entity
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; } // Sensitive data!
    public DateTime CreatedAt { get; set; }
}
```

### 5. Implement Proper Error Handling

Use consistent error handling patterns:

```csharp
public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);

        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        return _mapper.Map<UserDto>(user);
    }
}
```

### 6. Use Cancellation Tokens

Always accept and use cancellation tokens:

```csharp
public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
{
    // Pass cancellation token to all async operations
    var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
    var orders = await _orderRepository.GetByUserIdAsync(request.UserId, cancellationToken);

    return new UserDto { /* ... */ };
}
```

### 7. Separate Validation Logic

Use dedicated validators instead of inline validation:

```csharp
// ✅ Good - separate validator
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(BeUniqueEmail)
            .WithMessage("Email already exists");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        // Custom validation logic
        return await _userRepository.IsEmailUniqueAsync(email);
    }
}
```

## Complete Examples

### E-Commerce Product Management

This complete example shows a typical product management scenario with full CQRS implementation:

#### 1. Domain Models

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### 2. DTOs

```csharp
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class ProductSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}
```

#### 3. Commands (Write Operations)

```csharp
// Create Product Command
public class CreateProductCommand : IRequest<int>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int InitialStock { get; set; }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.InitialStock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");
    }
}

public class CreateProductHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateProductCommand> _validator;

    public CreateProductHandler(IProductRepository productRepository, IValidator<CreateProductCommand> validator)
    {
        _productRepository = productRepository;
        _validator = validator;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validate
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        // Create entity
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.InitialStock,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Persist
        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        return product.Id;
    }
}

// Update Product Command
public class UpdateProductCommand : IRequest
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);

        if (product == null)
            throw new NotFoundException($"Product with ID {request.ProductId} not found");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();
    }
}
```

#### 4. Queries (Read Operations)

```csharp
// Get Product by ID Query
public class GetProductByIdQuery : IRequest<ProductDto>
{
    public int ProductId { get; set; }
}

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);

        if (product == null)
            throw new NotFoundException($"Product with ID {request.ProductId} not found");

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive
        };
    }
}

// Get Products with Pagination Query
public class GetProductsQuery : IRequest<PagedResult<ProductSummaryDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SearchTerm { get; set; } = "";
    public bool? IsActive { get; set; }
}

public class GetProductsHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductSummaryDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResult<ProductSummaryDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (products, totalCount) = await _productRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.IsActive);

        var productDtos = products.Select(p => new ProductSummaryDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            IsActive = p.IsActive
        }).ToList();

        return new PagedResult<ProductSummaryDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
```

#### 5. Controller Implementation

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        try
        {
            var product = await _mediator.Send(new GetProductByIdQuery { ProductId = id });
            return Ok(product);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = "",
        [FromQuery] bool? isActive = null)
    {
        var query = new GetProductsQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IsActive = isActive
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateProduct(CreateProductCommand command)
    {
        try
        {
            var productId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetProduct), new { id = productId }, productId);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProduct(int id, UpdateProductCommand command)
    {
        try
        {
            command.ProductId = id;
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
    }
}
```

#### 6. Supporting Infrastructure

```csharp
// Paged Result DTO
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

// Repository Interface
public interface IProductRepository
{
    Task<Product> GetByIdAsync(int id);
    Task<(List<Product> products, int totalCount)> GetPagedAsync(int page, int pageSize, string searchTerm, bool? isActive);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task SaveChangesAsync();
}
```

This complete example demonstrates:

-   ✅ Proper CQRS separation (Commands vs Queries)
-   ✅ Input validation with FluentValidation
-   ✅ Error handling with custom exceptions
-   ✅ Pagination for large datasets
-   ✅ Thin controllers that delegate to mediator
-   ✅ Clean separation of concerns
-   ✅ Consistent naming conventions
-   ✅ Proper use of DTOs

---

## Summary

Blazing.Mediator provides a clean, lightweight implementation of the Mediator pattern with built-in CQRS support. By following the patterns and practices outlined in this guide, you can build maintainable, testable, and scalable applications.

### Key Takeaways

1. **Start simple** with the Quick Start guide
2. **Use CQRS** to separate read and write operations
3. **Keep handlers focused** on single responsibilities
4. **Implement proper validation** and error handling
5. **Test handlers in isolation** for better unit tests
6. **Follow naming conventions** for consistency
7. **Avoid common mistakes** like circular dependencies
8. **Use the troubleshooting guide** when issues arise

For more examples and advanced scenarios, check out the sample projects in the repository.
