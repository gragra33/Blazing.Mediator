# OpenTelemetry Example - Blazing.Mediator Integration

A comprehensive demonstration of OpenTelemetry integration with Blazing.Mediator, showcasing telemetry collection, metrics, tracing, and monitoring capabilities across a full-stack .NET 9 application.

## Table of Contents

1. [Solution Architecture](#solution-architecture)
2. [Features Demonstrated](#features-demonstrated)
3. [Prerequisites](#prerequisites)
4. [Running the Application](#running-the-application)
   - [Option 1: .NET Aspire App Host (Recommended)](#option-1-net-aspire-app-host-recommended)
   - [Option 2: Manual API Server + Blazor Client](#option-2-manual-api-server--blazor-client)
   - [Option 3: API Server Only + Swagger UI](#option-3-api-server-only--swagger-ui)
   - [Option 4: API Server + HTTP File Testing](#option-4-api-server--http-file-testing)
   - [Option 5: Visual Studio Code](#option-5-visual-studio-code)
5. [Exploring the Application](#exploring-the-application)
6. [OpenTelemetry Components](#opentelemetry-components)
7. [Monitoring and Observability](#monitoring-and-observability)
8. [Configuration](#configuration)
9. [Testing Scenarios](#testing-scenarios)
10. [Security and Privacy](#security-and-privacy)
11. [Advanced Scenarios](#advanced-scenarios)
12. [Docker and Production](#docker-and-production)
13. [Troubleshooting](#troubleshooting)
14. [Learn More](#learn-more)

## Solution Architecture

This sample consists of three main projects:

1. **OpenTelemetryExample** - ASP.NET Core Web API with comprehensive OpenTelemetry integration
2. **OpenTelemetryExample.Client** - Blazor WebAssembly client application with client-side telemetry
3. **OpenTelemetryExample.AppHost** - .NET Aspire App Host for orchestration and service discovery

## Features Demonstrated

### OpenTelemetry Integration

- **Complete OpenTelemetry Integration** - Metrics, tracing, and health checks
- **Mediator Telemetry** - Comprehensive telemetry for all mediator operations
- **Middleware Pipeline Telemetry** - Tracking middleware execution and performance
- **Error and Exception Tracking** - Detailed error telemetry with sanitized data
- **Health Checks** - Built-in health monitoring for telemetry systems
- **Prometheus Metrics** - Metrics exported in Prometheus format
- **Console Exporters** - Real-time telemetry output during development
- **Validation Telemetry** - FluentValidation integration with telemetry
- **Performance Monitoring** - Request duration and throughput metrics

### CQRS with Blazing.Mediator

- **Commands & Queries** - Complete CRUD operations
- **Middleware Pipeline** - Validation, logging, and telemetry middleware
- **Notifications** - Event-driven architecture examples
- **Error Handling** - Comprehensive exception management

### Modern .NET Stack

- **.NET 9** - Latest framework features
- **Blazor WebAssembly** - Modern client-side SPA
- **Bootstrap 5** - Responsive UI framework
- **.NET Aspire** - Cloud-native orchestration

## Prerequisites

- .NET 9.0 or later
- Docker (optional, for Prometheus/Grafana)
- Visual Studio 2022, Visual Studio Code, or JetBrains Rider (optional, for HTTP file testing)

## Running the Application

### Option 1: .NET Aspire App Host (Recommended)

This is the easiest way to run the complete solution with proper service discovery and telemetry collection.

1. **Navigate to the OpenTelemetry sample root:**
   ```bash
   cd src/samples/OpenTelemetry
   ```

2. **Run the Aspire App Host:**
   ```bash
   dotnet run --project OpenTelemetryExample.AppHost
   ```

3. **Access the applications:**
   - **Aspire Dashboard**: Check the console output for the dashboard URL (typically https://localhost:15888)
   - **API Server**: Will be assigned a port by Aspire (check dashboard for exact URL)
   - **Blazor Client**: Will be assigned a port by Aspire (check dashboard for exact URL)

**Benefits:**
- Automatic service discovery between API and client
- Centralized dashboard for monitoring both services
- Automatic port assignment and routing
- Integrated OpenTelemetry across the distributed application

### Option 2: Manual API Server + Blazor Client

Run both the API server and Blazor WebAssembly client manually for full-stack development.

1. **Start the API Server:**
   ```bash
   cd src/samples/OpenTelemetry/OpenTelemetryExample
   dotnet run
   ```

   The API will be available at:
   - HTTPS: `https://localhost:7000`
   - HTTP: `http://localhost:5000`

2. **Start the Blazor Client (in a new terminal):**
   ```bash
   cd src/samples/OpenTelemetry/OpenTelemetryExample.Client
   dotnet run
   ```

   The client will be available at:
   - HTTPS: `https://localhost:7001`
   - HTTP: `http://localhost:5001`

**Benefits:**
- Full user interface for testing all features
- Real-time telemetry monitoring through the web client
- Interactive demo scenarios
- Complete CRUD operations with visual feedback

### Option 3: API Server Only + Swagger UI

Perfect for API development and testing without the client overhead.

1. **Start the API Server:**
   ```bash
   cd src/samples/OpenTelemetry/OpenTelemetryExample
   dotnet run
   ```

2. **Access Swagger UI:**
   - **Swagger UI**: https://localhost:7000/swagger
   - **OpenAPI JSON**: https://localhost:7000/swagger/v1/swagger.json

**Available Endpoints:**
- **API Base**: https://localhost:7000/api
- **Health Checks**: https://localhost:7000/health
- **Prometheus Metrics**: https://localhost:7000/metrics
- **Telemetry Health**: https://localhost:7000/telemetry/health
- **Telemetry Info**: https://localhost:7000/telemetry/metrics

**Benefits:**
- Interactive API documentation
- Built-in request/response testing
- Schema validation
- No additional tools required

### Option 4: API Server + HTTP File Testing

Use the provided HTTP file for comprehensive API testing scenarios.

1. **Start the API Server:**
   ```bash
   cd src/samples/OpenTelemetry/OpenTelemetryExample
   dotnet run
   ```

2. **Use the HTTP file:**
   - Open `OpenTelemetryExample.http` in your IDE
   - Execute requests directly from the file
   - Available in VS Code (with REST Client extension) or JetBrains IDEs

**Sample HTTP requests:**
```http
### Get all users (Query)
GET https://localhost:7000/api/users

### Get specific user (Query)
GET https://localhost:7000/api/users/1

### Create user (Command)
POST https://localhost:7000/api/users
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com"
}

### Update user (Command)
PUT https://localhost:7000/api/users/1
Content-Type: application/json

{
  "userId": 1,
  "name": "Jane Doe",
  "email": "jane@example.com"
}

### Delete user (Command)
DELETE https://localhost:7000/api/users/1

### Simulate validation error
POST https://localhost:7000/api/users/simulate-validation-error

### Simulate general error
POST https://localhost:7000/api/users/simulate-error
```

**Benefits:**
- Comprehensive API testing scenarios
- Automated testing workflows
- Load testing patterns
- Error simulation endpoints
- IDE integration

### Option 5: Visual Studio Code

If you're using VS Code with the provided launch configurations:

1. Open the workspace in VS Code
2. Go to the Run and Debug view (Ctrl+Shift+D)
3. Select "Run OpenTelemetry Example" from the dropdown
4. Click the play button to start both applications

## Exploring the Application

### API Documentation

Before exploring the application, review the complete API documentation:

- **Swagger UI** (`https://localhost:7000/swagger`) - Interactive documentation where you can:
  - Browse all available endpoints
  - Test API operations directly from the browser
  - View request/response schemas
  - Understand authentication requirements

### Web Client Features

Navigate to the Blazor client application and explore:

1. **Home Page** - Overview of OpenTelemetry features and architecture
2. **Users** - Complete CRUD interface demonstrating:
   - Create, read, update, delete operations
   - Form validation with telemetry
   - Error handling with trace correlation
3. **Telemetry Dashboard** - Real-time monitoring showing:
   - API health status
   - Telemetry configuration
   - Interactive testing scenarios
4. **Demo** - Interactive telemetry generation:
   - Successful operations
   - Error scenarios for testing
   - Performance load testing

## OpenTelemetry Components

### Metrics Collected

| Metric Name                            | Type      | Description                                   |
| -------------------------------------- | --------- | --------------------------------------------- |
| `mediator.send.duration`               | Histogram | Duration of mediator send operations          |
| `mediator.send.success`                | Counter   | Number of successful send operations          |
| `mediator.send.failure`                | Counter   | Number of failed send operations              |
| `mediator.publish.duration`            | Histogram | Duration of notification publish operations   |
| `mediator.publish.success`             | Counter   | Number of successful publish operations       |
| `mediator.publish.failure`             | Counter   | Number of failed publish operations           |
| `mediator.publish.subscriber.duration` | Histogram | Duration of individual subscriber processing  |
| `mediator.publish.subscriber.success`  | Counter   | Number of successful subscriber notifications |
| `mediator.publish.subscriber.failure`  | Counter   | Number of failed subscriber notifications     |
| `mediator.telemetry.health`            | Counter   | Health check counter for telemetry system    |

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

## Monitoring and Observability

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

## Configuration

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

### Aspire App Host Configuration

The `OpenTelemetryExample.AppHost` project uses the latest .NET Aspire SDK to orchestrate the solution:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.OpenTelemetryExample>("OpenTelemetry-api-server")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.OpenTelemetryExample_Client>("OpenTelemetry-blazor-client")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
```

### Environment Variables

Set these for advanced scenarios:

- `OTEL_EXPORTER_OTLP_ENDPOINT` - OTLP collector endpoint
- `OTEL_SERVICE_NAME` - Override service name
- `OTEL_RESOURCE_ATTRIBUTES` - Additional resource attributes

## Testing Scenarios

### Performance Testing

The demo page includes scenarios for:

1. **Successful Operations** - Normal CRUD workflows
2. **Error Scenarios** - Validation and exception testing
3. **Load Testing** - Concurrent operation patterns

### Telemetry Validation

Test telemetry collection by:

1. Performing various operations through the UI
2. Using the HTTP file for automated testing
3. Running the interactive demo scenarios
4. Monitoring the console output for telemetry data

## Security and Privacy

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

## Advanced Scenarios

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

## Docker and Production

### Prometheus Configuration

```yaml
# prometheus.yml
global:
    scrape_interval: 15s

scrape_configs:
    - job_name: "opentelemetry-example"
      static_configs:
          - targets: ["localhost:7000"]
      metrics_path: "/metrics"
      scrape_interval: 5s
```

### Docker Compose

```yaml
version: "3.8"
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

## Troubleshooting

### Common Issues

1. **Port Conflicts**: Ensure ports 5000-5001 and 7000-7001 are available
2. **HTTPS Certificates**: Run `dotnet dev-certs https --trust` if needed
3. **Build Errors**: Clean and rebuild with `dotnet clean && dotnet build`

### Telemetry Not Appearing

1. Check the console output for telemetry exports
2. Verify OpenTelemetry health at `/telemetry/health`
3. Ensure the service is properly configured in `appsettings.json`

### API Connectivity Issues

1. Verify the API server is running and accessible
2. Check the client configuration for correct API base URL
3. Test API endpoints directly via Swagger UI or HTTP file

## Learn More

- [Blazing.Mediator Documentation](../../../README.md)
- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/languages/net/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Blazor WebAssembly Guide](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Prometheus Metrics](https://prometheus.io/docs/concepts/metric_types/)
- [ASP.NET Core Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

---

This example showcases the power of combining modern .NET technologies with comprehensive observability through OpenTelemetry and CQRS patterns with Blazing.Mediator. The solution demonstrates production-ready telemetry integration with multiple deployment and testing options to suit different development workflows.
