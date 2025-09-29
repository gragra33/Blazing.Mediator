
### MediatorConfiguration Fluent Methods

The `MediatorConfiguration` class provides fluent methods for configuring various aspects of the mediator, including middleware discovery, statistics tracking, telemetry, logging, notification handling, and assembly registration. These methods allow you to customize the mediator's behavior to match your application's specific requirements and performance needs. The fluent interface provides a clean, discoverable way to configure complex behaviors while maintaining backward compatibility.

#### Assembly Registration Methods

| Method | Parameters | Purpose | Configuration Impact |
|--------|------------|---------|---------------------|
| `AddFromAssembly(Type)` | Assembly marker type | Register handlers from specific assembly using marker type | Scans assembly for handlers and requests |
| `AddFromAssembly<TAssemblyMarker>()` | None (generic type parameter) | Register handlers from specific assembly using generic marker | Scans assembly for handlers and requests |
| `AddFromAssembly(Assembly)` | Assembly instance | Register handlers from specific assembly | Scans assembly for handlers and requests |
| `AddFromAssemblies(params Type[])` | Assembly marker types | Register handlers from multiple assemblies using marker types | Scans multiple assemblies for handlers |
| `AddFromAssemblies(params Assembly[])` | Assembly instances | Register handlers from assembly collection | Scans provided assemblies for handlers |
| `AddAssembly(Type)` | Assembly marker type | Alias for AddFromAssembly using marker type | Scans assembly for handlers and requests |
| `AddAssembly<TAssemblyMarker>()` | None (generic type parameter) | Alias for AddFromAssembly using generic marker | Scans assembly for handlers and requests |
| `AddAssembly(Assembly)` | Assembly instance | Alias for AddFromAssembly | Scans assembly for handlers and requests |
| `AddAssemblies(params Type[])` | Assembly marker types | Alias for AddFromAssemblies using marker types | Scans multiple assemblies for handlers |
| `AddAssemblies(params Assembly[])` | Assembly instances | Alias for AddFromAssemblies | Scans provided assemblies for handlers |

#### Statistics Configuration Methods

| Method | Parameters | Purpose | Configuration Impact |
|--------|------------|---------|---------------------|
| `WithStatisticsTracking()` | None | Enable basic statistics with default options | Tracks request counts and basic metrics |
| `WithStatisticsTracking(Action<StatisticsOptions>)` | Configuration action | Enable statistics with custom options | Configures detailed performance tracking |
| `WithStatisticsTracking(StatisticsOptions)` | Options instance | Enable statistics with provided options | Uses pre-configured statistics options |
| `WithoutStatistics()` | None | Disable statistics tracking | Prevents runtime statistics collection |

#### Telemetry Configuration Methods

| Method | Parameters | Purpose | Configuration Impact |
|--------|------------|---------|---------------------|
| `WithTelemetry()` | None | Enable telemetry with default options | Enables OpenTelemetry integration |
| `WithTelemetry(Action<TelemetryOptions>)` | Configuration action | Enable telemetry with custom options | Configures OpenTelemetry settings |
| `WithTelemetry(TelemetryOptions)` | Options instance | Enable telemetry with provided options | Uses pre-configured telemetry options |
| `WithNotificationTelemetry()` | None | Enable notification telemetry with default options | Comprehensive notification handler and subscriber telemetry |
| `WithNotificationTelemetry(Action<TelemetryOptions>)` | Configuration action | Enable notification telemetry with custom options | Configures notification telemetry settings |
| `WithHandlerChildSpans(bool)` | Enabled flag (default true) | Enable creation of child spans for individual notification handlers | Detailed per-handler visibility in distributed tracing |
| `WithSubscriberMetrics(bool)` | Enabled flag (default true) | Enable capture of notification subscriber metrics | Tracks manual subscriber performance and registration status |
| `WithNotificationHandlerDetails(bool)` | Enabled flag (default true) | Enable capture of detailed notification handler information | Handler execution details, performance metrics, and error tracking |
| `WithNotificationMiddlewareDetails(bool)` | Enabled flag (default true) | Enable capture of notification middleware execution details | Middleware performance, execution order, and error handling |
| `WithoutNotificationTelemetry()` | None | Disable all notification-specific telemetry tracking | Turns off child spans, subscriber metrics, handler details, middleware details |
| `WithoutTelemetry()` | None | Disable telemetry tracking | Prevents OpenTelemetry metrics and tracing collection |

