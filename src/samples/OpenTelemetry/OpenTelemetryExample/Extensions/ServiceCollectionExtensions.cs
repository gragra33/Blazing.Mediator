using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Blazing.Mediator;
using Blazing.Mediator.OpenTelemetry;
using OpenTelemetryExample.Application.Middleware;
using FluentValidation;

namespace OpenTelemetryExample.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to configure dependency injection.
/// Follows single responsibility principle by separating service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all application services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IWebHostEnvironment environment)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerServices();
        services.AddMediatorServices();
        services.AddValidationServices();
        services.AddHealthCheckServices();
        services.AddOpenTelemetryServices(configuration, environment);
        services.AddCorsServices();

        return services;
    }

    /// <summary>
    /// Adds Swagger/OpenAPI services.
    /// </summary>
    private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "OpenTelemetry Example API",
                Version = "v1",
                Description = "An example API demonstrating OpenTelemetry integration with Blazing.Mediator",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "OpenTelemetry Example",
                    Url = new Uri("https://github.com/example/opentelemetry-example")
                }
            });

            // Include XML documentation if available
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Configure tags for better organization
            options.TagActionsBy(api => new[] { api.GroupName ?? "Default" });
        });

        return services;
    }

    /// <summary>
    /// Adds Blazing.Mediator services with middleware pipeline.
    /// </summary>
    private static IServiceCollection AddMediatorServices(this IServiceCollection services)
    {
        services.AddMediatorTelemetry();
        services.AddMediator(config =>
        {
            // Add middleware pipeline in order of execution
            config.AddMiddleware(typeof(ErrorHandlingMiddleware<,>));
            config.AddMiddleware(typeof(ErrorHandlingMiddleware<>));
            config.AddMiddleware(typeof(ValidationMiddleware<,>));
            config.AddMiddleware(typeof(ValidationMiddleware<>));
            config.AddMiddleware(typeof(LoggingMiddleware<,>));
            config.AddMiddleware(typeof(LoggingMiddleware<>));
            config.AddMiddleware(typeof(PerformanceMiddleware<,>));
            config.AddMiddleware(typeof(PerformanceMiddleware<>));
        }, typeof(Program).Assembly);

        return services;
    }

    /// <summary>
    /// Adds FluentValidation services.
    /// </summary>
    private static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        return services;
    }

    /// <summary>
    /// Adds health check services.
    /// </summary>
    private static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<OpenTelemetryExample.Application.HealthChecks.MediatorTelemetryHealthCheck>("mediator_telemetry");

        return services;
    }

    /// <summary>
    /// Adds OpenTelemetry services with metrics and tracing.
    /// </summary>
    private static IServiceCollection AddOpenTelemetryServices(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IWebHostEnvironment environment)
    {
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        
        Console.WriteLine($"[*] OpenTelemetry Configuration:");
        Console.WriteLine($"[*] OTLP Endpoint: {otlpEndpoint ?? "Not configured"}");

        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("Blazing.Mediator")
                    .AddMeter("OpenTelemetryExample");

                // Only add console exporter in development and if no OTLP endpoint
                if (environment.IsDevelopment() && string.IsNullOrEmpty(otlpEndpoint))
                {
                    Console.WriteLine("[*] Adding Console Metrics Exporter for development");
                    metrics.AddConsoleExporter();
                }

                // Add OTLP exporter if endpoint is configured (for Aspire)
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    Console.WriteLine($"[*] OTLP Metrics Exporter configured with endpoint: {otlpEndpoint}");
                    metrics.AddOtlpExporter();
                }
                else
                {
                    Console.WriteLine("[!] OTLP Metrics Exporter not configured - OTEL_EXPORTER_OTLP_ENDPOINT not set");
                }
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Exclude telemetry endpoints to prevent feedback loops
                        options.Filter = (httpContext) =>
                        {
                            var path = httpContext.Request.Path.ToString();
                            return !path.StartsWith("/telemetry") && 
                                   !path.StartsWith("/debug") &&
                                   !path.Contains("otlp");
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        // Exclude OTLP requests to prevent feedback loops
                        options.FilterHttpRequestMessage = (request) =>
                        {
                            var uri = request.RequestUri?.ToString() ?? "";
                            return !uri.Contains("otlp") && !uri.Contains("21270");
                        };
                    })
                    .AddSource("Blazing.Mediator")
                    .AddSource("OpenTelemetryExample");

                // Only add console exporter in development and if no OTLP endpoint
                if (environment.IsDevelopment() && string.IsNullOrEmpty(otlpEndpoint))
                {
                    Console.WriteLine("[*] Adding Console Tracing Exporter for development");
                    tracing.AddConsoleExporter();
                }

                // Add OTLP exporter if endpoint is configured (for Aspire)
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    Console.WriteLine($"[*] OTLP Tracing Exporter configured with endpoint: {otlpEndpoint}");
                    tracing.AddOtlpExporter();
                }
                else
                {
                    Console.WriteLine("[!] OTLP Tracing Exporter not configured - OTEL_EXPORTER_OTLP_ENDPOINT not set");
                }
            });

        return services;
    }

    /// <summary>
    /// Adds CORS services for Blazor client.
    /// </summary>
    private static IServiceCollection AddCorsServices(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }
}