# OpenTelemetryExample - Blazing.Mediator OpenTelemetry Integration Sample

A comprehensive demonstration of OpenTelemetry integration with Blazing.Mediator, showcasing telemetry collection, metrics, tracing, and monitoring capabilities.

## ?? Features Demonstrated

- **? Complete OpenTelemetry Integration** - Metrics, tracing, and health checks
- **? Mediator Telemetry** - Comprehensive telemetry for all mediator operations
- **? Middleware Pipeline Telemetry** - Tracking middleware execution and performance
- **? Error and Exception Tracking** - Detailed error telemetry with sanitized data
- **? Health Checks** - Built-in health monitoring for telemetry systems
- **? Prometheus Metrics** - Metrics exported in Prometheus format
- **? Console Exporters** - Real-time telemetry output during development
- **? Validation Telemetry** - FluentValidation integration with telemetry
- **? Performance Monitoring** - Request duration and throughput metrics

## ?? OpenTelemetry Components

### Metrics Collected

| Metric Name | Type | Description |
|-------------|------|-------------|
| `mediator.send.duration` | Histogram | Duration of mediator send operations |
| `mediator.send.success` | Counter | Number of successful send operations |
| `mediator.send.failure` | Counter | Number of failed send operations |
| `mediator.publish.duration` | Histogram | Duration of notification publish operations |
| `mediator.publish.success` | Counter | Number of successful publish operations |
| `mediator.publish.failure` | Counter | Number of failed publish operations |
| `mediator.publish.subscriber.duration` | Histogram | Duration of individual subscriber processing |
| `mediator.publish.subscriber.success` | Counter | Number of successful subscriber notifications |
| `mediator.publish.subscriber.failure` | Counter | Number of failed subscriber notifications |
| `mediator.telemetry.health` | Counter | Health check counter for telemetry system |

### Tracing (Activities)

- **Request Processing** - Complete request/response lifecycle
- **Middleware Execution** - Individual middleware execution spans
- **Handler Execution** - Business logic execution spans
- **Error Tracking** - Exception details and stack traces
- **Validation Results** - Validation success/failure details

### Tags and Attributes

All telemetry includes relevant tags:
- `request_name` - The request type name (sanitized)
- `request_type` - "query" or "command"
- `response_type` - The response type name (for queries)
- `middleware.executed` - List of executed middleware
- `middleware.pipeline` - Complete middleware pipeline
- `handler.type` - The handler type name
- `exception.type` - Exception type (sanitized)
- `exception.message` - Exception message (sanitized)
- `validation.passed` - Validation result
- `performance.duration_ms` - Performance metrics

## ????? Running the Example

### Prerequisites

- .NET 9.0 or later
- Docker (optional, for Prometheus/Grafana)

### Basic Setup

1. **Clone and navigate to the sample:**
   ```bash
   cd samples/OpenTelemetryExample
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

3. **Open your browser:**
   - **Swagger UI**: https://localhost:7000/swagger
   - **Health Checks**: https://localhost:7000/health
   - **Prometheus Metrics**: https://localhost:7000/metrics
   - **Telemetry Health**: https://localhost:7000/telemetry/health
   - **Telemetry Info**: https://localhost:7000/telemetry/metrics

### Generating Telemetry Data

Use Swagger UI or curl to make requests:

```bash
# Get all users (Query)
curl -X GET "https://localhost:7000/api/users"

# Get specific user (Query)
curl -X GET "https://localhost:7000/api/users/1"

# Create user (Command)
curl -X POST "https://localhost:7000/api/users" \
  -H "Content-Type: application/json" \
  -d '{"name": "John Doe", "email": "john@example.com"}'

# Update user (Command)
curl -X PUT "https://localhost:7000/api/users/1" \
  -H "Content-Type: application/json" \
  -d '{"userId": 1, "name": "Jane Doe", "email": "jane@example.com"}'

# Delete user (Command)
curl -X DELETE "https://localhost:7000/api/users/1"

# Simulate validation error
curl -X POST "https://localhost:7000/api/users/simulate-validation-error"

# Simulate general error
curl -X POST "https://localhost:7000/api/users/simulate-error"
```

## ?? Monitoring and Observability

### Console Output

The application outputs telemetry data to the console in real-time:

```
info: OpenTelemetryExample.Application.Middleware.LoggingMiddleware[0]
      Handling request GetUserQuery
