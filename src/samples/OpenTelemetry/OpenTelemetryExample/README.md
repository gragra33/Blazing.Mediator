# OpenTelemetry Example - Blazing.Mediator Integration

A comprehensive demonstration of OpenTelemetry integration with Blazing.Mediator, showcasing telemetry collection, metrics, tracing, **structured logging**, real-time streaming, and monitoring capabilities across a full-stack .NET 9 application.

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

This sample consists of four main projects:

1. **OpenTelemetryExample** - ASP.NET Core Web API with comprehensive OpenTelemetry integration
2. **OpenTelemetryExample.Client** - Blazor WebAssembly client application with real-time telemetry dashboard
3. **OpenTelemetryExample.Shared** - Shared DTOs and models between API and client
4. **OpenTelemetryExample.AppHost** - .NET Aspire App Host for orchestration and service discovery

## Features Demonstrated

### OpenTelemetry Integration

- **Complete OpenTelemetry Integration** - Metrics, tracing, and health checks
- **Structured Logging** - Serilog integration with OpenTelemetry for comprehensive log collection
- **Log Correlation** - Automatic correlation between logs, traces, and spans
- **Blazing.Mediator Telemetry** - Comprehensive telemetry for all mediator operations including streaming
- **Middleware Pipeline Telemetry** - Tracking middleware execution and performance
- **Streaming Operations Telemetry** - Packet-level visibility for streaming operations
- **Error and Exception Tracking** - Detailed error telemetry with sanitized data
- **Health Checks** - Built-in health monitoring for telemetry systems
- **Real-time Data Collection** - Live metrics, traces, and logs with custom processors and readers
- **Console Exporters** - Real-time telemetry output during development
- **OTLP Exporters** - Integration with Aspire dashboard and external systems
- **Validation Telemetry** - FluentValidation integration with telemetry
- **Performance Monitoring** - Request duration and throughput metrics

### CQRS with Blazing.Mediator

- **Commands & Queries** - Complete CRUD operations with telemetry
- **Streaming Operations** - Real-time data streaming through mediator
- **Middleware Pipeline** - Validation, logging, performance, and telemetry middleware
- **Notifications** - Event-driven architecture examples
- **Error Handling** - Comprehensive exception management with telemetry

### Real-time Streaming Features

- **HTTP Streaming** - IAsyncEnumerable-based streaming endpoints
- **SignalR Integration** - Real-time bidirectional streaming
- **Server-Sent Events (SSE)** - Live data streaming to web clients
- **Streaming Telemetry** - Comprehensive packet-level telemetry for streaming operations
- **Multiple Streaming Patterns** - Various patterns for different use cases

### Modern .NET Stack

- **.NET 9** - Latest framework features
- **Blazor WebAssembly** - Modern client-side SPA with real-time dashboard
- **Entity Framework Core** - In-memory database for demonstrations
- **FluentValidation** - Validation with telemetry integration
- **SignalR** - Real-time communication
- **Serilog** - Structured logging with OpenTelemetry integration
- **.NET Aspire** - Cloud-native orchestration with enhanced API documentation

## Prerequisites

- .NET 9.0 or later
- Docker (optional, for external monitoring tools)
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
- Integrated OpenTelemetry collection and visualization
- Enhanced API documentation through Swagger integration

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
- Interactive streaming demonstrations
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
- **Telemetry Health**: https://localhost:7000/telemetry/health
- **Live Metrics**: https://localhost:7000/telemetry/live-metrics
- **Recent Traces**: https://localhost:7000/telemetry/traces
- **Grouped Traces**: https://localhost:7000/telemetry/traces/grouped
- **Recent Logs**: https://localhost:7000/api/logs/recent
- **Log Details**: https://localhost:7000/api/logs/{id}
- **Logs Summary**: https://localhost:7000/api/logs/summary
- **Streaming Health**: https://localhost:7000/api/streaming/health

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

### Stream users data
GET https://localhost:7000/api/streaming/users?count=10&delayMs=500

### Get live telemetry metrics
GET https://localhost:7000/telemetry/live-metrics

### Get recent traces
GET https://localhost:7000/telemetry/traces?maxRecords=20

### Get recent logs
GET https://localhost:7000/api/logs/recent?timeWindowMinutes=30&pageSize=20

### Get logs with filtering
GET https://localhost:7000/api/logs/recent?appOnly=true&errorsOnly=true&minLogLevel=Warning

### Get specific log details
GET https://localhost:7000/api/logs/1

### Get logs summary
GET https://localhost:7000/api/logs/summary?timeWindowMinutes=60

### Simulate validation error
POST https://localhost:7000/api/users/simulate-validation-error

