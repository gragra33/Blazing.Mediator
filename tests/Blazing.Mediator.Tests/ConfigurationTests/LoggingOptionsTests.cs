using Blazing.Mediator.Configuration;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Blazing.Mediator.Tests.ConfigurationTests;

/// <summary>
/// Comprehensive tests for logging options configuration and functionality.
/// Ensures that logging options are properly respected by the mediator system.
/// </summary>
public class LoggingOptionsTests
{
    private readonly Assembly _testAssembly = Assembly.GetExecutingAssembly();

    #region LoggingOptions Configuration Tests

    [Fact]
    public void LoggingOptions_DefaultConfiguration_HasCorrectValues()
    {
        // Arrange & Act
        var options = new LoggingOptions();

        // Assert - Test all default values
        Assert.True(options.EnableRequestMiddleware);
        Assert.True(options.EnableNotificationMiddleware);
        Assert.True(options.EnableSend);
        Assert.True(options.EnableSendStream);
        Assert.True(options.EnablePublish);
        Assert.True(options.EnableRequestPipelineResolution);
        Assert.True(options.EnableNotificationPipelineResolution);
        Assert.True(options.EnableWarnings);
        Assert.True(options.EnableQueryAnalyzer);
        Assert.True(options.EnableCommandAnalyzer);
        Assert.True(options.EnableStatistics);
        Assert.False(options.EnableDetailedTypeClassification);
        Assert.False(options.EnableDetailedHandlerInfo);
        Assert.False(options.EnableMiddlewareExecutionOrder);
        Assert.True(options.EnablePerformanceTiming);
        Assert.True(options.EnableSubscriberDetails);
        Assert.False(options.EnableConstraintLogging);
        Assert.False(options.EnableMiddlewareRoutingLogging);
    }

    [Fact]
    public void LoggingOptions_CreateMinimal_DisablesMostOptions()
    {
        // Arrange & Act
        var options = LoggingOptions.CreateMinimal();

        // Assert - Most options should be disabled
        Assert.False(options.EnableRequestMiddleware);
        Assert.False(options.EnableNotificationMiddleware);
        Assert.False(options.EnableSend);
        Assert.False(options.EnableSendStream);
        Assert.False(options.EnablePublish);
        Assert.False(options.EnableRequestPipelineResolution);
        Assert.False(options.EnableNotificationPipelineResolution);
        Assert.True(options.EnableWarnings); // Warnings should remain enabled
        Assert.False(options.EnableQueryAnalyzer);
        Assert.False(options.EnableCommandAnalyzer);
        Assert.False(options.EnableStatistics);
        Assert.False(options.EnableDetailedTypeClassification);
        Assert.False(options.EnableDetailedHandlerInfo);
        Assert.False(options.EnableMiddlewareExecutionOrder);
        Assert.False(options.EnablePerformanceTiming);
        Assert.False(options.EnableSubscriberDetails);
        Assert.False(options.EnableConstraintLogging);
        Assert.False(options.EnableMiddlewareRoutingLogging);
    }

    [Fact]
    public void LoggingOptions_CreateVerbose_EnablesAllOptions()
    {
        // Arrange & Act
        var options = LoggingOptions.CreateVerbose();

        // Assert - All options should be enabled
        Assert.True(options.EnableRequestMiddleware);
        Assert.True(options.EnableNotificationMiddleware);
        Assert.True(options.EnableSend);
        Assert.True(options.EnableSendStream);
        Assert.True(options.EnablePublish);
        Assert.True(options.EnableRequestPipelineResolution);
        Assert.True(options.EnableNotificationPipelineResolution);
        Assert.True(options.EnableWarnings);
        Assert.True(options.EnableQueryAnalyzer);
        Assert.True(options.EnableCommandAnalyzer);
        Assert.True(options.EnableStatistics);
        Assert.True(options.EnableDetailedTypeClassification);
        Assert.True(options.EnableDetailedHandlerInfo);
        Assert.True(options.EnableMiddlewareExecutionOrder);
        Assert.True(options.EnablePerformanceTiming);
        Assert.True(options.EnableSubscriberDetails);
        Assert.True(options.EnableConstraintLogging);
        Assert.True(options.EnableMiddlewareRoutingLogging);
    }

    [Fact]
    public void LoggingOptions_CreateConstraintDebugging_HasCorrectConfiguration()
    {
        // Arrange & Act
        var options = LoggingOptions.CreateConstraintDebugging();

        // Assert - Only constraint-related options should be enabled
        Assert.False(options.EnableRequestMiddleware);
        Assert.True(options.EnableNotificationMiddleware);
        Assert.False(options.EnableSend);
        Assert.False(options.EnableSendStream);
        Assert.True(options.EnablePublish);
        Assert.False(options.EnableRequestPipelineResolution);
        Assert.True(options.EnableNotificationPipelineResolution);
        Assert.True(options.EnableWarnings);
        Assert.False(options.EnableQueryAnalyzer);
        Assert.False(options.EnableCommandAnalyzer);
        Assert.False(options.EnableStatistics);
        Assert.False(options.EnableDetailedTypeClassification);
        Assert.False(options.EnableDetailedHandlerInfo);
        Assert.True(options.EnableMiddlewareExecutionOrder);
        Assert.True(options.EnablePerformanceTiming);
        Assert.False(options.EnableSubscriberDetails);
        Assert.True(options.EnableConstraintLogging);
        Assert.True(options.EnableMiddlewareRoutingLogging);
    }