Activity.TraceId:            80000000000000000000000000000001
Activity.SpanId:             8000000000000002
Activity.TraceFlags:         Recorded
Activity.ParentSpanId:       8000000000000001
Activity.ActivitySourceName: Blazing.Mediator
Activity.DisplayName:        Mediator.Send:GetUserQuery
Activity.Kind:               Internal
Activity.StartTime:          2024-01-15T10:30:00.0000000Z
Activity.Duration:           00:00:00.0451234
Activity.Tags:
    request_name: GetUserQuery
    request_type: query
    handler.type: GetUserHandler
    duration_ms: 45.1234
Resource associated with Activity:
    service.name: OpenTelemetryExample
    service.version: 1.0.0
```

### Prometheus Metrics

Access metrics at `https://localhost:7000/metrics`:

```prometheus
# HELP mediator_send_duration Duration of mediator send operations
# TYPE mediator_send_duration histogram
mediator_send_duration_bucket{request_name="GetUserQuery",request_type="query",le="0.1"} 5
mediator_send_duration_bucket{request_name="GetUserQuery",request_type="query",le="0.25"} 8
mediator_send_duration_bucket{request_name="GetUserQuery",request_type="query",le="0.5"} 12

# HELP mediator_send_success Number of successful mediator send operations
# TYPE mediator_send_success counter
mediator_send_success{request_name="GetUserQuery",request_type="query"} 15

# HELP mediator_send_failure Number of failed mediator send operations
# TYPE mediator_send_failure counter
mediator_send_failure{request_name="GetUserQuery",request_type="query",exception_type="NotFoundException"} 2
```

### Health Checks

Health check endpoint at `https://localhost:7000/health`:

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0156832",
  "entries": {
    "mediator_telemetry": {
      "data": {
        "is_enabled": true,
        "can_record_metrics": true,
        "meter_name": "Blazing.Mediator",
        "activity_source_name": "Blazing.Mediator"
      },
      "description": "Telemetry is enabled and working correctly",
      "duration": "00:00:00.0123456",
      "status": "Healthy"
    }
  }
}
```

## ?? Configuration

### OpenTelemetry Configuration

The sample demonstrates comprehensive OpenTelemetry setup:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("Blazing.Mediator") // ? Blazing.Mediator metrics
            .AddMeter("OpenTelemetryExample")
            .AddConsoleExporter()
            .AddPrometheusExporter();
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("Blazing.Mediator") // ? Blazing.Mediator tracing
            .AddSource("OpenTelemetryExample")
            .AddConsoleExporter();
    });
```

### Mediator Telemetry Configuration

```csharp
// Enable telemetry with default settings
builder.Services.AddMediatorTelemetry();

// Or configure with custom options
builder.Services.ConfigureMediatorTelemetry(options =>
{
    options.Enabled = true;
    options.CaptureMiddlewareDetails = true;
    options.CaptureHandlerDetails = true;
    options.CaptureExceptionDetails = true;
    options.MaxExceptionMessageLength = 200;
    options.SensitiveDataPatterns.Add("custom_sensitive_pattern");
});

// Disable telemetry
builder.Services.DisableMediatorTelemetry();
```

## ??? Security and Privacy

### Sensitive Data Filtering

The implementation automatically sanitizes sensitive data:

- **Type Names**: Removes patterns like "Password", "Secret", "Token", "Key", "Auth"
- **Exception Messages**: Filters connection strings, file paths, and sensitive patterns
- **Stack Traces**: Removes file paths and limits to first 3 lines
- **Configurable Patterns**: Add custom sensitive data patterns

### Example Sanitization

```
Original: "LoginCommand with password 'secret123'"
Sanitized: "LoginCommand with *** 'sensitive_data_error'"

Original: "Connection failed: Server=localhost;Database=app;User=admin;Password=secret"
Sanitized: "connection_error"

Original: "File not found: C:\app\secrets\config.json"
Sanitized: "file_path_error"
```

## ?? Middleware Pipeline Telemetry

The sample demonstrates comprehensive middleware pipeline telemetry:

### Pipeline Execution Tracking

- **All Middleware**: Lists all registered middleware in execution order
- **Executed Middleware**: Tracks which middleware actually executed
- **Execution Order**: Demonstrates proper middleware ordering
- **Performance Impact**: Measures individual middleware performance

### Sample Pipeline Output

```
middleware.pipeline: ErrorHandlingMiddleware,ValidationMiddleware,LoggingMiddleware,PerformanceMiddleware
middleware.executed: ErrorHandlingMiddleware,ValidationMiddleware,LoggingMiddleware,PerformanceMiddleware
```