### Test notifications
POST https://localhost:7000/testing/notifications
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
  - Browse all available endpoints including streaming endpoints
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
3. **Streaming** - Real-time streaming demonstrations:
   - HTTP streaming with IAsyncEnumerable
   - SignalR real-time communication
   - Server-Sent Events (SSE) streaming
   - Interactive streaming controls
4. **Telemetry Dashboard** - Real-time monitoring showing:
   - Live metrics and performance data
   - Recent traces with filtering options
   - Recent logs with filtering and search capabilities
   - Grouped traces for better visualization
   - Log details with exception information and trace correlation
   - API health status
   - Telemetry configuration details
   - Interactive testing scenarios
5. **Demo** - Interactive telemetry generation:
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
| `mediator.sendstream.duration`         | Histogram | Duration of streaming operations              |
| `mediator.sendstream.packet.duration`  | Histogram | Duration of individual packet processing      |
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
- **Streaming Operations** - Individual packet-level spans for streaming
- **SignalR Activities** - Real-time communication tracing
- **Error Tracking** - Exception details and stack traces
- **Validation Results** - Validation success/failure details

### Structured Logging

- **Log Correlation** - Automatic correlation with traces and spans using TraceId and SpanId
- **Application Logs** - Comprehensive logging from application components
- **Mediator Logs** - Detailed logging from Blazing.Mediator operations
- **Exception Logging** - Detailed exception information with context
- **Performance Logging** - Request duration and performance metrics
- **Validation Logging** - FluentValidation results and details
- **Middleware Logging** - Pipeline execution logging
- **Database Storage** - Logs stored in ApplicationDbContext for analysis
- **OpenTelemetry Integration** - Logs sent to OTLP endpoint for Aspire dashboard
- **Filtering and Search** - Advanced filtering by level, source, time, and content

### Tags and Attributes

All telemetry includes relevant tags:

- `request_name` - The request type name (sanitized)
- `request_type` - "query", "command", or "stream"
- `response_type` - The response type name (for queries)
- `stream.packet.number` - Packet number for streaming operations
- `stream.batch.id` - Batch identifier for streaming operations
- `middleware.executed` - List of executed middleware
- `middleware.pipeline` - Complete middleware pipeline
- `handler.type` - The handler type name
- `signalr.connection_id` - SignalR connection identifier
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
Activity.DisplayName:        Mediator.SendStream:StreamUsersQuery
Activity.Kind:               Internal
Activity.StartTime:          2024-01-15T10:30:00.0000000Z
Activity.Duration:           00:00:00.0451234
Activity.Tags:
    request_name: StreamUsersQuery
    request_type: stream
    handler.type: StreamUsersHandler
    stream.packet.count: 10
    duration_ms: 45.1234
```

### Real-time Data Collection

The application includes custom OpenTelemetry processors and readers:

- **OpenTelemetryActivityProcessor** - Captures and stores trace data in memory
- **OpenTelemetryMetricsReader** - Collects and stores metrics data
- **Live Dashboard Integration** - Real-time updates through API endpoints

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

### Telemetry Health Check

Specific telemetry health at `https://localhost:7000/telemetry/health`:

```json
{
    "isHealthy": true,
    "isEnabled": true,
    "canRecordMetrics": true,
    "meterName": "Blazing.Mediator",
    "activitySourceName": "Blazing.Mediator",
    "message": "Telemetry is enabled and working correctly"
}
```

## Configuration

### OpenTelemetry Configuration

The sample demonstrates comprehensive OpenTelemetry setup:

```csharp
services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("Blazing.Mediator") // Blazing.Mediator metrics
            .AddMeter("OpenTelemetryExample")
            .AddMeter("OpenTelemetryExample.Controller")
            .AddMeter("OpenTelemetryExample.Handler")
            .AddMeter("OpenTelemetryExample.Mediator")
            .AddReader(serviceProvider.GetRequiredService<OpenTelemetryMetricsReader>())
            .AddConsoleExporter() // Development only
            .AddOtlpExporter(); // Aspire integration
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("Blazing.Mediator") // Blazing.Mediator tracing
            .AddSource("OpenTelemetryExample")
            .AddSource("OpenTelemetryExample.Controller")
            .AddSource("OpenTelemetryExample.Handler")
            .AddSource("OpenTelemetryExample.Mediator")
            .AddProcessor(serviceProvider.GetRequiredService<OpenTelemetryActivityProcessor>())
            .AddConsoleExporter() // Development only
            .AddOtlpExporter(); // Aspire integration
    });
```

### Mediator Telemetry Configuration

