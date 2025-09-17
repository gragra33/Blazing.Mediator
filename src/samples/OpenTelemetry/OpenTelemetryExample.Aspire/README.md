# OpenTelemetry Example with Aspire

This sample demonstrates a comprehensive OpenTelemetry implementation with Blazing.Mediator using .NET Aspire for orchestration and distributed tracing.

## Architecture

### Components

1. **OpenTelemetryExample (Web API Server)**
   - ASP.NET Core Web API with Blazing.Mediator
   - OpenTelemetry instrumentation for all mediator operations
   - Comprehensive middleware pipeline demonstrating telemetry tracking
   - Health checks and telemetry endpoints

2. **OpenTelemetryExample.Client (Blazor WebAssembly)**
   - Blazor WebAssembly client consuming the API
   - Client-side OpenTelemetry for HTTP requests
   - Interactive UI for testing telemetry scenarios

3. **OpenTelemetryExample.Aspire (Aspire Host)**
   - .NET Aspire orchestration
   - Distributed tracing and metrics collection
   - Centralized observability dashboard

## Features Demonstrated

### OpenTelemetry Instrumentation

- **Send Operations**: Command and query telemetry with timing, success/failure tracking
- **Publish Operations**: Notification telemetry with subscriber tracking
- **Streaming Operations**: Stream request telemetry with item count tracking
- **Middleware Pipeline**: Tracking of executed middleware components
- **Exception Handling**: Error telemetry with stack trace preservation
- **Sensitive Data Sanitization**: PII scrubbing in telemetry data

### Middleware Types

1. **Standard Middleware**: `IRequestMiddleware<T>` and `IRequestMiddleware<T, TResponse>`
2. **Conditional Middleware**: `IConditionalMiddleware<T>` with execution conditions
3. **Type-Constrained Middleware**: Middleware with generic type constraints

### Short-Circuiting Scenarios

- **Logic-based**: Conditional middleware not executing based on request data
- **Exception-based**: Pipeline termination due to middleware exceptions
- **Telemetry Impact**: Only executed components are tracked in telemetry

## Running the Example

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code with .NET Aspire workload

### Steps

1. **Start the Aspire Host**:
   ```bash
   cd samples/OpenTelemetry/OpenTelemetryExample.Aspire
   dotnet run
   ```

2. **Access the Applications**:
   - **Aspire Dashboard**: https://localhost:15888 (check console output for exact URL)
   - **API Server**: Will be assigned a dynamic port by Aspire
   - **Blazor Client**: Will be assigned a dynamic port by Aspire

3. **Monitor Telemetry**:
   - Use the Aspire dashboard to view distributed traces and metrics
   - Navigate through the Blazor client to generate telemetry data
   - Check the API health endpoints for telemetry status

## Testing Scenarios

### Basic Operations

1. **User Management**:
   - Create, read, update, delete users
   - Search and filter operations
   - Triggers various command and query telemetry

2. **Error Scenarios**:
   - Validation errors
   - Not found exceptions
   - Generates error telemetry with proper exception tracking

3. **Performance Testing**:
   - Bulk operations
   - Concurrent requests
   - Shows telemetry overhead measurement

### Middleware Demonstrations

1. **Full Pipeline Execution**:
   - All middleware execute successfully
   - Complete telemetry tracking

2. **Exception Short-Circuiting**:
   - Middleware throws exception
   - Only executed middleware tracked in telemetry

3. **Conditional Skipping**:
   - Conditional middleware skipped based on request data
   - Telemetry reflects actual execution path

## Telemetry Endpoints

### Health and Diagnostics

- `GET /health` - Application health checks
- `GET /telemetry/health` - Telemetry system health
- `GET /telemetry/metrics` - Telemetry configuration info

### Metrics Endpoints

- `/metrics` - Prometheus-formatted metrics
- Includes custom Blazing.Mediator metrics:
  - `mediator.send.duration` - Send operation timing
  - `mediator.send.success` - Successful send operations
  - `mediator.send.failure` - Failed send operations
  - `mediator.publish.duration` - Publish operation timing
  - `mediator.publish.success` - Successful publish operations
  - `mediator.publish.failure` - Failed publish operations

## Observability Features

### Distributed Tracing

- End-to-end request tracing from Blazor client through Web API
- Mediator operation spans with detailed metadata
- Middleware execution tracking
- Exception correlation across service boundaries

### Metrics Collection

- Custom metrics for mediator operations
- Performance counters for middleware execution
- Error rates and success rates
- Resource utilization metrics

### Logging Integration

- Structured logging with correlation IDs
- Log level configuration per component
- Integration with OpenTelemetry logging pipeline

## Development and Debugging

### Local Development

1. **Individual Components**:
   - Run each project independently for focused development
   - API: `dotnet run --project samples/OpenTelemetry/OpenTelemetryExample`
   - Client: `dotnet run --project samples/OpenTelemetry/OpenTelemetryExample.Client`

2. **Aspire Development**:
   - Use Aspire for integrated testing
   - Automatic service discovery and configuration
   - Centralized logging and telemetry

### Telemetry Debugging

1. **Console Output**: Check application console for OpenTelemetry logs
2. **Aspire Dashboard**: Real-time telemetry visualization
3. **Health Endpoints**: Verify telemetry system status
4. **Metrics Export**: Prometheus metrics for external monitoring

## Configuration

### OpenTelemetry Configuration

The example demonstrates various OpenTelemetry configurations:

```csharp
// Server-side (OpenTelemetryExample)
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("Blazing.Mediator")
            .AddConsoleExporter()
            .AddPrometheusExporter();
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("Blazing.Mediator")
            .AddConsoleExporter();
    });

// Client-side (OpenTelemetryExample.Client)
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddHttpClientInstrumentation()
            .AddSource("Blazing.Mediator")
            .AddConsoleExporter();
    });
```

### Mediator Configuration

```csharp
// Enable telemetry
builder.Services.AddMediatorTelemetry();

// Configure middleware pipeline
builder.Services.AddMediator(config =>
{
    config.AddMiddleware(typeof(ErrorHandlingMiddleware<,>));
    config.AddMiddleware(typeof(ValidationMiddleware<,>));
    config.AddMiddleware(typeof(LoggingMiddleware<,>));
    config.AddMiddleware(typeof(PerformanceMiddleware<,>));
}, typeof(Program).Assembly);
```

## Best Practices Demonstrated

1. **Telemetry Design**:
   - Only track executed components
   - Sanitize sensitive data
   - Use structured tags and attributes

2. **Performance Considerations**:
   - Minimal telemetry overhead
   - Efficient data collection
   - Configurable sampling rates

3. **Error Handling**:
   - Preserve exception stack traces
   - Track error propagation
   - Maintain telemetry during failures

4. **Distributed Architecture**:
   - Service correlation
   - End-to-end tracing
   - Centralized observability

This sample provides a comprehensive reference for implementing OpenTelemetry with Blazing.Mediator in production applications, demonstrating best practices for observability, performance monitoring, and distributed tracing.