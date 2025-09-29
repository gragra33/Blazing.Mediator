using Blazing.Mediator;
using Blazing.Mediator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConfigurationExample;

/// <summary>
/// Demonstrates configuration features with real-world scenarios
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=================================================================");
        Console.WriteLine("*** Blazing.Mediator - Configuration Example ***");
        Console.WriteLine("=================================================================");
        Console.WriteLine();

        await RunEnvironmentAwareExample();
        await RunMixedConfigurationExample();
        await RunDiagnosticsExample();
        RunValidationExample();

        Console.WriteLine();
        Console.WriteLine("*** All examples completed successfully! ***");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    /// <summary>
    /// Demonstrates environment-aware configuration with automatic preset selection
    /// </summary>
    private static async Task RunEnvironmentAwareExample()
    {
        Console.WriteLine(">> Example 1: Environment-Aware Configuration");
        Console.WriteLine("===============================================");

        // Simulate different environments
        var environments = new[] { "Development", "Production", "Testing", "Staging" };

        foreach (var envName in environments)
        {
            Console.WriteLine($"\n[*] Environment: {envName}");
            
            var host = Host.CreateDefaultBuilder()
                .UseEnvironment(envName)
                .ConfigureAppConfiguration((_, config) =>
                {
                    // Add environment-specific configuration
                    var environmentConfig = GetEnvironmentConfiguration(envName);
                    config.AddInMemoryCollection(environmentConfig!);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddMediator(config =>
                    {
                        // Use environment-aware configuration
                        config.WithEnvironmentConfiguration(context.Configuration, context.HostingEnvironment)
                              .AddAssembly(typeof(Program).Assembly);
                        
                        Console.WriteLine($"   [+] Configured for {envName} environment");
                    });

                    services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
                })
                .Build();

            // Get diagnostics
            var mediatorConfig = new MediatorConfiguration()
                .WithEnvironmentConfiguration(host.Services.GetRequiredService<IConfiguration>(),
                                            host.Services.GetRequiredService<IHostEnvironment>())
                .AddAssembly(typeof(Program).Assembly);

            var diagnostics = mediatorConfig.GetDiagnostics(host.Services.GetRequiredService<IHostEnvironment>());
            
            Console.WriteLine($"   [#] Statistics: {(diagnostics.HasStatistics ? "Enabled" : "Disabled")}");
            Console.WriteLine($"   [#] Telemetry: {(diagnostics.HasTelemetry ? "Enabled" : "Disabled")}");
            Console.WriteLine($"   [#] Logging: {(diagnostics.HasLogging ? "Enabled" : "Disabled")}");
            Console.WriteLine($"   [?] Discovery: MW={diagnostics.DiscoverySettings.DiscoverMiddleware}, " +
                            $"NH={diagnostics.DiscoverySettings.DiscoverNotificationHandlers}");

            await host.StopAsync();
            host.Dispose();
        }
    }

    /// <summary>
    /// Demonstrates mixed configuration (preset + JSON overrides)
    /// </summary>
    private static async Task RunMixedConfigurationExample()
    {
        Console.WriteLine("\n\n>> Example 2: Mixed Configuration (Preset + JSON)");
        Console.WriteLine("==================================================");

        var configurationData = new Dictionary<string, string>
        {
            // Override development preset defaults
            ["Blazing:Mediator:Statistics:MetricsRetentionPeriod"] = "04:00:00",
            ["Blazing:Mediator:Statistics:EnableDetailedAnalysis"] = "false",
            ["Blazing:Mediator:Telemetry:PacketLevelTelemetryEnabled"] = "false",
            ["Blazing:Mediator:Discovery:DiscoverMiddleware"] = "false"
        };

        var host = Host.CreateDefaultBuilder()
            .UseEnvironment("Development")
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(configurationData!);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddMediator(config =>
                {
                    // Apply development preset first
                    config.WithDevelopmentPreset()
                          .WithConfiguration(context.Configuration) // Then apply JSON overrides
                          .AddAssembly(typeof(Program).Assembly);
                });

                services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
            })
            .Build();

        var mediatorConfig = new MediatorConfiguration()
            .WithDevelopmentPreset()
            .WithConfiguration(host.Services.GetRequiredService<IConfiguration>())
            .AddAssembly(typeof(Program).Assembly);

        Console.WriteLine("   [~] Applied Development preset, then JSON overrides:");
        Console.WriteLine($"   [#] Metrics Retention: {mediatorConfig.StatisticsOptions?.MetricsRetentionPeriod} (overridden)");
        Console.WriteLine($"   [#] Detailed Analysis: {mediatorConfig.StatisticsOptions?.EnableDetailedAnalysis} (overridden)");
        Console.WriteLine($"   [#] Packet Telemetry: {mediatorConfig.TelemetryOptions?.PacketLevelTelemetryEnabled} (overridden)");
        Console.WriteLine($"   [?] Discover MW: {mediatorConfig.DiscoverMiddleware} (overridden)");
        Console.WriteLine($"   [+] Other settings retained from Development preset");

        await host.StopAsync();
        host.Dispose();
    }

    /// <summary>
    /// Demonstrates configuration diagnostics and troubleshooting
    /// </summary>
    private static async Task RunDiagnosticsExample()
    {
        Console.WriteLine("\n\n>> Example 3: Configuration Diagnostics");
        Console.WriteLine("========================================");

        var host = Host.CreateDefaultBuilder()
            .UseEnvironment("Development")
            .ConfigureServices((_, services) =>
            {
                services.AddMediator(config =>
                {
                    config.WithDevelopmentPreset()
                          .AddAssembly(typeof(Program).Assembly);
                });

                services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
            })
            .Build();

        var mediatorConfig = new MediatorConfiguration()
            .WithDevelopmentPreset()
            .AddAssembly(typeof(Program).Assembly);

        var diagnostics = mediatorConfig.GetDiagnostics(host.Services.GetRequiredService<IHostEnvironment>());

        Console.WriteLine("   [#] Configuration Diagnostics Report:");
        Console.WriteLine($"   [*] Environment: {diagnostics.Environment}");
        Console.WriteLine($"   [@] Generated: {diagnostics.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine($"   [+] Valid: {diagnostics.IsValid}");
        Console.WriteLine($"   [#] Assemblies: {diagnostics.AssemblyCount}");
        Console.WriteLine($"   [#] Statistics: {(diagnostics.StatisticsEnabled ? "Enabled" : "Disabled")}");
        Console.WriteLine($"   [#] Telemetry: {(diagnostics.TelemetryEnabled ? "Enabled" : "Disabled")}");
        Console.WriteLine($"   [#] Streaming Telemetry: {(diagnostics.StreamingTelemetryEnabled ? "Enabled" : "Disabled")}");
        Console.WriteLine($"   [@] Retention Period: {diagnostics.StatisticsRetentionPeriod}");
        
        if (diagnostics.ValidationErrors.Any())
        {
            Console.WriteLine("   [!] Validation Errors:");
            foreach (var error in diagnostics.ValidationErrors)
            {
                Console.WriteLine($"      • {error}");
            }
        }

        if (diagnostics.LoggingWarnings.Any())
        {
            Console.WriteLine("   [!] Logging Warnings:");
            foreach (var warning in diagnostics.LoggingWarnings)
            {
                Console.WriteLine($"      • {warning}");
            }
        }

        await host.StopAsync();
        host.Dispose();
    }

    /// <summary>
    /// Demonstrates environment-specific validation
    /// </summary>
    private static void RunValidationExample()
    {
        Console.WriteLine("\n\n>> Example 4: Environment-Specific Validation");
        Console.WriteLine("==============================================");

        // Test production validation (should pass)
        Console.WriteLine("   [*] Testing Production Configuration:");
        try
        {
            var prodConfig = new MediatorConfiguration()
                .WithProductionPreset()
                .AddAssembly(typeof(Program).Assembly);

            var prodEnvironment = new Microsoft.Extensions.Hosting.Internal.HostingEnvironment
            {
                EnvironmentName = "Production"
            };

            prodConfig.ValidateForEnvironment(prodEnvironment);
            Console.WriteLine("   [+] Production configuration is valid");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [-] Production validation failed: {ex.Message}");
        }

        // Test invalid production configuration (should fail)
        Console.WriteLine("\n   [*] Testing Invalid Production Configuration:");
        try
        {
            var invalidProdConfig = new MediatorConfiguration()
                .WithTelemetry(options => options.PacketLevelTelemetryEnabled = true) // Invalid for production
                .WithLogging(options => options.EnableDetailedHandlerInfo = true) // Invalid for production
                .AddAssembly(typeof(Program).Assembly);

            var prodEnvironment = new Microsoft.Extensions.Hosting.Internal.HostingEnvironment
            {
                EnvironmentName = "Production"
            };

            invalidProdConfig.ValidateForEnvironment(prodEnvironment);
            Console.WriteLine("   [-] Should have failed validation!");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"   [+] Correctly caught invalid configuration: {ex.Message}");
        }

        // Test development validation
        Console.WriteLine("\n   [*] Testing Development Configuration:");
        try
        {
            var devConfig = new MediatorConfiguration()
                .WithDevelopmentPreset()
                .AddAssembly(typeof(Program).Assembly);

            var devEnvironment = new Microsoft.Extensions.Hosting.Internal.HostingEnvironment
            {
                EnvironmentName = "Development"
            };

            devConfig.ValidateForEnvironment(devEnvironment);
            Console.WriteLine("   [+] Development configuration is valid");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [-] Development validation failed: {ex.Message}");
        }

        // Test custom fluent validation
        Console.WriteLine("\n   [*] Testing Custom Fluent Validation:");
        try
        {
            var customConfig = new MediatorConfiguration()
                .WithStatisticsTracking(options => 
                {
                    options.MetricsRetentionPeriod = TimeSpan.FromMinutes(30);
                    options.CleanupInterval = TimeSpan.FromMinutes(15); // Fix: Ensure cleanup is less than retention
                    options.EnableDetailedAnalysis = true;
                })
                .WithTelemetry(options => options.PacketLevelTelemetryEnabled = true)
                .AddAssembly(typeof(Program).Assembly);

            var customEnvironment = new Microsoft.Extensions.Hosting.Internal.HostingEnvironment
            {
                EnvironmentName = "Production"
            };

            customConfig.ValidateForEnvironment(customEnvironment, validation =>
            {
                validation
                    .Statistics(stats =>
                    {
                        stats.RetentionPeriodAtLeast(TimeSpan.FromHours(2), "Custom rule: Production requires at least 2 hours retention");
                        stats.Rule(options => options?.EnableDetailedAnalysis != true, "Custom rule: Detailed analysis disabled in production");
                    })
                    .Telemetry(telemetry =>
                        telemetry.PacketLevelMustBeDisabled("Custom rule: Packet telemetry not allowed in production"))
                    .ForProduction(cfg => cfg.Assemblies.Count > 0, "Custom rule: Production must have assemblies configured");
            });

            Console.WriteLine("   [-] Should have failed custom validation!");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"   [+] Custom fluent validation correctly caught issues: {ex.Message}");
        }

        // Test successful custom validation
        Console.WriteLine("\n   [*] Testing Successful Custom Validation:");
        try
        {
            var successConfig = new MediatorConfiguration()
                .WithStatisticsTracking(options => 
                {
                    options.MetricsRetentionPeriod = TimeSpan.FromHours(4);
                    options.CleanupInterval = TimeSpan.FromHours(1); // Fix: Ensure cleanup is less than retention
                    options.EnableDetailedAnalysis = false;
                })
                .WithTelemetry(options => options.PacketLevelTelemetryEnabled = false)
                .WithLogging(options => 
                {
                    options.EnableDetailedHandlerInfo = false;
                    options.EnableConstraintLogging = false;
                })
                .AddAssembly(typeof(Program).Assembly);

            var prodEnvironment = new Microsoft.Extensions.Hosting.Internal.HostingEnvironment
            {
                EnvironmentName = "Production"
            };

            successConfig.ValidateForEnvironment(prodEnvironment, validation =>
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
                    .Rule((cfg, env) => env.EnvironmentName == "Production" && cfg.Assemblies.Count > 0,
                        "Production environments must have assemblies configured");
            });

            Console.WriteLine("   [+] Custom validation passed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [-] Unexpected validation failure: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets environment-specific configuration data
    /// </summary>
    private static Dictionary<string, string> GetEnvironmentConfiguration(string environment)
    {
        return environment switch
        {
            "Development" => new Dictionary<string, string>
            {
                ["Blazing:Mediator:Statistics:EnableDetailedAnalysis"] = "true",
                ["Blazing:Mediator:Telemetry:PacketLevelTelemetryEnabled"] = "true",
                ["Blazing:Mediator:Logging:EnableDetailedHandlerInfo"] = "true"
            },
            "Production" => new Dictionary<string, string>
            {
                ["Blazing:Mediator:Statistics:MetricsRetentionPeriod"] = "24:00:00",
                ["Blazing:Mediator:Telemetry:PacketLevelTelemetryEnabled"] = "false",
                ["Blazing:Mediator:Logging:EnableDetailedHandlerInfo"] = "false"
            },
            "Testing" => new Dictionary<string, string>
            {
                ["Blazing:Mediator:Statistics:EnableRequestMetrics"] = "false",
                ["Blazing:Mediator:Telemetry:Enabled"] = "false",
                ["Blazing:Mediator:Discovery:DiscoverMiddleware"] = "false"
            },
            _ => new Dictionary<string, string>
            {
                ["Blazing:Mediator:Statistics:EnableRequestMetrics"] = "true",
                ["Blazing:Mediator:Discovery:DiscoverNotificationHandlers"] = "true"
            }
        };
    }
}

// Sample handler for assembly scanning
public record SampleNotification : INotification;

public class SampleNotificationHandler : INotificationHandler<SampleNotification>
{
    public Task Handle(SampleNotification notification, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}