```csharp
// Enable telemetry with environment-specific settings
if (environment.IsDevelopment())
{
    services.AddMediatorTelemetryWithFullVisibility();
}
else
{
    services.AddMediatorTelemetryForProduction();
}

// Configure streaming telemetry
services.AddMediatorStreamingTelemetry(
    enablePacketLevelTelemetry: true,
    batchSize: environment.IsDevelopment() ? 1 : 10
);

// Add comprehensive middleware pipeline
services.AddMediator(config =>
{
    config.AddMiddleware(typeof(TracingMiddleware<,>));
    config.AddMiddleware(typeof(StreamingTracingMiddleware<,>));
    config.AddMiddleware(typeof(StreamingPerformanceMiddleware<,>));
    config.AddMiddleware(typeof(ErrorHandlingMiddleware<,>));
    config.AddMiddleware(typeof(ValidationMiddleware<,>));
    config.AddMiddleware(typeof(LoggingMiddleware<,>));
    config.AddMiddleware(typeof(PerformanceMiddleware<,>));
}, typeof(Program).Assembly);
```

### Serilog Configuration

The sample demonstrates comprehensive Serilog setup with OpenTelemetry integration:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "OpenTelemetryExample")
    .Enrich.WithProperty("Environment", environment.EnvironmentName)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}")
    .WriteTo.Conditional(
        condition => !string.IsNullOrEmpty(otlpEndpoint),
        configureSink => configureSink.OpenTelemetry(options =>
        {
            options.Endpoint = otlpEndpoint ?? "";
            options.IncludedData = IncludedData.TraceIdField | 
                                  IncludedData.SpanIdField |
                                  IncludedData.SourceContextAttribute;
            options.ResourceAttributes = new Dictionary<string, object>
            {
                ["service.name"] = "OpenTelemetryExample",
                ["service.version"] = "1.0.0"
            };
        }))
    .CreateLogger();

services.AddSerilog();

// Custom database logging provider for telemetry storage
services.AddSingleton<TelemetryDatabaseLoggingProvider>();
services.AddLogging(builder =>
{
    builder.Services.AddSingleton<ILoggerProvider>(serviceProvider => 
        serviceProvider.GetRequiredService<TelemetryDatabaseLoggingProvider>());
});
```

### Aspire App Host Configuration

The `OpenTelemetryExample.AppHost` project uses .NET Aspire with enhanced API documentation:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> apiService = builder
    .AddProject<Projects.OpenTelemetryExample>("OpenTelemetry-api-server")
    .WithSwagger(); // Enhanced API documentation

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

The demo page and HTTP file include scenarios for:

1. **Successful Operations** - Normal CRUD workflows
2. **Streaming Operations** - Real-time data streaming tests
3. **Error Scenarios** - Validation and exception testing
4. **Load Testing** - Concurrent operation patterns
5. **SignalR Testing** - Real-time communication testing

### Telemetry Validation

Test telemetry collection by:

1. Performing various operations through the UI
2. Using the HTTP file for automated testing
3. Running the interactive demo scenarios
4. Monitoring the console output for telemetry data
5. Viewing real-time data in the telemetry dashboard
6. Testing streaming operations with packet-level visibility

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

### Streaming Telemetry

The implementation includes comprehensive streaming telemetry:

```csharp
public class StreamingTracingMiddleware<TRequest, TResponse> : IStreamRequestMiddleware<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    public async IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request, 
        StreamRequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Mediator.SendStream");
        var packetNumber = 0;
        
        await foreach (var response in next())
        {
            packetNumber++;
            activity?.AddEvent(new ActivityEvent($"stream.packet.{packetNumber}"));
            yield return response;
        }
        
        activity?.SetTag("stream.total_packets", packetNumber);
    }
}
```

## Docker and Production

### Docker Compose with External Monitoring

```yaml
version: "3.8"
services:
    app:
        build: .
        ports:
            - "7000:8080"
        environment:
            - OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:14268

    jaeger:
        image: jaegertracing/all-in-one:latest
        ports:
            - "16686:16686"
            - "14268:14268"

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
4. **SignalR Connection Issues**: Check CORS configuration and firewall settings

### Telemetry Not Appearing

1. Check the console output for telemetry exports
2. Verify OpenTelemetry health at `/telemetry/health`
3. Ensure the service is properly configured
4. Check the live metrics endpoint `/telemetry/live-metrics`
5. Verify Blazing.Mediator telemetry is enabled

### API Connectivity Issues

1. Verify the API server is running and accessible
2. Check the client configuration for correct API base URL
3. Test API endpoints directly via Swagger UI or HTTP file
4. Check streaming endpoints at `/api/streaming/health`

## Learn More

- [Blazing.Mediator Documentation](../../../README.md)
- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/languages/net/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Blazor WebAssembly Guide](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [ASP.NET Core Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

---

This example showcases the power of combining modern .NET technologies with comprehensive observability through OpenTelemetry and advanced CQRS patterns with Blazing.Mediator. The solution demonstrates production-ready telemetry integration with real-time streaming capabilities, multiple deployment options, and comprehensive testing scenarios to suit different development workflows.