#### Logging Configuration Methods

| Method | Parameters | Purpose | Configuration Impact |
|--------|------------|---------|---------------------|
| `WithLogging()` | None | Enable debug logging with default configuration | Enables comprehensive debug logging |
| `WithLogging(Action<LoggingOptions>)` | Configuration action | Enable debug logging with custom options | Configures detailed logging settings |
| `WithLogging(LoggingOptions)` | Options instance | Enable debug logging with provided options | Uses pre-configured logging options |
| `WithoutLogging()` | None | Disable debug logging | Prevents detailed debug logging generation |

#### Middleware Discovery Methods

| Method | Parameters | Purpose | Configuration Impact |
|--------|------------|---------|---------------------|
| `WithMiddlewareDiscovery()` | None | Enable automatic discovery of request middleware | Scans assemblies for request middleware implementations |
| `WithoutMiddlewareDiscovery()` | None | Disable automatic discovery of request middleware | Only manually registered request middleware available |
| `WithNotificationMiddlewareDiscovery()` | None | Enable automatic discovery of notification middleware | Scans assemblies for notification middleware implementations |
| `WithoutNotificationMiddlewareDiscovery()` | None | Disable automatic discovery of notification middleware | Only manually registered notification middleware available |
| `WithConstrainedMiddlewareDiscovery()` | None | Enable automatic discovery of type-constrained notification middleware | Discovers middleware implementing INotificationMiddleware{T} |
| `WithoutConstrainedMiddlewareDiscovery()` | None | Disable automatic discovery of type-constrained notification middleware | Only manually registered constrained middleware available |

#### Notification Handler Discovery Methods

| Method | Parameters | Purpose | Configuration Impact |
|--------|------------|---------|---------------------|
| `WithNotificationHandlerDiscovery()` | None | Enable automatic discovery of notification handlers | Scans assemblies for notification handler implementations |
| `WithoutNotificationHandlerDiscovery()` | None | Disable automatic discovery of notification handlers | Only manually registered notification handlers available |

#### Middleware Registration Methods

| Method | Parameters | Purpose | Configuration Impact |
|--------|------------|---------|---------------------|
| `AddMiddleware<TMiddleware>()` | Middleware type (generic) | Register specific middleware | Adds middleware to request pipeline |
| `AddMiddleware(Type)` | Middleware type | Register middleware by type | Dynamic middleware registration |
| `AddMiddleware(params Type[])` | Multiple middleware types | Register multiple middleware types | Adds multiple middleware maintaining order |
| `AddNotificationMiddleware<TMiddleware>()` | Notification middleware type (generic) | Register specific notification middleware | Adds middleware to notification pipeline |
| `AddNotificationMiddleware<TMiddleware>(object?)` | Middleware type + configuration | Register notification middleware with configuration | Adds configured middleware to notification pipeline |
| `AddNotificationMiddleware(Type)` | Notification middleware type | Register notification middleware by type | Dynamic notification middleware registration |
| `AddNotificationMiddleware(params Type[])` | Multiple notification middleware types | Register multiple notification middleware types | Adds multiple notification middleware maintaining order |

### Static Factory Methods

The `MediatorConfiguration` class also provides several static factory methods for creating pre-configured instances optimized for different environments:

| Method | Purpose | Configuration | Best For |
|--------|---------|---------------|----------|
| `Development(params Assembly[])` | Development environment configuration | Comprehensive features with detailed debugging information | Development and debugging scenarios |
| `Production(params Assembly[])` | Production environment configuration | Essential features with optimized performance settings | Production deployments |
| `Disabled(params Assembly[])` | Minimal configuration with features disabled | All optional features disabled for maximum performance | High-performance scenarios |
| `Minimal(params Assembly[])` | Minimal configuration with basic features | Basic features only with minimal overhead | Performance-critical applications |
| `NotificationOptimized(params Assembly[])` | Notification-focused configuration | Optimized for notification-centric applications | Event-driven architectures |
| `StreamingOptimized(params Assembly[])` | Streaming-focused configuration | Optimized for streaming applications | Real-time data processing applications |

### Utility Methods

