# OpenTelemetry Guide for Blazing.Mediator

## Overview

This guide explains how to use the new Telemetry and Metrics features in Blazing.Mediator applications, leveraging OpenTelemetry for comprehensive monitoring, tracing, and observability. It covers setup, configuration, and best practices for integrating OpenTelemetry in .NET 9 solutions using Blazing.Mediator.

---

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
| `mediator.telemetry.health`            | Counter   | Health check counter for telemetry system     |

### Tracing (Activities)

-   **Request Processing** - Complete request/response lifecycle
-   **Middleware Execution** - Individual middleware execution spans
-   **Handler Execution** - Business logic execution spans
-   **Error Tracking** - Exception details and stack traces
-   **Validation Results** - Validation success/failure details

### Tags and Attributes

All telemetry includes relevant tags:

-   `request_name` - The request type name (sanitized)
-   `request_type` - "query" or "command"
-   `response_type` - The response type name (for queries)
-   `middleware.executed` - List of executed middleware
-   `middleware.pipeline` - Complete middleware pipeline
-   `handler.type` - The handler type name
-   `exception.type` - Exception type (sanitized)
-   `exception.message` - Exception message (sanitized)
-   `validation.passed` - Validation result
-   `performance.duration_ms` - Performance metrics

---

## Monitoring and Observability

### Console Output

The application outputs telemetry data to the console in real-time, including trace IDs, span IDs, and activity details for each operation. This enables immediate feedback during development and troubleshooting.

### Prometheus Metrics

Metrics are exposed at `/metrics` in Prometheus format, ready for scraping by Prometheus or compatible tools. Example metrics include operation durations, success/failure counts, and health checks.

### Health Checks

A dedicated health check endpoint (`/health`) reports the status of the telemetry system, including whether metrics and tracing are enabled and operational.

---

## How to Integrate OpenTelemetry in Your Application

### 1. Install Required Packages

Add the following NuGet packages to your project:

-   `OpenTelemetry`
-   `OpenTelemetry.Extensions.Hosting`
-   `OpenTelemetry.Exporter.Console`
-   `OpenTelemetry.Exporter.OpenTelemetryProtocol`
-   `OpenTelemetry.Instrumentation.AspNetCore`
-   `OpenTelemetry.Instrumentation.Http`
-   `Blazing.Mediator.OpenTelemetry` (if available)

### 2. Configure OpenTelemetry in Program.cs

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("Blazing.Mediator")
            .AddMeter("YourAppName")
            .AddConsoleExporter()
            .AddPrometheusExporter();
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("Blazing.Mediator")
            .AddSource("YourAppName")
            .AddConsoleExporter();
    });
```

### 3. Enable Mediator Telemetry

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
```

### 4. Expose Metrics and Health Endpoints

Ensure your API exposes endpoints for metrics and health checks:

-   `/metrics` for Prometheus
-   `/health` for health status
-   `/telemetry/metrics` for telemetry info
-   `/telemetry/health` for telemetry health

### 5. Monitor and Visualize

-   Use the console output for real-time feedback during development.
-   Integrate with Prometheus and Grafana for production monitoring.
-   Use the health endpoints to verify telemetry is working as expected.

---

## Best Practices

-   **Sanitize Sensitive Data:** Use built-in and custom patterns to filter sensitive information from telemetry.
-   **Use Health Checks:** Regularly check the `/health` and `/telemetry/health` endpoints.
-   **Leverage Middleware:** Add custom middleware for additional metrics or tracing as needed.
-   **Test Error Scenarios:** Simulate errors and validation failures to ensure telemetry captures all relevant data.
-   **Document Your Metrics:** Keep a reference of all custom and built-in metrics for your team.

---

## Learn More

-   [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/languages/net/)
-   [Prometheus Metrics](https://prometheus.io/docs/concepts/metric_types/)
-   [ASP.NET Core Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
-   [Blazing.Mediator Documentation](../README.md)

---

This guide provides a practical overview for developers to quickly enable, configure, and monitor OpenTelemetry-powered metrics and tracing in Blazing.Mediator-based applications. For advanced scenarios, refer to the sample project and official documentation.
