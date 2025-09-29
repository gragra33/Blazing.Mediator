# Blazing.Mediator - Configuration Guide

## Table of Contents

1. [Introduction](#introduction)
2. [Pre-Configured Instances](#pre-configured-instances)
3. [Configuration Sources](#configuration-sources)
4. [Quick Reference Tables](#quick-reference-tables)
5. [Environment-Aware Configuration](#environment-aware-configuration)
6. [Configuration Validation](#configuration-validation)
7. [Configuration Diagnostics](#configuration-diagnostics)
8. [Real-World Usage Examples](#real-world-usage-examples)
9. [Configuration Examples](#configuration-examples)
10. [Configuration Methods Comparison](#configuration-methods-comparison)
11. [Performance Impact](#performance-impact)
12. [Best Practices](#best-practices)

## Introduction

Blazing.Mediator provides a comprehensive configuration system that enables fine-tuned control over all aspects of the mediator framework. The configuration system supports both simple scenarios with zero configuration and complex enterprise environments requiring sophisticated settings. This guide covers all configuration approaches, from basic setup to advanced environment-aware configurations with full diagnostic capabilities.

The configuration system is built around a fluent API that provides discoverability, type safety, and powerful composition capabilities. Whether you're building a simple console application or a complex microservices architecture, the configuration system adapts to your needs while maintaining performance and providing comprehensive observability.

### Key Configuration Features

#### ✨ **Smart Configuration Binding**
The configuration system provides intelligent binding that:
- Only applies explicitly configured JSON values
- Preserves preset defaults for non-configured options
- Uses `IConfigurationSection` inspection to detect explicit configuration

#### ⚡ **Environment-Aware Automation**
```csharp
// One line handles everything
config.WithEnvironmentConfiguration(configuration, environment)
```
- **Development** → Full features + debugging capabilities
- **Production** → Optimized performance + essential monitoring  
- **Testing** → Minimal overhead + fast execution
- **Custom** → Intelligent fallback to minimal preset

#### 🚀 **Pre-Configured Instances**
```csharp
// Direct static factory method support
services.AddMediator(MediatorConfiguration.Production(typeof(Program).Assembly));
```
- **Instant Setup** → One-line configuration for common scenarios
- **Battle-Tested Presets** → Production-ready settings out of the box
- **Customizable** → Start with presets, then customize as needed

#### 🔍 **Advanced Diagnostics**
```csharp
var diagnostics = config.GetDiagnostics(environment);
// Provides: Validation status, feature availability, warnings, environment suitability
```

#### ✅ **Production Safety**
Automatic validation prevents common deployment issues:
- Blocks verbose logging in production
- Prevents packet-level telemetry overhead
- Validates retention periods for scalability
- Ensures appropriate resource usage

## Pre-Configured Instances

Blazing.Mediator supports pre-configured `MediatorConfiguration` instances directly with the `AddMediator` method. This feature provides instant setup with battle-tested configurations for common scenarios while maintaining full customization capabilities.

### Direct Static Factory Method Support

Instead of using configuration actions, you can pass pre-configured instances directly:

```csharp
// 🚀 Direct pre-configured instance support
builder.Services.AddMediator(MediatorConfiguration.Production(typeof(Program).Assembly));

// 🎯 All static factory methods work
builder.Services.AddMediator(MediatorConfiguration.Development(typeof(Program).Assembly));
builder.Services.AddMediator(MediatorConfiguration.Minimal(typeof(Program).Assembly));
builder.Services.AddMediator(MediatorConfiguration.Disabled(typeof(Program).Assembly));
builder.Services.AddMediator(MediatorConfiguration.NotificationOptimized(typeof(Program).Assembly));
builder.Services.AddMediator(MediatorConfiguration.StreamingOptimized(typeof(Program).Assembly));
```

### Available Pre-Configured Presets

#### Production Configuration
Optimized for production deployments with essential monitoring and performance focus:

```csharp
// Production-ready configuration with optimized performance
builder.Services.AddMediator(MediatorConfiguration.Production(typeof(Program).Assembly));
```

**What's Included:**
- ✅ Essential statistics tracking (`StatisticsOptions.Production()`)
- ✅ Production-optimized telemetry (`TelemetryOptions.Production()`)
- ✅ Minimal logging for performance (`LoggingOptions.CreateMinimal()`)
- ✅ All discovery features enabled
- ✅ Optimized for scalability and performance

#### Development Configuration
Comprehensive features for development and debugging scenarios:

```csharp
// Full-featured development configuration
builder.Services.AddMediator(MediatorConfiguration.Development(typeof(Program).Assembly));
```

**What's Included:**
- ✅ Comprehensive statistics tracking (`StatisticsOptions.Development()`)
- ✅ Verbose telemetry with debugging info (`TelemetryOptions.Development()`)
- ✅ Detailed logging for debugging (`LoggingOptions.CreateVerbose()`)
- ✅ All discovery features enabled
- ✅ Maximum observability and debugging capabilities

#### Minimal Configuration
Basic features only for performance-critical scenarios:

```csharp
// Minimal overhead configuration
builder.Services.AddMediator(MediatorConfiguration.Minimal(typeof(Program).Assembly));
```

**What's Included:**
- ✅ Disabled statistics tracking (`StatisticsOptions.Disabled()`)
- ✅ Minimal telemetry (`TelemetryOptions.Minimal()`)
- ✅ No logging (disabled)
- ✅ Constrained middleware discovery disabled
- ✅ Basic notification handler discovery enabled

#### Disabled Configuration
Maximum performance with all optional features disabled:

```csharp
// High-performance configuration with features disabled
builder.Services.AddMediator(MediatorConfiguration.Disabled(typeof(Program).Assembly));
```

**What's Included:**
- ✅ No statistics tracking
- ✅ Disabled telemetry configuration (`TelemetryOptions.Disabled()`)
- ✅ No logging
- ✅ Constrained middleware discovery disabled
- ✅ Notification handler discovery disabled

#### Notification-Optimized Configuration
Specialized for event-driven and notification-heavy applications:

```csharp
// Optimized for notification-centric applications
builder.Services.AddMediator(MediatorConfiguration.NotificationOptimized(typeof(Program).Assembly));
```

**What's Included:**
- ✅ Notification-focused statistics (request metrics disabled)
- ✅ Notification-only telemetry (`TelemetryOptions.NotificationOnly()`)
- ✅ Notification-focused logging (send operations disabled)
- ✅ Full notification middleware and handler discovery
- ✅ Request middleware discovery disabled

#### Streaming-Optimized Configuration
Specialized for real-time data processing and streaming scenarios:

```csharp
// Optimized for streaming applications
builder.Services.AddMediator(MediatorConfiguration.StreamingOptimized(typeof(Program).Assembly));
```

**What's Included:**
- ✅ Request-focused statistics (notification metrics disabled)
- ✅ Streaming-only telemetry (`TelemetryOptions.StreamingOnly()`)
- ✅ Streaming-focused logging (publish operations disabled)
- ✅ Request middleware discovery enabled
- ✅ Notification features disabled for performance

### Custom Pre-Configured Instances

You can create your own custom pre-configured instances for reuse across multiple applications:

```csharp
// Create a custom configuration instance
var customConfig = new MediatorConfiguration()
    .WithStatisticsTracking(options =>
    {
        options.EnableRequestMetrics = true;
        options.EnableNotificationMetrics = true;
        options.EnablePerformanceCounters = true;
        options.EnableDetailedAnalysis = true;
        options.MetricsRetentionPeriod = TimeSpan.FromHours(6);
    })
    .WithTelemetry(options =>
    {
        options.Enabled = true;
        options.CaptureExceptionDetails = true;
        options.PacketLevelTelemetryEnabled = false;
    })
    .WithLogging(options =>
    {
        options.EnableSend = true;
        options.EnableDetailedHandlerInfo = false;
    })
    .WithMiddlewareDiscovery()
    .AddAssembly(typeof(Program).Assembly);

// Use the custom configuration
builder.Services.AddMediator(customConfig);
```

### Creating Reusable Configuration Presets

For enterprise scenarios, you can create reusable configuration factories:

```csharp
public static class EnterpriseConfigurations
{
    public static MediatorConfiguration CreateMicroserviceConfiguration(params Assembly[] assemblies)
    {
        return new MediatorConfiguration()
            .WithStatisticsTracking(options =>
            {
                options.EnableRequestMetrics = true;
                options.EnableNotificationMetrics = true;
                options.EnableMiddlewareMetrics = true;
                options.EnablePerformanceCounters = true;
                options.EnableDetailedAnalysis = true;
                options.MetricsRetentionPeriod = TimeSpan.FromDays(7);
                options.CleanupInterval = TimeSpan.FromHours(4);
            })
            .WithTelemetry(options =>
            {
                options.Enabled = true;
                options.CaptureExceptionDetails = true;
                options.CaptureHandlerDetails = true;
                options.CreateHandlerChildSpans = true;
            })
            .WithLogging(options =>
            {
                options.EnableSend = true;
                options.EnablePublish = true;
                options.EnablePerformanceTiming = true;
            })
            .WithMiddlewareDiscovery()
            .WithNotificationMiddlewareDiscovery()
            .WithConstrainedMiddlewareDiscovery()
            .WithNotificationHandlerDiscovery()
            .AddAssemblies(assemblies);
    }
    
    public static MediatorConfiguration CreateHighPerformanceConfiguration(params Assembly[] assemblies)
    {
        return new MediatorConfiguration()
            .WithStatisticsTracking(StatisticsOptions.Disabled())
            .WithTelemetry(TelemetryOptions.Minimal())
            .WithoutLogging()
            .WithoutMiddlewareDiscovery()  // Manual middleware registration only
            .WithoutNotificationMiddlewareDiscovery()
            .WithoutConstrainedMiddlewareDiscovery()
            .WithNotificationHandlerDiscovery()  // Keep handlers for functionality
            .AddAssemblies(assemblies);
    }
}

// Usage in different microservices
builder.Services.AddMediator(EnterpriseConfigurations.CreateMicroserviceConfiguration(
    typeof(Program).Assembly,
    typeof(OrderHandler).Assembly,
    typeof(PaymentHandler).Assembly
));
```

### Comparison with Configuration Actions

The pre-configured instance support provides an alternative to configuration actions:

```csharp
// Before: Using configuration actions
builder.Services.AddMediator(config =>
{
    config.WithProductionPreset()
          .AddAssembly(typeof(Program).Assembly);
});

// After: Using pre-configured instance (equivalent)
builder.Services.AddMediator(MediatorConfiguration.Production(typeof(Program).Assembly));
```

### When to Use Pre-Configured Instances

#### ✅ **Use Pre-Configured Instances When:**
- You want quick setup with proven configurations
- You're starting a new project and want best practices
- You need consistent configuration across multiple services
- You want to minimize configuration complexity
- You trust the library's optimization choices

#### ⚠️ **Use Configuration Actions When:**
- You need fine-grained control over every setting
- You have specific performance requirements that differ from presets
- You're integrating with existing configuration systems
- You need dynamic configuration based on runtime conditions
- You want to gradually migrate from another mediator library

This pre-configured instance support significantly simplifies common configuration scenarios while maintaining all the flexibility and power of the existing configuration system.

## Configuration Sources

Blazing.Mediator supports multiple configuration sources that can be used individually or combined to provide maximum flexibility for different deployment scenarios and application architectures.

### Configuration Source Options

| **Source Type** | **Method** | **Use Case** | **Example** |
|-----------------|------------|--------------|-------------|
| **Fluent Configuration** | Code-based fluent API | Type-safe, compile-time configuration | `config.WithStatisticsTracking()` |
| **JSON Configuration** | appsettings.json files | Environment-specific settings | `"Blazing:Mediator:Statistics:EnableDetailedAnalysis"` |
| **Environment Variables** | System environment variables | Container and cloud deployments | `BLAZING__MEDIATOR__STATISTICS__ENABLEDETAILEDANALISISTRUE` |
| **Command Line Arguments** | Application startup parameters | Development and deployment scripts | `--Blazing:Mediator:Statistics:EnableDetailedAnalysis=true` |
| **Azure App Configuration** | Cloud configuration service | Centralized configuration management | Remote configuration with feature flags |
| **Database Configuration** | Custom configuration providers | Dynamic runtime configuration | Stored procedures or tables for settings |
| **Key Vault Integration** | Azure Key Vault or similar | Secure configuration storage | Connection strings and sensitive settings |

### Configuration Precedence

When multiple configuration sources are used, the .NET configuration system follows a specific precedence order (last wins):

1. **Code Configuration** (fluent API) - Base settings
2. **appsettings.json** - Default application settings
3. **appsettings.{Environment}.json** - Environment-specific overrides
4. **Environment Variables** - Deployment environment settings
5. **Command Line Arguments** - Runtime overrides
6. **Azure App Configuration** - Cloud-based settings
7. **User Secrets** - Development-time sensitive data

### Environment Variables

Environment variables provide excellent support for containerized deployments and cloud environments where configuration needs to be externalized.

```bash
# Basic feature toggles
BLAZING__MEDIATOR__STATISTICS__ENABLED=true
BLAZING__MEDIATOR__TELEMETRY__ENABLED=true
BLAZING__MEDIATOR__LOGGING__ENABLED=false

# Advanced settings
BLAZING__MEDIATOR__STATISTICS__METRICSRETENTIONPERIOD=24:00:00
BLAZING__MEDIATOR__TELEMETRY__PACKETLEVELTELEMETRYENABLED=false
BLAZING__MEDIATOR__LOGGING__ENABLEDETAILEDHANDLERINFO=false

# Discovery settings
BLAZING__MEDIATOR__DISCOVERY__DISCOVERMIDDLEWARE=true
BLAZING__MEDIATOR__DISCOVERY__DISCOVERNOTIFICATIONHANDLERS=true
```

### Command Line Arguments

Command line arguments are ideal for deployment scripts, CI/CD pipelines, and development scenarios where settings need to be overridden at runtime.

```bash
# Start application with specific configuration
dotnet run --Blazing:Mediator:Statistics:Enabled=true \
          --Blazing:Mediator:Telemetry:PacketLevelTelemetryEnabled=false \
          --Blazing:Mediator:Logging:EnableDetailedHandlerInfo=true

# Docker container with environment-specific overrides
docker run myapp --Blazing:Mediator:Statistics:MetricsRetentionPeriod=12:00:00
```

### Custom Configuration Providers

You can implement custom configuration providers for scenarios requiring database-driven configuration, remote configuration services, or specialized configuration sources.

```csharp
// Custom configuration provider example
public class DatabaseConfigurationProvider : ConfigurationProvider
{
    private readonly string _connectionString;

    public DatabaseConfigurationProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public override void Load()
    {
        // Load configuration from database
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        var command = new SqlCommand("SELECT ConfigKey, ConfigValue FROM MediatorConfiguration", connection);
        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            var key = reader["ConfigKey"].ToString();
            var value = reader["ConfigValue"].ToString();
            Data[key] = value;
        }
    }
}

// Register custom provider
builder.Configuration.Add(new DatabaseConfigurationSource(_connectionString));
```

## Quick Reference Tables

### MediatorConfiguration Fluent Methods

The `MediatorConfiguration` class provides fluent methods for configuring various aspects of the mediator, including middleware discovery, statistics tracking, telemetry, logging, notification handling, and assembly registration. These methods allow you to customize the mediator's behavior to match your application's specific requirements and performance needs.

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

The `MediatorConfiguration` class provides several static factory methods for creating pre-configured instances optimized for different environments and scenarios.

| Method | Purpose | Configuration | Best For |
|--------|---------|---------------|----------|
| `Development(params Assembly[])` | Development environment configuration | Comprehensive features with detailed debugging information | Development and debugging scenarios |
| `Production(params Assembly[])` | Production environment configuration | Essential features with optimized performance settings | Production deployments |
| `Disabled(params Assembly[])` | Minimal configuration with features disabled | All optional features disabled for maximum performance | High-performance scenarios where only basic mediator functionality is needed |
| `Minimal(params Assembly[])` | Minimal configuration with basic features | Basic features only with minimal overhead | Performance-critical applications with minimal overhead |
| `NotificationOptimized(params Assembly[])` | Notification-focused configuration | Optimized for notification-centric applications | Event-driven architectures and notification-heavy applications |
| `StreamingOptimized(params Assembly[])` | Streaming-focused configuration | Optimized for streaming applications | Real-time data processing and streaming scenarios |

### Utility Methods

| Method | Purpose | Configuration Impact |
|--------|---------|---------------------|
| `Validate()` | Validate current configuration | Returns list of validation error messages |
| `ValidateAndThrow()` | Validate configuration and throw if invalid | Throws ArgumentException for invalid configuration |
| `Clone()` | Create copy of current configuration | Returns new configuration instance with same values |

## Environment-Aware Configuration

The environment-aware configuration system automatically detects your deployment environment and applies the most suitable settings. This powerful feature eliminates the need for manual environment-specific configuration while ensuring optimal performance and appropriate feature sets for each deployment scenario.

### Automatic Environment Detection

The `WithEnvironmentConfiguration` method automatically detects your environment and applies the most suitable preset:

```csharp
builder.Services.AddMediator(config =>
{
    config.WithEnvironmentConfiguration(builder.Configuration, builder.Environment)
          .AddAssembly(typeof(Program).Assembly);
});
```

**Environment Mapping:**
- **Development** → `WithDevelopmentPreset()` (Full features, verbose logging)
- **Production** → `WithProductionPreset()` (Optimized performance, minimal logging)
- **Testing** → `WithDisabledPreset()` (Minimal overhead for testing)
- **Other** → `WithMinimalPreset()` (Safe defaults)

### Environment-Specific Configuration Files

Combine presets with environment-specific JSON configuration to provide both automatic optimization and fine-tuned control:

#### appsettings.Development.json
```json
{
  "Blazing": {
    "Mediator": {
      "Statistics": {
        "EnableDetailedAnalysis": true,
        "MetricsRetentionPeriod": "02:00:00"
      },
      "Telemetry": {
        "PacketLevelTelemetryEnabled": true,
        "CreateHandlerChildSpans": true
      },
      "Logging": {
        "EnableDetailedHandlerInfo": true,
        "EnableConstraintLogging": true
      }
    }
  }
}
```

#### appsettings.Production.json
```json
{
  "Blazing": {
    "Mediator": {
      "Statistics": {
        "MetricsRetentionPeriod": "24:00:00",
        "EnableDetailedAnalysis": false
      },
      "Telemetry": {
        "PacketLevelTelemetryEnabled": false,
        "CreateHandlerChildSpans": false
      },
      "Logging": {
        "EnableDetailedHandlerInfo": false,
        "EnableConstraintLogging": false
      }
    }
  }
}
```

### Mixed Configuration Strategies

Choose the configuration strategy that best fits your deployment requirements and organizational standards:

#### Strategy 1: Preset First, Then JSON Overrides
```csharp
services.AddMediator(config =>
{
    config.WithDevelopmentPreset()                    // Apply preset first
          .WithConfiguration(builder.Configuration)   // Then JSON overrides
          .AddAssembly(typeof(Program).Assembly);
});
```

#### Strategy 2: Environment-Aware with JSON
```csharp
services.AddMediator(config =>
{
    config.WithEnvironmentConfiguration(builder.Configuration, builder.Environment)
          .AddAssembly(typeof(Program).Assembly);
});
```

#### Strategy 3: Custom Section Path
```csharp
services.AddMediator(config =>
{
    config.WithEnvironmentConfiguration(
            builder.Configuration, 
            builder.Environment, 
            "MyApp:MediatorSettings")
          .AddAssembly(typeof(Program).Assembly);
});
```

## Configuration Validation

The validation system provides comprehensive environment-specific validation to prevent common deployment issues and ensure optimal configuration for different deployment scenarios.

### Environment-Specific Validation

#### Basic Environment Validation

```csharp
services.AddMediator(config =>
{
    config.WithEnvironmentConfiguration(builder.Configuration, builder.Environment)
          .ValidateForEnvironment(builder.Environment) // Validates against built-in environment rules
          .AddAssembly(typeof(Program).Assembly);
});
```

#### Built-in Validation Rules

**Production Environment:**
- 🚫 No verbose logging features (performance impact)
- 🚫 No packet-level telemetry (overhead)
- ⏳ Minimum 1-hour metrics retention period
- 📊 Essential monitoring features enabled

**Development Environment:**
- ✅ Some form of logging must be enabled (debugging)

### Custom Fluent Validation

The `ValidateForEnvironment` method supports custom validation rules through a comprehensive fluent interface:

```csharp
services.AddMediator(config =>
{
    config.WithEnvironmentConfiguration(builder.Configuration, builder.Environment)
          .ValidateForEnvironment(builder.Environment, validation =>
          {
              // Environment-specific rules
              validation
                  .ForProduction(cfg => cfg.StatisticsOptions?.EnableDetailedAnalysis != true,
                      "Detailed analysis should be disabled in production")
                  .ForDevelopment(cfg => cfg.LoggingOptions != null,
                      "Development must have logging enabled");

              // Statistics validation
              validation.Statistics(stats =>
              {
                  stats.MustBeEnabled("Statistics required for monitoring");
                  stats.RetentionPeriodAtLeast(TimeSpan.FromHours(6), "Extended retention required");
                  stats.Rule(options => options?.CleanupInterval <= options?.MetricsRetentionPeriod / 4,
                      "Cleanup should run frequently enough");
              });

              // Telemetry validation
              validation.Telemetry(telemetry =>
              {
                  telemetry.MustBeEnabled("Telemetry required for observability");
                  telemetry.PacketLevelMustBeDisabled("Performance optimization");
                  telemetry.Rule(options => options?.Enabled == true && options?.CaptureExceptionDetails == true,
                      "Exception details must be captured when telemetry is enabled");
              });

              // Logging validation
              validation.Logging(logging =>
              {
                  logging.MustBeEnabled("Logging required for diagnostics");
                  logging.DetailedFeaturesMustBeDisabled("Performance optimization");
                  logging.Rule(options => options?.EnableWarnings == true,
                      "Warnings must be enabled for issue detection");
              });

              // Custom rules with full context
              validation.Rule((cfg, env) => 
                  env.IsProduction() ? cfg.Assemblies.Count > 0 : true,
                  "Production environments must have assemblies configured");
          })
          .AddAssembly(typeof(Program).Assembly);
});
```

### Available Validation Builders

The validation system provides a comprehensive set of fluent builders that enable precise control over configuration validation rules. These builders are organized by functional area and support both simple validation scenarios and complex enterprise requirements. Each builder category offers specialized methods for common validation patterns while maintaining the flexibility to create custom rules tailored to your specific deployment needs.

#### Environment-Specific Rules
- `ForProduction(predicate, errorMessage)` - Rules that only apply in production
- `ForDevelopment(predicate, errorMessage)` - Rules that only apply in development  
- `ForStaging(predicate, errorMessage)` - Rules that only apply in staging
- `ForEnvironment(environmentName, predicate, errorMessage)` - Rules for specific environments

#### Statistics Validation
- `MustBeEnabled(errorMessage?)` - Requires statistics to be enabled
- `MustBeDisabled(errorMessage?)` - Requires statistics to be disabled
- `RetentionPeriodAtLeast(timeSpan, errorMessage?)` - Minimum retention period
- `Rule(predicate, errorMessage)` - Custom statistics validation rules

#### Telemetry Validation  
- `MustBeEnabled(errorMessage?)` - Requires telemetry to be enabled
- `MustBeDisabled(errorMessage?)` - Requires telemetry to be disabled
- `PacketLevelMustBeDisabled(errorMessage?)` - Requires packet telemetry to be disabled
- `Rule(predicate, errorMessage)` - Custom telemetry validation rules

#### Logging Validation
- `MustBeEnabled(errorMessage?)` - Requires logging to be enabled
- `DetailedFeaturesMustBeDisabled(errorMessage?)` - Requires detailed features to be disabled
- `Rule(predicate, errorMessage)` - Custom logging validation rules

#### General Rules
- `Rule(condition, errorMessage)` - Simple boolean condition
- `Rule(predicate, errorMessage)` - Configuration-based predicate
- `Rule(predicate, errorMessage)` - Configuration and environment predicate

### Real-World Validation Examples

These examples demonstrate practical validation scenarios that address common deployment challenges and organizational requirements. The validation patterns shown here represent battle-tested approaches used in production environments, from high-performance systems that prioritize minimal overhead to enterprise applications requiring comprehensive compliance and monitoring capabilities.

**Enterprise Production Validation:**
```csharp
.ValidateForEnvironment(validation =>
{
    validation
        .ForProduction(cfg => cfg.StatisticsOptions?.MetricsRetentionPeriod >= TimeSpan.FromDays(7),
            "Production requires 7-day metrics retention for compliance")
        .ForProduction(cfg => cfg.TelemetryOptions?.Enabled == true,
            "Production must have telemetry for observability")
        .Statistics(stats => stats.Rule(options => 
            options?.EnableDetailedAnalysis != true || options?.MetricsRetentionPeriod >= TimeSpan.FromDays(30),
            "Detailed analysis requires 30-day retention for meaningful insights"))
        .Telemetry(telemetry => telemetry.Rule(options =>
            options?.Enabled != true || options?.CaptureExceptionDetails == true,
            "Exception capture required when telemetry is enabled"))
        .Rule((cfg, env) => cfg.Assemblies.Count >= 2,
            "Enterprise applications must scan multiple assemblies");
})
```

**High-Performance Validation:**
```csharp
.ValidateForEnvironment(validation =>
{
    validation
        .Statistics(stats => stats.Rule(options =>
            options?.EnablePerformanceCounters != true,
            "Performance counters disabled for maximum throughput"))
        .Telemetry(telemetry => telemetry.PacketLevelMustBeDisabled(
            "Packet telemetry adds overhead in high-performance scenarios"))
        .Logging(logging => logging.DetailedFeaturesMustBeDisabled(
            "Detailed logging impacts performance"))
        .Rule(cfg => cfg.StatisticsOptions?.CleanupInterval <= TimeSpan.FromMinutes(5),
            "Frequent cleanup required for memory management");
})
```

## Configuration Diagnostics

The diagnostics system provides comprehensive insight into configuration state, validation results, and feature availability for troubleshooting and monitoring purposes.

### Basic Diagnostics

```csharp
var diagnostics = config.GetDiagnostics(environment);

Console.WriteLine($"Environment: {diagnostics.Environment}");
Console.WriteLine($"Valid: {diagnostics.IsValid}");
Console.WriteLine($"Statistics: {(diagnostics.StatisticsEnabled ? "Enabled" : "Disabled")}");
Console.WriteLine($"Telemetry: {(diagnostics.TelemetryEnabled ? "Enabled" : "Disabled")}");
```

### Detailed Diagnostics Report

```csharp
var diagnostics = config.GetDiagnostics(environment);

if (!diagnostics.IsValid)
{
    Console.WriteLine("❌ Configuration Errors:");
    foreach (var error in diagnostics.ValidationErrors)
    {
        Console.WriteLine($"  • {error}");
    }
}

if (diagnostics.LoggingWarnings.Any())
{
    Console.WriteLine("⚠️ Logging Warnings:");
    foreach (var warning in diagnostics.LoggingWarnings)
    {
        Console.WriteLine($"  • {warning}");
    }
}

Console.WriteLine($"ℹ️ Discovery Settings:");
Console.WriteLine($"  Middleware: {diagnostics.DiscoverySettings.DiscoverMiddleware}");
Console.WriteLine($"  Notification MW: {diagnostics.DiscoverySettings.DiscoverNotificationMiddleware}");
Console.WriteLine($"  Constrained MW: {diagnostics.DiscoverySettings.DiscoverConstrainedMiddleware}");
Console.WriteLine($"  Handlers: {diagnostics.DiscoverySettings.DiscoverNotificationHandlers}");
```

### Diagnostics Properties

The `ConfigurationDiagnostics` class provides comprehensive information about configuration state:

- **Environment**: Current environment name
- **Timestamp**: When diagnostics were generated
- **IsValid**: Overall validation status
- **ValidationErrors**: List of validation failures
- **LoggingWarnings**: Non-critical logging warnings
- **AssemblyCount**: Number of registered assemblies
- **HasStatistics/HasTelemetry/HasLogging**: Feature availability
- **StatisticsEnabled/TelemetryEnabled**: Feature status
- **StreamingTelemetryEnabled**: Streaming-specific telemetry status
- **StatisticsRetentionPeriod**: Current retention setting
- **DiscoverySettings**: All discovery configuration flags

## Real-World Usage Examples

These real-world examples showcase how Blazing.Mediator integrates seamlessly into different application architectures and deployment scenarios. Each example represents a complete, production-ready configuration that demonstrates best practices for specific technology stacks and use cases, from modern Blazor WebAssembly applications to traditional console applications with sophisticated configuration requirements.

### Blazor WebAssembly Project
```csharp
builder.Services.AddMediator(config =>
{
    config.WithEnvironmentConfiguration(builder.Configuration, builder.Environment)
          .AddAssembly<Program>();
});
```

### ASP.NET Core Web API
```csharp
builder.Services.AddMediator(config =>
{
    config.WithEnvironmentConfiguration(builder.Configuration, builder.Environment)
          .AddAssembly(typeof(Program).Assembly)
          .AddAssembly(typeof(CreateOrderHandler).Assembly);
});
```

### Console Application with Custom Configuration
```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddMediator(config =>
        {
            config.WithEnvironmentConfiguration(context.Configuration, context.HostingEnvironment)
                  .ValidateForEnvironment(context.HostingEnvironment)
                  .AddAssembly(typeof(Program).Assembly);
        });
    })
    .Build();
```

## Configuration Examples

This comprehensive collection of configuration examples covers the full spectrum of Blazing.Mediator setup scenarios, from simple single-line configurations to complex enterprise environments with custom validation and advanced observability requirements. Each example is designed to be immediately applicable and demonstrates specific patterns that solve common configuration challenges while maintaining optimal performance and reliability.

### Basic Registration

The simplest way to get started requires minimal configuration and automatically discovers components:

```csharp
// Program.cs - Modern fluent configuration approach (RECOMMENDED)
builder.Services.AddMediator(config =>
{
    config.AddAssembly(typeof(Program).Assembly);
});

// Pre-configured instances for instant setup
builder.Services.AddMediator(MediatorConfiguration.Production(typeof(Program).Assembly));

// Basic registration with no configuration
builder.Services.AddMediator();
```

### Pre-Configured Instance Examples

The pre-configured instance support provides instant setup for common scenarios:

```csharp
// Production deployment - optimized for performance and essential monitoring
builder.Services.AddMediator(MediatorConfiguration.Production(typeof(Program).Assembly));

// Development environment - full features for debugging
builder.Services.AddMediator(MediatorConfiguration.Development(typeof(Program).Assembly));

// High-performance scenarios - minimal overhead
builder.Services.AddMediator(MediatorConfiguration.Minimal(typeof(Program).Assembly));

// Event-driven applications - notification-focused
builder.Services.AddMediator(MediatorConfiguration.NotificationOptimized(typeof(Program).Assembly));

// Real-time streaming - streaming-focused
builder.Services.AddMediator(MediatorConfiguration.StreamingOptimized(typeof(Program).Assembly));

// Maximum performance - all optional features disabled
builder.Services.AddMediator(MediatorConfiguration.Disabled(typeof(Program).Assembly));
```

### Multi-Assembly Pre-Configured Registration

Pre-configured instances support multiple assemblies just like the static factory methods:

```csharp
// Production configuration with multiple assemblies
builder.Services.AddMediator(MediatorConfiguration.Production(
    typeof(Program).Assembly,                    // API layer
    typeof(GetUserHandler).Assembly,             // Application layer
    typeof(User).Assembly                        // Domain layer
));

// Development configuration with comprehensive features
builder.Services.AddMediator(MediatorConfiguration.Development(
    typeof(Program).Assembly,
    typeof(OrderHandler).Assembly,
    typeof(PaymentHandler).Assembly
));
```

### Middleware Configuration Examples

Configure middleware components that provide cross-cutting concerns across all request and notification processing:

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

Automatically discover and register middleware and handlers from assemblies:

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

Configure comprehensive monitoring and observability features:

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

Configure detailed logging for debugging and monitoring:

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

### Advanced Scenarios

#### Conditional Configuration

Configure different settings based on environment and deployment conditions:

```csharp
services.AddMediator(config =>
{
    if (builder.Environment.IsDevelopment())
    {
        config.WithDevelopmentPreset();
        
        // Additional development-only features
        config.WithTelemetry(options =>
        {
            options.PacketLevelTelemetryEnabled = true;
            options.CapturePacketSize = true;
        });
    }
    else if (builder.Environment.IsProduction())
    {
        config.WithProductionPreset();
        
        // Production-specific overrides
        config.WithStatisticsTracking(options =>
        {
            options.MetricsRetentionPeriod = TimeSpan.FromDays(7);
        });
    }
    else
    {
        config.WithMinimalPreset();
    }
    
    // Apply common configuration
    config.WithConfiguration(builder.Configuration)
          .AddAssembly(typeof(Program).Assembly);
});
```

#### Configuration Validation Pipeline

Create reusable configuration validation patterns:

```csharp
public static class ConfigurationExtensions
{
    public static MediatorConfiguration WithValidatedConfiguration(
        this MediatorConfiguration config,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        return config
            .WithEnvironmentConfiguration(configuration, environment)
            .ValidateForEnvironment(environment)
            .LogDiagnostics(environment);
    }
    
    private static MediatorConfiguration LogDiagnostics(
        this MediatorConfiguration config,
        IHostEnvironment environment)
    {
        var diagnostics = config.GetDiagnostics(environment);
        
        Console.WriteLine($"?? Mediator Configuration for {diagnostics.Environment}:");
        Console.WriteLine($"   Valid: {(diagnostics.IsValid ? "?" : "?")}");
        Console.WriteLine($"   Statistics: {(diagnostics.StatisticsEnabled ? "?" : "?")}");
        Console.WriteLine($"   Telemetry: {(diagnostics.TelemetryEnabled ? "?" : "?")}");
        
        return config;
    }
}

// Usage
services.AddMediator(config =>
{
    config.WithValidatedConfiguration(builder.Configuration, builder.Environment)
          .AddAssembly(typeof(Program).Assembly);
});
```

#### Dynamic Configuration Updates

Implement dynamic configuration management for runtime configuration changes:

```csharp
public class MediatorConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<MediatorConfigurationService> _logger;

    public MediatorConfigurationService(
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<MediatorConfigurationService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public MediatorConfiguration CreateConfiguration()
    {
        var config = new MediatorConfiguration();
        
        try
        {
            config.WithEnvironmentConfiguration(_configuration, _environment);
            
            var diagnostics = config.GetDiagnostics(_environment);
            _logger.LogInformation(
                "Mediator configuration created for {Environment}. Valid: {IsValid}, Features: Statistics={HasStats}, Telemetry={HasTelemetry}",
                diagnostics.Environment,
                diagnostics.IsValid,
                diagnostics.HasStatistics,
                diagnostics.HasTelemetry);
                
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create mediator configuration");
            
            // Fallback to minimal configuration
            _logger.LogWarning("Falling back to minimal configuration");
            return config.WithMinimalPreset();
        }
    }
}
```

### Complete Application Setup Examples

#### ASP.NET Core Web API Application
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
    config.WithEnvironmentConfiguration(builder.Configuration, builder.Environment)
          .AddAssembly(typeof(Program).Assembly)
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

#### Minimal API Application
```csharp
// For Minimal APIs (Recommended for Simple APIs)
using Blazing.Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Mediator using fluent configuration
builder.Services.AddMediator(config =>
{
    config.WithEnvironmentConfiguration(builder.Configuration, builder.Environment)
          .AddAssembly(typeof(Program).Assembly);
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

## Performance Impact

The configuration system is designed for optimal performance across all scenarios:

- **Zero overhead** when features are disabled
- **Minimal startup cost** for configuration loading
- **Memory efficient** through existing class reuse
- **Production optimized** with automatic validation

## Best Practices

1. **Use Environment-Aware Configuration**: Always use `WithEnvironmentConfiguration` for production applications
2. **Consider Pre-Configured Instances**: Use `MediatorConfiguration.Production()` and similar methods for quick, battle-tested setup
3. **Validate Early**: Call `ValidateForEnvironment()` during startup to catch issues early
4. **Monitor Diagnostics**: Use `GetDiagnostics()` for health checks and monitoring
5. **Environment-Specific Settings**: Use multiple configuration sources for environment overrides
6. **Fail Safe**: Always have fallback configuration for error scenarios
7. **Log Configuration**: Log configuration diagnostics for troubleshooting
8. **External Configuration**: Use environment variables and command line arguments for deployment flexibility
9. **Security**: Store sensitive configuration in secure vaults and secret management systems
10. **Start Simple**: Begin with pre-configured instances, then customize as needed
11. **Consistency**: Use the same configuration approach across your organization for maintainability

### Configuration Approach Decision Tree

```
Are you starting a new project?
├─ Yes: Use pre-configured instances (MediatorConfiguration.Production())
│   └─ Need customization? Add fluent configuration on top
└─ No: Are you migrating from another mediator?
    ├─ Yes: Use configuration actions for gradual migration
    └─ No: Do you have specific performance requirements?
        ├─ Yes: Use configuration actions for fine control
        └─ No: Consider switching to pre-configured instances for simplicity
```

This comprehensive configuration guide provides all the tools and knowledge needed to configure Blazing.Mediator for any deployment scenario, from simple development environments to complex enterprise production systems.