| Method | Purpose | Configuration Impact |
|--------|---------|---------------------|
| `Validate()` | Validate current configuration | Returns list of validation error messages |
| `ValidateAndThrow()` | Validate configuration and throw if invalid | Throws ArgumentException for invalid configuration |
| `Clone()` | Create copy of current configuration | Returns new configuration instance with same values |

## Current Registration Examples (Updated from MEDIATOR_PATTERN_GUIDE.md)

### Basic Registration

```csharp
// Program.cs - Modern fluent configuration approach (RECOMMENDED)
builder.Services.AddMediator(config =>
{
    config.AddAssembly(typeof(Program).Assembly);
});

// Basic registration with no configuration
builder.Services.AddMediator();
```

### Multi-Assembly Registration

```csharp
// Register handlers from multiple assemblies using fluent configuration
builder.Services.AddMediator(config =>
{
    config.AddAssembly(typeof(Program).Assembly)                    // Current assembly (API)
          .AddAssembly(typeof(GetUserHandler).Assembly)             // Application layer
          .AddAssembly(typeof(User).Assembly);                      // Domain layer (if needed)
});

// With auto-discovery for middleware using fluent configuration
builder.Services.AddMediator(config =>
{
    config.WithMiddlewareDiscovery()
          .AddAssembly(typeof(Program).Assembly)                    // Current assembly (API)
          .AddAssembly(typeof(GetUserHandler).Assembly)             // Application layer
          .AddAssembly(typeof(LoggingMiddleware<,>).Assembly);      // Infrastructure layer
});
```

### Assembly Registration Variations

```csharp
// Method 1: Using assembly marker types with fluent configuration
builder.Services.AddMediator(config =>
{
    config.AddAssembly(typeof(GetUserHandler).Assembly)
          .AddAssembly(typeof(CreateOrderHandler).Assembly)
          .AddAssembly(typeof(UpdateProductHandler).Assembly);
});

// Method 1a: Using AddAssemblies with multiple marker types
builder.Services.AddMediator(config =>
{
    config.AddAssemblies(typeof(GetUserHandler), typeof(CreateOrderHandler), typeof(UpdateProductHandler));
});

// Method 2: Using assembly references with fluent configuration
builder.Services.AddMediator(config =>
{
    config.AddAssembly(Assembly.GetExecutingAssembly())
          .AddAssembly(typeof(ExternalHandler).Assembly);
});

// Method 2a: Using AddAssemblies with multiple assembly references
builder.Services.AddMediator(config =>
{
    config.AddAssemblies(Assembly.GetExecutingAssembly(), typeof(ExternalHandler).Assembly);
});
```

### Calling Assembly Registration (Obsolete)

```csharp
// Method 3: Using calling assembly
builder.Services.AddMediatorFromCallingAssembly();

// Method 3a: Using calling assembly with configuration
builder.Services.AddMediatorFromCallingAssembly(config =>
{
    config.WithMiddlewareDiscovery()
          .WithStatisticsTracking();
});
```

### Loaded Assemblies Registration (Obsolete)

```csharp
// Method 4: Using loaded assemblies with filter
builder.Services.AddMediatorFromLoadedAssemblies(assembly =>
    assembly.FullName?.StartsWith("MyCompany.") == true &&
    assembly.FullName.Contains(".Application"));

// Method 4a: Using loaded assemblies with configuration
builder.Services.AddMediatorFromLoadedAssemblies(config =>
{
    config.WithMiddlewareDiscovery()
          .WithStatisticsTracking();
}, assembly => assembly.FullName?.StartsWith("MyCompany.") == true);
```

### Middleware Configuration Examples

```csharp
// Basic middleware configuration
builder.Services.AddMediator(config =>
{
    config.AddMiddleware<GeneralLoggingMiddleware<,>>()
          .AddMiddleware<GeneralCommandLoggingMiddleware<>>()
          .AddAssembly(typeof(Program).Assembly);
});

// Conditional middleware configuration
builder.Services.AddMediator(config =>
{
    config.AddMiddleware<OrderLoggingMiddleware<,>>()
          .AddMiddleware<ProductLoggingMiddleware<,>>()
          .AddAssembly(typeof(Program).Assembly);
});

// Mixed middleware approach
builder.Services.AddMediator(config =>
{
    config.AddMiddleware<ValidationMiddleware<,>>()     // Global validation
          .AddMiddleware<OrderLoggingMiddleware<,>>()    // Conditional logging
          .AddMiddleware<ProductLoggingMiddleware<,>>()  // Conditional logging
          .AddMiddleware<CachingMiddleware<,>>()         // Global caching
          .AddAssembly(typeof(Program).Assembly);
});
```

