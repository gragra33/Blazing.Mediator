# OpenTelemetry Example with Blazing.Mediator

This comprehensive example demonstrates how to integrate OpenTelemetry with Blazing.Mediator in a full-stack .NET 9 application using CQRS patterns.

## ??? Architecture

The solution consists of three main components:

1. **OpenTelemetryExample** - ASP.NET Core Web API with OpenTelemetry integration
2. **OpenTelemetryExample.Client** - Blazor WebAssembly client application
3. **OpenTelemetryExample.Aspire** - .NET Aspire orchestration project

## ?? Features Demonstrated

### OpenTelemetry Integration
- ? **Metrics Collection** - Request duration, success rates, custom business metrics
- ? **Distributed Tracing** - End-to-end request tracing through CQRS pipeline
- ? **Structured Logging** - Correlated logs with trace context
- ? **Health Checks** - System and telemetry health monitoring
- ? **Data Sanitization** - Automatic PII filtering in telemetry

### CQRS with Blazing.Mediator
- ? **Commands & Queries** - Complete CRUD operations
- ? **Middleware Pipeline** - Validation, logging, and telemetry middleware
- ? **Notifications** - Event-driven architecture examples
- ? **Error Handling** - Comprehensive exception management

### Modern .NET Stack
- ? **.NET 9** - Latest framework features
- ? **Blazor WebAssembly** - Modern client-side SPA
- ? **Bootstrap 5** - Responsive UI framework
- ? **.NET Aspire** - Cloud-native orchestration

## ????? Running the Application

### Option 1: Using .NET Aspire (Recommended)

```bash
# Navigate to the OpenTelemetry example directory
cd src/samples/OpenTelemetry

# Run the Aspire orchestrator
dotnet run --project OpenTelemetryExample.Aspire
```

This will start both the API and client applications with proper service discovery and telemetry collection.

### Option 2: Manual Startup

#### Start the API Server
```bash
cd src/samples/OpenTelemetry/OpenTelemetryExample
dotnet run
```
The API will be available at:
- HTTPS: `https://localhost:7000`
- HTTP: `http://localhost:5000`

#### Start the Blazor Client
```bash
cd src/samples/OpenTelemetry/OpenTelemetryExample.Client
dotnet run
```
The client will be available at:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5001`

### Option 3: Using Visual Studio Code

If you're using VS Code, you can use the provided launch configurations:

1. Open the workspace in VS Code
2. Go to the Run and Debug view (Ctrl+Shift+D)
3. Select "Run OpenTelemetry Example" from the dropdown
4. Click the play button to start both applications

## ?? Exploring the Application

### Web Client Features

Navigate to the Blazor client application and explore:

1. **Home Page** - Overview of OpenTelemetry features
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

### HTTP Testing

Use the provided `OpenTelemetryExample.http` file for comprehensive API testing:

- Health check endpoints
- User CRUD operations
- Error simulation endpoints
- Load testing scenarios
- Telemetry-specific endpoints

## ?? OpenTelemetry Observability

### Metrics Collected

The application automatically collects:

- **HTTP Request Metrics**
  - Request duration histograms
  - Request count counters
  - Success/failure rates
  - Status code distributions

- **CQRS Metrics**
  - Command execution times
  - Query response times
  - Middleware pipeline durations
  - Validation failure counts

- **Business Metrics**
  - User creation rates
  - Error frequencies
  - Feature usage patterns

### Distributed Tracing

Each request generates comprehensive traces showing:

- HTTP request/response spans
- CQRS command/query execution
- Middleware pipeline stages
- Database operations (simulated)
- Cross-service correlations

### Logging Integration

All logs include:

- Trace and span IDs for correlation
- Structured data with consistent formats
- Automatic PII sanitization
- Configurable log levels per component

## ??? Configuration

### OpenTelemetry Settings

Key configuration options in `appsettings.json`:

```json
{
  "OpenTelemetry": {
    "ServiceName": "OpenTelemetryExample",
    "ServiceVersion": "1.0.0",
    "EnableConsoleExporter": true,
    "EnableOtlpExporter": false,
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

### Health Check Endpoints

- `/health` - Overall application health
- `/telemetry/health` - OpenTelemetry system health
- `/telemetry/metrics` - Current metrics information

### Environment Variables

Set these for advanced scenarios:

- `OTEL_EXPORTER_OTLP_ENDPOINT` - OTLP collector endpoint
- `OTEL_SERVICE_NAME` - Override service name
- `OTEL_RESOURCE_ATTRIBUTES` - Additional resource attributes

## ?? Testing Scenarios

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

## ?? Code Structure

### API Project (`OpenTelemetryExample`)

```
Application/
??? Commands/          # CQRS commands
??? Queries/           # CQRS queries
??? Notifications/     # Event notifications
??? Middleware/        # Pipeline middleware

Controllers/           # API endpoints
Models/               # Data models
```

### Client Project (`OpenTelemetryExample.Client`)

```
Pages/                # Blazor pages
??? Home.razor        # Feature overview
??? Users.razor       # CRUD interface
??? Telemetry.razor   # Monitoring dashboard
??? Demo.razor        # Interactive testing

Services/             # HTTP client services
Models/               # Client-side models
Layout/               # Application layout
Shared/               # Reusable components
```

### Aspire Project (`OpenTelemetryExample.Aspire`)

- Service orchestration configuration
- Development environment setup
- Service discovery and configuration

## ?? Troubleshooting

### Common Issues

1. **Port Conflicts**: Ensure ports 5000-5001 and 7000-7001 are available
2. **HTTPS Certificates**: Run `dotnet dev-certs https --trust` if needed
3. **Build Errors**: Clean and rebuild with `dotnet clean && dotnet build`

### Telemetry Not Appearing

1. Check the console output for telemetry exports
2. Verify OpenTelemetry health at `/telemetry/health`
3. Ensure the service is properly configured in `appsettings.json`

## ?? Extension Points

This example can be extended with:

- **External Collectors**: Jaeger, Prometheus, Application Insights
- **Custom Metrics**: Business-specific measurements
- **Additional Middleware**: Security, caching, rate limiting
- **Database Integration**: Entity Framework with telemetry
- **Message Queues**: Service Bus, RabbitMQ integration

## ?? Learn More

- [Blazing.Mediator Documentation](../../../README.md)
- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/languages/net/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Blazor WebAssembly Guide](https://learn.microsoft.com/en-us/aspnet/core/blazor/)

---

This example showcases the power of combining modern .NET technologies with comprehensive observability through OpenTelemetry and CQRS patterns with Blazing.Mediator.