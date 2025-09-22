using Blazing.Mediator;
using Blazing.Mediator.OpenTelemetry;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetryExample.Application.Middleware;
using OpenTelemetryExample.Application.Services;
using OpenTelemetryExample.Infrastructure.Data;

namespace OpenTelemetryExample.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to configure dependency injection.
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
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });
        services.AddSwaggerServices();
        services.AddDatabaseServices();
        services.AddMediatorServices(environment);
        services.AddValidationServices();
        services.AddHealthCheckServices();
        services.AddOpenTelemetryServices(configuration, environment);
        services.AddCorsServices();

        return services;
    }

    /// <summary>
    /// Adds database services including Entity Framework Core with in-memory database.
    /// </summary>
    private static IServiceCollection AddDatabaseServices(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase("OpenTelemetryExampleDb");
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

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
            options.TagActionsBy(api => [api.GroupName ?? "Default"]);
        });

        return services;
    }

    /// <summary>
    /// Adds Blazing.Mediator services with middleware pipeline and enhanced telemetry.
    /// </summary>
    private static IServiceCollection AddMediatorServices(this IServiceCollection services, IWebHostEnvironment environment)
    {
        // Configure telemetry based on environment
        if (environment.IsDevelopment())
        {
            Console.WriteLine("[*] Configuring Mediator with FULL VISIBILITY telemetry for development");
            services.AddMediatorTelemetryWithFullVisibility();
        }
        else
        {
            Console.WriteLine("[*] Configuring Mediator with PRODUCTION telemetry");
            services.AddMediatorTelemetryForProduction();
        }

        // Alternative: Use custom streaming telemetry configuration
        // services.AddMediatorStreamingTelemetry(
        //     enablePacketLevelTelemetry: true,
        //     batchSize: environment.IsDevelopment() ? 1 : 10
        // );

        services.AddMediator(config =>
        {
            // Add tracing middleware first for full coverage
            config.AddMiddleware(typeof(TracingMiddleware<,>));
            config.AddMiddleware(typeof(TracingMiddleware<>));
            
            // Add streaming-specific middleware
            config.AddMiddleware(typeof(StreamingTracingMiddleware<,>));
            config.AddMiddleware(typeof(StreamingPerformanceMiddleware<,>));
            config.AddMiddleware(typeof(StreamingLoggingMiddleware<,>));
            
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
    /// Adds OpenTelemetry services with metrics and tracing, including enhanced Blazing.Mediator instrumentation.
    /// </summary>
    private static IServiceCollection AddOpenTelemetryServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        Console.WriteLine("[*] OpenTelemetry Configuration:");
        Console.WriteLine($"[*] OTLP Endpoint: {otlpEndpoint ?? "Not configured"}");
        Console.WriteLine($"[*] Environment: {environment.EnvironmentName}");

        // Configure telemetry batching options based on environment
        var batchingOptions = environment.IsDevelopment() 
            ? TelemetryBatchingOptions.ForDevelopment()
            : TelemetryBatchingOptions.ForProduction();

        // Allow override from configuration
        configuration.GetSection("TelemetryBatching").Bind(batchingOptions);
        
        // Validate and register the options
        var validationErrors = batchingOptions.Validate();
        if (validationErrors.Count > 0)
        {
            throw new InvalidOperationException($"Invalid telemetry batching configuration: {string.Join(", ", validationErrors)}");
        }
        
        services.AddSingleton(batchingOptions);

        Console.WriteLine("[*] Telemetry Batching Configuration:");
        Console.WriteLine($"[*]   Streaming: {batchingOptions.StreamingBatchSize} items or {batchingOptions.StreamingBatchTimeoutMs}ms");
        Console.WriteLine($"[*]   Regular: {batchingOptions.RegularBatchSize} items or {batchingOptions.RegularBatchTimeoutMs}ms");
        Console.WriteLine($"[*]   Processing Interval: {batchingOptions.ProcessingIntervalMs}ms");

        // Register custom OpenTelemetry services as singletons for proper disposal
        services.AddSingleton<OpenTelemetryActivityProcessor>();
        services.AddSingleton<OpenTelemetryMetricsReader>();

        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("Blazing.Mediator") // Core mediator metrics
                    .AddMeter("OpenTelemetryExample")
                    .AddMeter("OpenTelemetryExample.Controller")
                    .AddMeter("OpenTelemetryExample.Handler")
                    .AddMeter("OpenTelemetryExample.Mediator");

                Console.WriteLine("[*] Added Blazing.Mediator metrics instrumentation");
                Console.WriteLine("[*] Enhanced streaming metrics enabled for packet-level visibility");

                // Add our custom metrics reader to capture and store metrics
                metrics.AddReader(services.BuildServiceProvider().GetRequiredService<OpenTelemetryMetricsReader>());

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
                        options.Filter = httpContext =>
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
                    .AddSource("Blazing.Mediator") // Core mediator tracing with packet-level spans
                    .AddSource("OpenTelemetryExample")
                    .AddSource("OpenTelemetryExample.Controller")
                    .AddSource("OpenTelemetryExample.Handler")
                    .AddSource("OpenTelemetryExample.Mediator");

                Console.WriteLine("[*] Added Blazing.Mediator tracing instrumentation");
                if (environment.IsDevelopment())
                {
                    Console.WriteLine("[*] Packet-level tracing enabled for streaming operations");
                    Console.WriteLine("[*] Individual packet spans will be visible in traces");
                }

                // Add our custom activity processor to capture and store traces
                tracing.AddProcessor(services.BuildServiceProvider().GetRequiredService<OpenTelemetryActivityProcessor>());

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