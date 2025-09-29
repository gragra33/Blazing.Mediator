using Blazing.Mediator.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Blazing.Mediator.Tests.Configuration;

public class FluentValidationTests
{
    [Fact]
    public void ValidateForEnvironment_WithCustomValidation_ShouldApplyCustomRules()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .WithStatisticsTracking(options => 
            {
                options.MetricsRetentionPeriod = TimeSpan.FromMinutes(30);
                options.CleanupInterval = TimeSpan.FromMinutes(15); // Fix: Ensure cleanup interval is less than retention
            })
            .WithTelemetry()
            .AddAssembly(typeof(FluentValidationTests).Assembly);

        var environment = new TestHostEnvironment { EnvironmentName = "CustomEnvironment" };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            config.ValidateForEnvironment(environment, validation =>
            {
                validation.Statistics(stats =>
                    stats.RetentionPeriodAtLeast(TimeSpan.FromHours(2), "Custom rule: retention must be at least 2 hours"));
            });
        });

        Assert.Contains("Custom rule: retention must be at least 2 hours", exception.Message);
    }

    [Fact]
    public void ValidateForEnvironment_WithEnvironmentSpecificRules_ShouldApplyRulesOnlyForTargetEnvironment()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .WithTelemetry(options => options.PacketLevelTelemetryEnabled = true)
            .AddAssembly(typeof(FluentValidationTests).Assembly);

        var productionEnvironment = new TestHostEnvironment { EnvironmentName = "Production" };
        var developmentEnvironment = new TestHostEnvironment { EnvironmentName = "Development" };

        // Act & Assert - Should fail for production
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            config.ValidateForEnvironment(productionEnvironment, validation =>
            {
                validation.ForProduction(cfg => cfg.TelemetryOptions?.PacketLevelTelemetryEnabled != true,
                    "Packet-level telemetry is not allowed in production");
            });
        });

        Assert.Contains("Packet-level telemetry is not allowed in production", exception.Message);

        // Should pass for development (add logging to avoid built-in validation error)
        var devConfig = new MediatorConfiguration()
            .WithTelemetry(options => options.PacketLevelTelemetryEnabled = true)
            .WithLogging() // Add logging to satisfy development environment requirements
            .AddAssembly(typeof(FluentValidationTests).Assembly);

        devConfig.ValidateForEnvironment(developmentEnvironment, validation =>
        {
            validation.ForProduction(cfg => cfg.TelemetryOptions?.PacketLevelTelemetryEnabled != true,
                "Packet-level telemetry is not allowed in production");
        });
    }

    [Fact]
    public void ValidateForEnvironment_WithTelemetryValidation_ShouldValidateTelemetrySettings()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .WithTelemetry(options => options.Enabled = false)
            .AddAssembly(typeof(FluentValidationTests).Assembly);

        var environment = new TestHostEnvironment { EnvironmentName = "Development" };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            config.ValidateForEnvironment(environment, validation =>
            {
                validation.Telemetry(telemetry =>
                    telemetry.MustBeEnabled("Telemetry is required for development environments"));
            });
        });

        Assert.Contains("Telemetry is required for development environments", exception.Message);
    }

    [Fact]
    public void ValidateForEnvironment_WithLoggingValidation_ShouldValidateLoggingSettings()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .WithLogging(options =>
            {
                options.EnableDetailedHandlerInfo = true;
                options.EnableConstraintLogging = true;
            })
            .AddAssembly(typeof(FluentValidationTests).Assembly);

        var environment = new TestHostEnvironment { EnvironmentName = "Production" };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            config.ValidateForEnvironment(environment, validation =>
            {
                validation.ForProduction(cfg => true, "Built-in validation will catch this");
                validation.Logging(logging =>
                    logging.DetailedFeaturesMustBeDisabled("Custom validation: detailed logging not allowed"));
            });
        });

        // Should contain both built-in and custom error messages
        Assert.Contains("detailed logging enabled for performance reasons", exception.Message);
        Assert.Contains("Custom validation: detailed logging not allowed", exception.Message);
    }

    [Fact]
    public void ValidateForEnvironment_WithComplexCustomRules_ShouldSupportComplexValidationScenarios()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .WithStatisticsTracking(options =>
            {
                options.EnableDetailedAnalysis = true;
                options.EnablePerformanceCounters = false; // Fix: Disable performance counters for production
                options.MetricsRetentionPeriod = TimeSpan.FromDays(7);
                options.CleanupInterval = TimeSpan.FromHours(1); // Fix: Ensure cleanup interval is reasonable
            })
            .WithTelemetry(options =>
            {
                options.Enabled = true;
                options.PacketLevelTelemetryEnabled = false;
            })
            .AddAssembly(typeof(FluentValidationTests).Assembly);

        var environment = new TestHostEnvironment { EnvironmentName = "Production" };

        // Act - Should pass with complex validation rules
        config.ValidateForEnvironment(environment, validation =>
        {
            validation
                .Statistics(stats =>
                {
                    stats.MustBeEnabled();
                    stats.RetentionPeriodAtLeast(TimeSpan.FromDays(1));
                    stats.Rule(options => options?.EnableDetailedAnalysis != true || options?.MetricsRetentionPeriod >= TimeSpan.FromDays(7),
                        "Detailed analysis requires at least 7 days retention");
                })
                .Telemetry(telemetry =>
                {
                    telemetry.MustBeEnabled();
                    telemetry.PacketLevelMustBeDisabled();
                })
                .ForProduction(cfg => cfg.StatisticsOptions?.EnablePerformanceCounters != true,
                    "Performance counters should be disabled in production")
                .Rule((cfg, env) => env.EnvironmentName == "Production" && cfg.Assemblies.Count > 0,
                    "Production must have at least one assembly configured");
        });

        // Should not throw any exceptions
    }

    [Fact]
    public void ValidateForEnvironment_WithChainedValidations_ShouldSupportMethodChaining()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .WithStatisticsTracking()
            .WithTelemetry()
            .WithLogging()
            .AddAssembly(typeof(FluentValidationTests).Assembly);

        var environment = new TestHostEnvironment { EnvironmentName = "Development" };

        // Act - Should support fluent chaining
        var result = config.ValidateForEnvironment(environment, validation =>
        {
            validation
                .Statistics(stats => stats.MustBeEnabled())
                .Telemetry(telemetry => telemetry.MustBeEnabled())
                .Logging(logging => logging.MustBeEnabled())
                .ForDevelopment(cfg => cfg.Assemblies.Count > 0, "Development must have assemblies")
                .Rule(cfg => cfg.StatisticsOptions != null, "Statistics must be configured");
        });

        // Assert - Should return the same configuration for chaining
        Assert.Same(config, result);
    }

    private class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "TestApp";
        public string ContentRootPath { get; set; } = "/test";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}