    [Fact]
    public void LoggingOptions_Clone_CreatesExactCopy()
    {
        // Arrange
        var original = new LoggingOptions
        {
            EnableRequestMiddleware = false,
            EnableNotificationMiddleware = true,
            EnableSend = false,
            EnableStatistics = false,
            EnableDetailedTypeClassification = true
        };

        // Act
        var cloned = original.Clone();

        // Assert
        Assert.NotSame(original, cloned);
        Assert.Equal(original.EnableRequestMiddleware, cloned.EnableRequestMiddleware);
        Assert.Equal(original.EnableNotificationMiddleware, cloned.EnableNotificationMiddleware);
        Assert.Equal(original.EnableSend, cloned.EnableSend);
        Assert.Equal(original.EnableStatistics, cloned.EnableStatistics);
        Assert.Equal(original.EnableDetailedTypeClassification, cloned.EnableDetailedTypeClassification);
    }

    [Fact]
    public void LoggingOptions_Validate_ReturnValidationErrors()
    {
        // Arrange
        var options = new LoggingOptions
        {
            EnableConstraintLogging = true,
            EnableMiddlewareRoutingLogging = true,
            EnablePerformanceTiming = true
        };

        // Act
        var errors = options.Validate();

        // Assert
        Assert.Single(errors);
        Assert.Contains("performance", errors.First().ToLower());
    }

    [Fact]
    public void LoggingOptions_Validate_NoErrorsForValidConfiguration()
    {
        // Arrange
        var options = new LoggingOptions
        {
            EnableConstraintLogging = true,
            EnableMiddlewareRoutingLogging = false,
            EnablePerformanceTiming = true
        };

        // Act
        var errors = options.Validate();

        // Assert
        Assert.Empty(errors);
    }

    #endregion

    #region MediatorConfiguration Logging Integration Tests