## ?? Key Telemetry Features

### 1. **Request Classification**
- Automatically distinguishes between queries and commands
- Uses interface-based detection (`IQuery<T>`, `ICommand`, `ICommand<T>`)
- Falls back to name-based detection for compatibility

### 2. **Exception Handling**
- Captures exception details safely
- Sanitizes sensitive information
- Provides structured error data

### 3. **Performance Monitoring**
- Tracks request duration
- Monitors middleware execution time
- Identifies performance bottlenecks

### 4. **Health Monitoring**
- Built-in health checks
- Real-time telemetry system status
- Integration with ASP.NET Core health checks

### 5. **Validation Tracking**
- FluentValidation integration
- Validation success/failure metrics
- Detailed validation error tracking

## ?? Advanced Scenarios

### Custom Metrics

Add application-specific metrics:

```csharp
public class CustomMetricsMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly Meter _meter = new("OpenTelemetryExample");
    private static readonly Counter<long> _customCounter = _meter.CreateCounter<long>("custom.requests");

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _customCounter.Add(1, new TagList { { "request_type", typeof(TRequest).Name } });
        return await next();
    }
}
```

### Custom Tracing

Add application-specific tracing:

```csharp
public class CustomTracingHandler : IRequestHandler<GetUserQuery, UserDto>
{
    private static readonly ActivitySource _activitySource = new("OpenTelemetryExample");

    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("Database.GetUser");
        activity?.SetTag("user.id", request.UserId);
        
        // Your handler logic here
        await Task.Delay(50, cancellationToken);
        
        return new UserDto { Id = request.UserId, Name = "Test User" };
    }
}
```

## ?? Docker and Production

### Prometheus Configuration

```yaml
# prometheus.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'opentelemetry-example'
    static_configs:
      - targets: ['localhost:7000']
    metrics_path: '/metrics'
    scrape_interval: 5s
```

### Docker Compose

```yaml
version: '3.8'
services:
  app:
    build: .
    ports:
      - "7000:8080"
  
  prometheus:
    image: prom/prometheus
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
  
  grafana:
    image: grafana/grafana
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
```

## ?? Learn More

- [Blazing.Mediator Documentation](../../docs/MEDIATOR_PATTERN_GUIDE.md)
- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/languages/net/)
- [Prometheus Metrics](https://prometheus.io/docs/concepts/metric_types/)
- [ASP.NET Core Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

## ?? Summary

This sample demonstrates a production-ready OpenTelemetry integration with Blazing.Mediator, providing:

- **Complete observability** into mediator operations
- **Secure telemetry** with sensitive data filtering
- **Production-ready** health checks and monitoring
- **Performance insights** through detailed metrics
- **Easy integration** with existing monitoring infrastructure

The implementation serves as a comprehensive reference for integrating OpenTelemetry with Blazing.Mediator in real-world applications.

## ?? Implementation Checklist

### Core OpenTelemetry Integration
- [x] **Metrics Collection** - Complete histogram and counter metrics for all operations
- [x] **Distributed Tracing** - Activity creation and tagging for request tracking
- [x] **Health Checks** - Built-in telemetry system health monitoring
- [x] **Configuration API** - Easy setup and configuration methods
- [x] **Security & Privacy** - Sensitive data sanitization and filtering

### Sample Project Components
- [x] **ASP.NET Core Web API** - REST endpoints with OpenTelemetry integration
- [x] **Middleware Pipeline** - Error handling, validation, logging, performance
- [x] **FluentValidation** - Validation telemetry integration
- [x] **Prometheus Export** - Metrics endpoint for scraping
- [x] **Console Export** - Real-time development telemetry output
- [ ] **Blazor WebAssembly** - Client-side telemetry demonstration
- [ ] **Aspire Integration** - .NET Aspire orchestration and telemetry
- [ ] **Unit Tests** - Comprehensive test coverage for telemetry

### Documentation & Examples
- [x] **Comprehensive README** - Setup, configuration, and usage examples
- [x] **API Documentation** - XML docs and inline code examples
- [x] **Production Guidance** - Docker, Prometheus, Grafana configuration
- [x] **Security Best Practices** - Sensitive data handling guidelines

### Missing Components (To Be Added)
- [ ] **Blazor WebAssembly Client** - Demonstrate client-side telemetry
- [ ] **Aspire Host Project** - Service orchestration with telemetry
- [ ] **Test Suite** - Unit and integration tests for OpenTelemetry features
- [ ] **Performance Benchmarks** - Telemetry overhead measurements