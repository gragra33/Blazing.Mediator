# ConfigurationExample - Blazing.Mediator Configuration Features

A comprehensive demonstration of Blazing.Mediator's **advanced configuration features**, showcasing environment-aware settings, JSON configuration, preset application, and diagnostics capabilities.

## 📋 Table of Contents

- [🏗️ Architecture](#️-architecture)
- [🎯 Design Principles](#-design-principles)
- [🔧 Features Demonstrated](#-features-demonstrated)
  - [🌍 Environment-Aware Configuration](#-environment-aware-configuration)
  - [📝 JSON Configuration Support](#-json-configuration-support)
  - [🎛️ Preset Integration](#️-preset-integration)
  - [🔍 Configuration Diagnostics](#-configuration-diagnostics)
  - [✅ Environment-Specific Validation](#-environment-specific-validation)
- [🔄 Configuration Examples](#-configuration-examples)
- [🏃‍♂️ Running the Example](#️-running-the-example)
- [🌟 Key Features](#-key-features)
- [📚 Key Learnings](#-key-learnings)
- [🛠️ Technologies Used](#️-technologies-used)
- [📖 Further Reading](#-further-reading)

## 🏗️ Architecture

This project demonstrates **advanced configuration features** with a focus on:

```
ConfigurationExample/
├── Program.cs                    # Main demonstration program
├── ConfigurationExample.csproj   # Project configuration
└── appsettings.*.json           # Environment-specific configuration files
```

**Key Architectural Concepts:**
- **Environment-Aware Configuration**: Automatic preset selection based on environment
- **Configuration Layering**: JSON configuration overrides preset defaults
- **Diagnostics Integration**: Real-time configuration health monitoring
- **Production Safety**: Intelligent validation for deployment environments

## 🎯 Design Principles

This example showcases advanced configuration principles:

- **🌍 Environment Awareness**: Automatic configuration based on deployment environment
- **🎛️ Configuration Layering**: JSON overrides combined with preset defaults
- **🔍 Observability**: Comprehensive diagnostics and validation
- **⚡ Performance**: Zero overhead when features are disabled
- **🛡️ Safety**: Production validation prevents performance issues

## 🔧 Features Demonstrated

### 🌍 Environment-Aware Configuration

**Automatic Environment Detection and Preset Application:**

```csharp
// One line handles everything - detects environment and applies appropriate preset + JSON overrides
services.AddMediator(config =>
{
    config.WithEnvironmentConfiguration(builder.Configuration, builder.Environment)
          .AddAssembly(typeof(Program).Assembly);
});
```

**Environment Mapping:**
- **Development** → `WithDevelopmentPreset()` (Full features + verbose logging)
- **Production** → `WithProductionPreset()` (Optimized performance + essential monitoring)
- **Testing** → `WithDisabledPreset()` (Minimal overhead for testing)
- **Custom** → `WithMinimalPreset()` (Safe defaults for unknown environments)

### 📝 JSON Configuration Support

**Clean Implementation - Reuses Existing Classes:**

```json
{
  "Blazing": {
    "Mediator": {
      "Statistics": {
        "EnableRequestMetrics": true,
        "EnableNotificationMetrics": true,
        "MetricsRetentionPeriod": "01:00:00"
      },
      "Telemetry": {
        "Enabled": true,
        "PacketLevelTelemetryEnabled": false
      },
      "Logging": {
        "EnableSend": true,
        "EnableDetailedHandlerInfo": false
      },
      "Discovery": {
        "DiscoverMiddleware": true,
        "DiscoverNotificationHandlers": true
      }
    }
  }
}
```

**Key Benefits:**
- ✅ **Same Property Names**: Uses existing `StatisticsOptions`, `TelemetryOptions`, `LoggingOptions`
- ✅ **Same Validation**: Leverages existing validation logic
- ✅ **IntelliSense Support**: Full IDE support with familiar properties
- ✅ **Environment-Specific**: Different settings per environment

### 🎛️ Preset Integration

**Fluent Preset Application:**

```csharp
// Apply preset then JSON overrides
services.AddMediator(config =>
{
    config.WithDevelopmentPreset()                    // Apply preset
          .WithConfiguration(builder.Configuration)   // Then JSON overrides
          .AddAssembly(typeof(Program).Assembly);     // Then assemblies
});
```

**Available Presets:**
- `WithDevelopmentPreset()` - Full features for debugging
- `WithProductionPreset()` - Optimized for performance
- `WithDisabledPreset()` - Minimal overhead
- `WithMinimalPreset()` - Basic functionality
- `WithNotificationOptimizedPreset()` - Event-driven focus
- `WithStreamingOptimizedPreset()` - Real-time data focus

### 🔍 Configuration Diagnostics

**Comprehensive Configuration Health Monitoring:**

```csharp
var diagnostics = config.GetDiagnostics(environment);

Console.WriteLine($"Environment: {diagnostics.Environment}");
Console.WriteLine($"Valid: {diagnostics.IsValid}");
Console.WriteLine($"Statistics: {(diagnostics.StatisticsEnabled ? "Enabled" : "Disabled")}");
Console.WriteLine($"Telemetry: {(diagnostics.TelemetryEnabled ? "Enabled" : "Disabled")}");
Console.WriteLine($"Assemblies: {diagnostics.AssemblyCount}");

if (!diagnostics.IsValid)
{
    foreach (var error in diagnostics.ValidationErrors)
        Console.WriteLine($"❌ {error}");
}
```

### ✅ Environment-Specific Validation

**Production Safety Guards:**

```csharp
try
{
    config.ValidateForEnvironment(environment);
    Console.WriteLine("✅ Configuration is valid for " + environment.EnvironmentName);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"❌ Configuration validation failed: {ex.Message}");
}
```

**Production Validation Rules:**
- ❌ No verbose logging (performance impact)
- ❌ No packet-level telemetry (overhead)
- ✅ Reasonable metrics retention (≥1 hour)
- ✅ Essential monitoring enabled

**Custom Fluent Validation:**

```csharp
config.ValidateForEnvironment(environment, validation =>
{
    validation
        .Statistics(stats =>
        {
            stats.MustBeEnabled("Statistics required for monitoring");
            stats.RetentionPeriodAtLeast(TimeSpan.FromHours(2), "Production requires extended retention");
        })
        .Telemetry(telemetry =>
        {
            telemetry.MustBeEnabled("Telemetry required for observability");
            telemetry.PacketLevelMustBeDisabled("Performance optimization");
        })
        .ForProduction(cfg => cfg.Assemblies.Count > 0, "Production must have assemblies")
        .Rule((cfg, env) => env.IsProduction() && cfg.StatisticsOptions != null,
            "Production environments require statistics");
});
```

## 🔄 Configuration Examples

### Example 1: Environment-Aware Configuration

**Automatically detects environment and applies appropriate settings:**

```csharp
services.AddMediator(config =>
{
    config.WithEnvironmentConfiguration(builder.Configuration, builder.Environment)
          .AddAssembly(typeof(Program).Assembly);
});
```

### Example 2: Mixed Configuration (Preset + JSON)

**Apply preset first, then JSON overrides:**

```csharp
services.AddMediator(config =>
{
    config.WithDevelopmentPreset()                    // Apply development preset
          .WithConfiguration(builder.Configuration)   // Apply JSON overrides
          .AddAssembly(typeof(Program).Assembly);
});
```

### Example 3: Custom Section Path

**Use custom JSON section paths:**

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

### Example 4: Validation Pipeline

**Comprehensive validation with diagnostics:**

```csharp
services.AddMediator(config =>
{
    config.WithEnvironmentConfiguration(builder.Configuration, builder.Environment)
          .ValidateForEnvironment(builder.Environment)
          .AddAssembly(typeof(Program).Assembly);
});
```

## 🏃‍♂️ Running the Example

### Prerequisites

- .NET 9.0 or later
- Terminal/Command Prompt or Visual Studio

### Steps

1. Navigate to the example directory:
   ```bash
   cd src/samples/ConfigurationExample
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

### Expected Output

The application demonstrates 4 comprehensive examples:

```
=================================================================
*** Blazing.Mediator - Configuration Example ***
=================================================================

📋 Example 1: Environment-Aware Configuration
===============================================

🌍 Environment: Development
   ✅ Configured for Development environment
   📊 Statistics: Enabled
   📡 Telemetry: Enabled
   📝 Logging: Enabled
   🔍 Discovery: MW=True, NH=True

🌍 Environment: Production
   ✅ Configured for Production environment
   📊 Statistics: Enabled
   📡 Telemetry: Enabled
   📝 Logging: Enabled
   🔍 Discovery: MW=True, NH=True

🔄 Example 2: Mixed Configuration (Preset + JSON)
==================================================
   🎛️  Applied Development preset, then JSON overrides:
   📊 Metrics Retention: 04:00:00 (overridden)
   🔬 Detailed Analysis: False (overridden)
   📦 Packet Telemetry: False (overridden)
   🔍 Discover MW: False (overridden)
   ✅ Other settings retained from Development preset

🔧 Example 3: Configuration Diagnostics
========================================
   📋 Configuration Diagnostics Report:
   🌍 Environment: Development
   ⏰ Generated: 2025-09-29 13:45:24 UTC
   ✅ Valid: True
   📚 Assemblies: 1
   📊 Statistics: Enabled
   📡 Telemetry: Enabled
   🌊 Streaming Telemetry: Enabled
   🕐 Retention Period: 01:00:00

✅ Example 4: Environment-Specific Validation
==============================================
   🏭 Testing Production Configuration:
   ✅ Production configuration is valid

   🏭 Testing Invalid Production Configuration:
   ✅ Correctly caught invalid configuration: Production environment should not have detailed logging enabled for performance reasons
```

## 🌟 Key Features

This ConfigurationExample project showcases key features:

### 🧠 **Smart Configuration Binding**
- **Explicit Override Detection**: Only applies JSON values that are explicitly configured
- **Preset Preservation**: Maintains preset defaults for non-configured options
- **Clean Implementation**: Reuses existing `StatisticsOptions`, `TelemetryOptions`, `LoggingOptions`

### 🌍 **Environment Intelligence**
- **Automatic Detection**: Detects environment and applies appropriate preset
- **Environment-Specific Validation**: Prevents misconfiguration in production
- **Configuration Layering**: JSON overrides preset defaults intelligently

### 🔍 **Advanced Diagnostics**
- **Real-Time Health Monitoring**: Live configuration status and validation
- **Comprehensive Reporting**: Detailed diagnostics with actionable insights
- **Troubleshooting Tools**: Built-in configuration debugging capabilities

### 🛡️ **Production Safety**
- **Intelligent Validation**: Prevents performance-impacting settings in production
- **Environment Guards**: Enforces environment-appropriate configuration
- **Error Prevention**: Catches configuration issues before deployment

### 🎛️ **Fluent Preset Integration**
- **Seamless Integration**: Static factory methods integrate with fluent API
- **Preset Chaining**: Seamless integration with other configuration methods
- **Override Support**: JSON configuration can override preset values

## 📚 Key Learnings

This example teaches:

### **Configuration Management:**
- **Environment-Aware Configuration**: Automatic preset selection based on environment
- **Configuration Layering**: Combining presets with JSON overrides
- **Smart Binding**: Only applying explicitly configured values
- **Clean Implementation**: Reusing existing validation and options classes

### **Production Readiness:**
- **Environment-Specific Validation**: Ensuring appropriate settings per environment
- **Production Safety**: Preventing performance-impacting configurations
- **Configuration Diagnostics**: Real-time health monitoring and troubleshooting
- **Error Prevention**: Catching issues before deployment

### **Developer Experience:**
- **Familiar APIs**: Using existing property names and validation
- **Fluent Integration**: Seamless chaining with other configuration methods
- **IntelliSense Support**: Full IDE support for JSON configuration
- **Easy Migration**: Simple transition from code-based to JSON-based configuration

### **Advanced Patterns:**
- **Configuration Factories**: Environment-aware configuration creation
- **Validation Pipelines**: Comprehensive configuration validation
- **Diagnostics Integration**: Built-in health monitoring and reporting
- **Override Strategies**: Intelligent precedence handling

## 🛠️ Technologies Used

- **.NET 9.0**: Latest .NET framework
- **Blazing.Mediator**: CQRS and mediator pattern with advanced configuration
- **Microsoft.Extensions.Configuration**: Configuration abstraction
- **Microsoft.Extensions.Hosting**: Application hosting and environment detection
- **Microsoft.Extensions.Logging**: Structured logging
- **JSON Configuration**: Environment-specific configuration files

## 📖 Further Reading

- [Blazing.Mediator Documentation](../../docs/)
- [Configuration Guide](../../docs/MEDIATOR_CONFIGURATION.md)
- [Microsoft Configuration Documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Environment-based Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments)

---

## 🎯 Summary

This example demonstrates Blazing.Mediator's comprehensive configuration system, featuring:

- ✅ **Environment-Aware Configuration** with automatic preset selection
- ✅ **JSON Configuration Support** with clean implementation
- ✅ **Fluent Preset Integration** with seamless method chaining
- ✅ **Advanced Diagnostics** with real-time configuration monitoring
- ✅ **Production Safety** with intelligent environment-specific validation
- ✅ **Zero Breaking Changes** maintaining full backward compatibility

**The configuration system provides a comprehensive, environment-aware, production-ready solution for managing Blazing.Mediator settings.**