    [Fact]
    public void MediatorConfiguration_WithLogging_ConfiguresDefaultLogging()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithLogging();

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.LoggingOptions);
        Assert.True(config.LoggingOptions.EnableSend);
        Assert.True(config.LoggingOptions.EnableStatistics);
    }

    [Fact]
    public void MediatorConfiguration_WithLogging_ActionConfiguration_ConfiguresCorrectly()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithLogging(options =>
        {
            options.EnableSend = false;
            options.EnableStatistics = false;
            options.EnableQueryAnalyzer = false;
        });

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.LoggingOptions);
        Assert.False(config.LoggingOptions.EnableSend);
        Assert.False(config.LoggingOptions.EnableStatistics);
        Assert.False(config.LoggingOptions.EnableQueryAnalyzer);
    }

    [Fact]
    public void MediatorConfiguration_WithLogging_PreConfiguredOptions_UsesProvidedOptions()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var customOptions = LoggingOptions.CreateMinimal();

        // Act
        var result = config.WithLogging(customOptions);

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.LoggingOptions);
        Assert.False(config.LoggingOptions.EnableSend);
        Assert.False(config.LoggingOptions.EnableStatistics);
        Assert.True(config.LoggingOptions.EnableWarnings); // Should remain true in minimal
    }

    [Fact]
    public void MediatorConfiguration_WithLogging_ValidationFailure_ThrowsException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        // Test null action parameter - this should throw ArgumentNullException
        Assert.Throws<ArgumentNullException>(() =>
            config.WithLogging((Action<LoggingOptions>)null!));
    }

    [Fact]
    public void MediatorConfiguration_WithLogging_ValidationWarning_DoesNotThrowException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act - This should not throw because warnings are not blocking errors
        var result = config.WithLogging(options =>
        {
            options.EnableConstraintLogging = true;
            options.EnableMiddlewareRoutingLogging = true;
            options.EnablePerformanceTiming = true;
        });

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.LoggingOptions);
        Assert.True(config.LoggingOptions.EnableConstraintLogging);
        Assert.True(config.LoggingOptions.EnableMiddlewareRoutingLogging);
        Assert.True(config.LoggingOptions.EnablePerformanceTiming);
    }

    [Fact]
    public void MediatorConfiguration_WithoutLogging_ClearsLoggingOptions()
    {
        // Arrange
        var config = new MediatorConfiguration();
        config.WithLogging(); // First enable
        Assert.NotNull(config.LoggingOptions);

        // Act
        var result = config.WithoutLogging();

        // Assert
        Assert.Same(config, result);
        Assert.Null(config.LoggingOptions);
    }

    #endregion

    #region Service Registration Tests

    [Fact]
    public void ServiceRegistration_WithLogging_RegistersLoggingOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.WithLogging()
                  .AddAssembly(_testAssembly);
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var loggingOptions = provider.GetService<LoggingOptions>();
        Assert.NotNull(loggingOptions);
        Assert.True(loggingOptions.EnableSend);
    }

    [Fact]
    public void ServiceRegistration_WithoutLogging_DoesNotRegisterLoggingOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithoutLogging()
                  .AddAssembly(_testAssembly);
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var loggingOptions = provider.GetService<LoggingOptions>();
        Assert.Null(loggingOptions);
    }

    [Fact]
    public void ServiceRegistration_MediatorLogger_OnlyCreatedWhenLoggingEnabled()
    {
        // Arrange
        var servicesWithLogging = new ServiceCollection();
        var servicesWithoutLogging = new ServiceCollection();

        // Configure logging
        servicesWithLogging.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        servicesWithoutLogging.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        // Act
        servicesWithLogging.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithLogging()
                  .AddAssembly(_testAssembly);
        });

        servicesWithoutLogging.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithoutLogging()
                  .AddAssembly(_testAssembly);
        });

        // Assert
        var providerWithLogging = servicesWithLogging.BuildServiceProvider();
        var providerWithoutLogging = servicesWithoutLogging.BuildServiceProvider();

        var loggingOptionsEnabled = providerWithLogging.GetService<LoggingOptions>();
        var loggingOptionsDisabled = providerWithoutLogging.GetService<LoggingOptions>();

        Assert.NotNull(loggingOptionsEnabled);
        Assert.Null(loggingOptionsDisabled);

        // Test that MediatorStatistics is created with the correct logger setup
        var statsWithLogging = providerWithLogging.GetService<MediatorStatistics>();
        var statsWithoutLogging = providerWithoutLogging.GetService<MediatorStatistics>();

        Assert.NotNull(statsWithLogging);
        Assert.NotNull(statsWithoutLogging);
    }

    #endregion

    #region Integration Tests with Test Requests

    // Simple test types for integration testing
    public class LoggingTestQuery : IRequest<string>
    {
        public string Message { get; set; } = "Test";
    }

    public class LoggingTestQueryHandler : IRequestHandler<LoggingTestQuery, string>
    {
        public Task<string> Handle(LoggingTestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Handled: {request.Message}");
        }
    }

    [Fact]
    public async Task IntegrationTest_WithLogging_ExecutesSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        services.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithLogging()
                  .AddAssembly(_testAssembly);
        });

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new LoggingTestQuery { Message = "Integration test" });

        // Assert
        Assert.Equal("Handled: Integration test", result);
    }

    [Fact]
    public async Task IntegrationTest_WithoutLogging_ExecutesSuccessfullyAndQuietly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        services.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithoutLogging()
                  .AddAssembly(_testAssembly);
        });

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new LoggingTestQuery { Message = "Integration test without logging" });

        // Assert
        Assert.Equal("Handled: Integration test without logging", result);
    }

    [Fact]
    public async Task IntegrationTest_MinimalLogging_ExecutesWithReducedLogging()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        services.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithLogging(options =>
                  {
                      // Configure minimal logging
                      var minimal = LoggingOptions.CreateMinimal();
                      options.EnableStatistics = minimal.EnableStatistics;
                      options.EnableSend = minimal.EnableSend;
                      options.EnableQueryAnalyzer = minimal.EnableQueryAnalyzer;
                      options.EnableCommandAnalyzer = minimal.EnableCommandAnalyzer;
                      options.EnableWarnings = true; // Keep warnings
                  })
                  .AddAssembly(_testAssembly);
        });

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new LoggingTestQuery { Message = "Minimal logging test" });

        // Assert
        Assert.Equal("Handled: Minimal logging test", result);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void MediatorConfiguration_WithLogging_NullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config.WithLogging((Action<LoggingOptions>)null!));
    }

    [Fact]
    public void MediatorConfiguration_WithLogging_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config.WithLogging((LoggingOptions)null!));
    }

    [Fact]
    public void ServiceRegistration_MultipleLoggingConfigurations_LastOneWins()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.WithLogging(opt => opt.EnableSend = true)
                  .WithLogging(opt => opt.EnableSend = false) // This should override
                  .AddAssembly(_testAssembly);
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var loggingOptions = provider.GetRequiredService<LoggingOptions>();
        Assert.False(loggingOptions.EnableSend);
    }

    [Fact]
    public void ServiceRegistration_LoggingAfterWithoutLogging_EnablesLogging()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.WithoutLogging()
                  .WithLogging() // This should re-enable
                  .AddAssembly(_testAssembly);
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var loggingOptions = provider.GetService<LoggingOptions>();
        Assert.NotNull(loggingOptions);
        Assert.True(loggingOptions.EnableSend);
    }

    #endregion
}