### Auto-Discovery Configuration

```csharp
// Auto-discover all middleware using fluent configuration
builder.Services.AddMediator(config =>
{
    config.WithMiddlewareDiscovery()
          .AddAssembly(typeof(Program).Assembly);
});

// Auto-discover middleware from multiple assemblies
builder.Services.AddMediator(config =>
{
    config.WithMiddlewareDiscovery()
          .AddAssembly(typeof(Program).Assembly)                    // API layer
          .AddAssembly(typeof(GetUserHandler).Assembly)             // Application layer
          .AddAssembly(typeof(LoggingMiddleware<,>).Assembly);      // Infrastructure layer
});

// Mixed auto-discovery with manual configuration
builder.Services.AddMediator(config =>
{
    // Manually add specific middleware with custom configuration
    config.AddMiddleware<CustomAuthorizationMiddleware<,>>()
          .AddMiddleware<DatabaseTransactionMiddleware<,>>()
          // Auto-discover other middleware
          .WithMiddlewareDiscovery()
          .AddAssembly(typeof(Program).Assembly);
});
```

### Statistics and Telemetry Configuration

```csharp
// Enable statistics tracking
builder.Services.AddMediator(config =>
{
    config.WithStatisticsTracking()
          .AddAssembly(typeof(Program).Assembly);
});

// Custom statistics configuration
builder.Services.AddMediator(config =>
{
    config.WithStatisticsTracking(options =>
          {
              options.EnableRequestMetrics = true;
              options.EnableNotificationMetrics = true;
              options.EnableMiddlewareMetrics = true;
              options.EnablePerformanceCounters = true;
              options.EnableDetailedAnalysis = true;
              options.MetricsRetentionPeriod = TimeSpan.FromHours(1);
              options.CleanupInterval = TimeSpan.FromMinutes(15);
          })
          .AddAssembly(typeof(Program).Assembly);
});

// Telemetry configuration
builder.Services.AddMediator(config =>
{
    config.WithTelemetry(options => options.Enabled = true)
          .WithNotificationTelemetry()
          .WithHandlerChildSpans()
          .WithSubscriberMetrics()
          .AddAssembly(typeof(Program).Assembly);
});
```

### Logging Configuration

```csharp
// Basic logging configuration
builder.Services.AddMediator(config =>
{
    config.WithLogging()
          .AddAssembly(typeof(Program).Assembly);
});

// Custom logging configuration
builder.Services.AddMediator(config =>
{
    config.WithLogging(options => 
          {
              options.EnableDetailedHandlerInfo = true;
              options.EnableSend = true;
              options.EnablePublish = true;
              options.EnableNotificationMiddleware = true;
              options.EnableSubscriberDetails = true;
              options.EnableConstraintLogging = true;
          })
          .AddAssembly(typeof(Program).Assembly);
});
```

### Complete Application Setup Examples

```csharp
// For Controllers (Recommended for CRUD APIs)
using Blazing.Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Mediator with multiple assemblies using fluent configuration
builder.Services.AddMediator(config =>
{
    config.AddAssembly(typeof(Program).Assembly)
          .AddAssembly(typeof(GetUserHandler).Assembly);
});

var app = builder.Build();

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

```csharp
// For Minimal APIs (Recommended for Simple APIs)
using Blazing.Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Mediator using fluent configuration
builder.Services.AddMediator(config =>
{
    config.AddAssembly(typeof(Program).Assembly);
});

var app = builder.Build();

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

### Legacy Registration Methods (Deprecated)

> **Migration Note**: The following methods are marked as obsolete and should be migrated to the new fluent configuration approach for better type safety, enhanced functionality, and future-proofing.

```csharp
// DEPRECATED - Use fluent configuration instead
builder.Services.AddMediator(discoverMiddleware: true, typeof(Program).Assembly);
builder.Services.AddMediatorFromCallingAssembly(discoverMiddleware: true);
builder.Services.AddMediatorFromLoadedAssemblies(discoverMiddleware: true);

// MIGRATE TO:
builder.Services.AddMediator(config =>
{
    config.WithMiddlewareDiscovery()
          .AddAssembly(typeof(Program).Assembly);
});
