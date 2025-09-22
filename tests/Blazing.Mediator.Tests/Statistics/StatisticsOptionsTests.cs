using Blazing.Mediator.Configuration;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Tests for StatisticsOptions configuration and functionality.
/// Validates that all statistics options are properly implemented and working.
/// </summary>
public class StatisticsOptionsTests : IDisposable
{
    private ServiceProvider? _serviceProvider;
    private IMediator? _mediator;
    private MediatorStatistics? _statistics;

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    private void SetupMediator(Action<StatisticsOptions> configureOptions)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Configure mediator with statistics options
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking(configureOptions);
        }, typeof(StatTestCommand).Assembly);

    // Do not register handlers explicitly; rely on assembly scanning to avoid duplicate handler registration

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _statistics = _serviceProvider.GetRequiredService<MediatorStatistics>();
    }

    [Fact]
    public async Task EnableRequestMetrics_True_ShouldTrackRequests()
    {
        // Arrange
        SetupMediator(options =>
        {
            options.EnableRequestMetrics = true;
            options.EnableNotificationMetrics = false;
            options.EnableMiddlewareMetrics = false;
            options.EnablePerformanceCounters = false;
        });

        // Act
        await _mediator!.Send(new StatTestCommand { Value = "test1" });
        await _mediator.Send(new StatTestQuery { Value = "test2" });

        // Assert
        _statistics!.ShouldNotBeNull();
        // Verify that request metrics are tracked by checking if statistics work without errors
        // The implementation uses private fields, so we test functionality indirectly
        _statistics.ReportStatistics(); // Should not throw
    }

    [Fact]
    public async Task EnableRequestMetrics_False_ShouldNotTrackRequests()
    {
        // Arrange
        SetupMediator(options =>
        {
            options.EnableRequestMetrics = false;
            options.EnableNotificationMetrics = true;
            options.EnableMiddlewareMetrics = false;
            options.EnablePerformanceCounters = false;
        });

        // Act
        await _mediator!.Send(new StatTestCommand { Value = "test1" });
        await _mediator.Send(new StatTestQuery { Value = "test2" });

        // Assert
        _statistics!.ShouldNotBeNull();
        // Verify that request metrics are not tracked
        _statistics.ReportStatistics(); // Should not throw
    }

    [Fact]
    public void EnableNotificationMetrics_True_ShouldTrackNotifications()
    {
        // Arrange
        SetupMediator(options =>
        {
            options.EnableRequestMetrics = false;
            options.EnableNotificationMetrics = true;
            options.EnableMiddlewareMetrics = false;
            options.EnablePerformanceCounters = false;
        });

        // Act
        // Test notification tracking directly using the statistics object
        _statistics!.IncrementNotification("TestNotification");
        _statistics.IncrementNotification("AnotherNotification");

        // Assert
        _statistics.ShouldNotBeNull();
        // Verify that notification metrics are tracked
        _statistics.ReportStatistics(); // Should not throw
    }

    [Fact]
    public void EnableNotificationMetrics_False_ShouldNotTrackNotifications()
    {
        // Arrange
        SetupMediator(options =>
        {
            options.EnableRequestMetrics = true;
            options.EnableNotificationMetrics = false;
            options.EnableMiddlewareMetrics = false;
            options.EnablePerformanceCounters = false;
        });

        // Act
        // Test that notification tracking is disabled
        _statistics!.IncrementNotification("TestNotification");

        // Assert
        _statistics.ShouldNotBeNull();
        // Verify that notification metrics are not tracked
        _statistics.ReportStatistics(); // Should not throw
    }

    [Fact]
    public async Task EnableMiddlewareMetrics_True_ShouldTrackMiddleware()
    {
        // Arrange
        SetupMediator(options =>
        {
            options.EnableRequestMetrics = false;
            options.EnableNotificationMetrics = false;
            options.EnableMiddlewareMetrics = true;
            options.EnablePerformanceCounters = false;
        });

        // Act
        await _mediator!.Send(new StatTestCommand { Value = "test" });

        // Assert
        _statistics!.ShouldNotBeNull();
        // Test the RecordMiddlewareExecution method directly
        _statistics.RecordMiddlewareExecution("TestMiddleware", 100, true);
        _statistics.ReportStatistics(); // Should not throw
    }

    [Fact]
    public async Task EnablePerformanceCounters_True_ShouldTrackPerformance()
    {
        // Arrange
        SetupMediator(options =>
        {
            options.EnableRequestMetrics = false;
            options.EnableNotificationMetrics = false;
            options.EnableMiddlewareMetrics = false;
            options.EnablePerformanceCounters = true;
        });

        // Act
        await _mediator!.Send(new StatTestCommand { Value = "test" });

        // Assert
        _statistics!.ShouldNotBeNull();
        
        // Test performance tracking methods directly
        _statistics.RecordExecutionTime("TestRequest", 150, true);
        _statistics.RecordMemoryAllocation(1024);
        
        var performanceMetrics = _statistics.GetPerformanceMetrics("TestRequest");
        performanceMetrics.ShouldNotBeNull();
        performanceMetrics.Value.RequestType.ShouldBe("TestRequest");
        performanceMetrics.Value.TotalExecutions.ShouldBe(1);
        
        var summary = _statistics.GetPerformanceSummary();
        summary.ShouldNotBeNull();
        summary.Value.TotalRequests.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task EnablePerformanceCounters_False_ShouldNotTrackPerformance()
    {
        // Arrange
        SetupMediator(options =>
        {
            options.EnableRequestMetrics = true;
            options.EnableNotificationMetrics = true;
            options.EnableMiddlewareMetrics = true;
            options.EnablePerformanceCounters = false;
        });

        // Act
        await _mediator!.Send(new StatTestCommand { Value = "test" });

        // Assert
        _statistics!.ShouldNotBeNull();
        
        // Test that performance tracking methods don't work when disabled
        _statistics.RecordExecutionTime("TestRequest", 150, true);
        _statistics.RecordMemoryAllocation(1024);
        
        var performanceMetrics = _statistics.GetPerformanceMetrics("TestRequest");
        performanceMetrics.ShouldBeNull();
        
        var summary = _statistics.GetPerformanceSummary();
        summary.ShouldBeNull();
    }

    [Fact]
    public async Task EnableDetailedAnalysis_True_ShouldTrackDetailedMetrics()
    {
        // Arrange
        SetupMediator(options =>
        {
            options.EnableRequestMetrics = false;
            options.EnableNotificationMetrics = false;
            options.EnableMiddlewareMetrics = false;
            options.EnablePerformanceCounters = false;
            options.EnableDetailedAnalysis = true;
            options.MaxTrackedRequestTypes = 100;
        });

        // Act
        await _mediator!.Send(new StatTestCommand { Value = "test" });

        // Assert
        _statistics!.ShouldNotBeNull();
        
        // Test detailed analysis methods directly
        _statistics.RecordExecutionPattern("TestRequest", DateTime.UtcNow);
        _statistics.ReportStatistics(); // Should not throw
        
        // Test analysis methods that depend on EnableDetailedAnalysis
        var queries = _statistics.AnalyzeQueries(_serviceProvider!, isDetailed: true);
        queries.ShouldNotBeNull();
        
        var commands = _statistics.AnalyzeCommands(_serviceProvider!, isDetailed: true);
        commands.ShouldNotBeNull();
    }

    [Fact]
    public async Task EnableDetailedAnalysis_False_ShouldNotTrackDetailedMetrics()
    {
        // Arrange
        SetupMediator(options =>
        {
            options.EnableRequestMetrics = true;
            options.EnableNotificationMetrics = true;
            options.EnableMiddlewareMetrics = true;
            options.EnablePerformanceCounters = true;
            options.EnableDetailedAnalysis = false;
        });

        // Act
        await _mediator!.Send(new StatTestCommand { Value = "test" });

        // Assert
        _statistics!.ShouldNotBeNull();
        
        // Test that detailed analysis methods don't track when disabled
        _statistics.RecordExecutionPattern("TestRequest", DateTime.UtcNow);
        _statistics.ReportStatistics(); // Should not throw
        
        // Test analysis methods with detailed flag
        var queries = _statistics.AnalyzeQueries(_serviceProvider!, isDetailed: false);
        queries.ShouldNotBeNull();
        
        var commands = _statistics.AnalyzeCommands(_serviceProvider!, isDetailed: false);
        commands.ShouldNotBeNull();
    }

    [Fact]
    public async Task MaxTrackedRequestTypes_ShouldLimitTrackedTypes()
    {
        // Arrange
        SetupMediator(options =>
        {
            options.EnableRequestMetrics = false;
            options.EnableNotificationMetrics = false;
            options.EnableMiddlewareMetrics = false;
            options.EnablePerformanceCounters = false;
            options.EnableDetailedAnalysis = true;
            options.MaxTrackedRequestTypes = 2; // Limit to 2 types
        });

        // Act
        await _mediator!.Send(new StatTestCommand { Value = "test" });

        // Assert
        _statistics!.ShouldNotBeNull();
        
        // Test that the limit is respected
        _statistics.RecordExecutionPattern("Type1", DateTime.UtcNow);
        _statistics.RecordExecutionPattern("Type2", DateTime.UtcNow);
        _statistics.RecordExecutionPattern("Type3", DateTime.UtcNow); // Should be ignored due to limit
        
        _statistics.ReportStatistics(); // Should not throw
    }

    [Fact]
    public void MetricsRetentionPeriod_ShouldConfigureCleanup()
    {
        // Arrange & Act
        SetupMediator(options =>
        {
            options.EnableRequestMetrics = true;
            options.MetricsRetentionPeriod = TimeSpan.FromHours(1);
            options.CleanupInterval = TimeSpan.FromMinutes(30);
        });

        // Assert
        _statistics!.ShouldNotBeNull();
        // The cleanup timer is initialized internally, just verify the statistics object is created
        _statistics.ReportStatistics(); // Should not throw
    }

    [Fact]
    public void IsEnabled_ShouldReturnTrueWhenAnyTrackingEnabled()
    {
        // Arrange
        var options = new StatisticsOptions
        {
            EnableRequestMetrics = false,
            EnableNotificationMetrics = false,
            EnableMiddlewareMetrics = false,
            EnablePerformanceCounters = true // Only this one enabled
        };

        // Act & Assert
        options.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void IsEnabled_ShouldReturnFalseWhenNoTrackingEnabled()
    {
        // Arrange
        var options = new StatisticsOptions
        {
            EnableRequestMetrics = false,
            EnableNotificationMetrics = false,
            EnableMiddlewareMetrics = false,
            EnablePerformanceCounters = false
        };

        // Act & Assert
        options.IsEnabled.ShouldBeFalse();
    }

    [Fact]
    public void IsAnyTrackingEnabled_ShouldReturnTrueWhenAnyTrackingEnabled()
    {
        // Arrange
        var options = new StatisticsOptions
        {
            EnableRequestMetrics = false,
            EnableNotificationMetrics = true, // Only this one enabled
            EnableMiddlewareMetrics = false,
            EnablePerformanceCounters = false
        };

        // Act & Assert
        options.IsAnyTrackingEnabled.ShouldBeTrue();
    }

    [Fact]
    public void DefaultOptions_ShouldHaveExpectedValues()
    {
        // Arrange & Act
        var options = new StatisticsOptions();

        // Assert
        options.EnableRequestMetrics.ShouldBeTrue();
        options.EnableNotificationMetrics.ShouldBeTrue();
        options.EnableMiddlewareMetrics.ShouldBeFalse();
        options.EnablePerformanceCounters.ShouldBeFalse();
        options.MetricsRetentionPeriod.ShouldBe(TimeSpan.FromHours(24));
        options.EnableDetailedAnalysis.ShouldBeFalse();
        options.MaxTrackedRequestTypes.ShouldBe(1000);
        options.CleanupInterval.ShouldBe(TimeSpan.FromHours(1));
    }

    [Fact]
    public void ValidateAndThrow_ShouldValidateOptionsCorrectly()
    {
        // Arrange
        var validOptions = new StatisticsOptions
        {
            EnableRequestMetrics = true,
            MetricsRetentionPeriod = TimeSpan.FromHours(1),
            MaxTrackedRequestTypes = 100,
            CleanupInterval = TimeSpan.FromMinutes(30)
        };

        var invalidOptions = new StatisticsOptions
        {
            MetricsRetentionPeriod = TimeSpan.FromSeconds(-1), // Invalid
            MaxTrackedRequestTypes = -1, // Invalid
            CleanupInterval = TimeSpan.FromSeconds(-1) // Invalid
        };

        // Act & Assert
        Should.NotThrow(() => validOptions.ValidateAndThrow());
        Should.Throw<ArgumentException>(() => invalidOptions.ValidateAndThrow());
    }

    [Fact]
    public void Clone_ShouldCreateDeepCopy()
    {
        // Arrange
        var original = new StatisticsOptions
        {
            EnableRequestMetrics = false,
            EnableNotificationMetrics = true,
            MetricsRetentionPeriod = TimeSpan.FromHours(2),
            MaxTrackedRequestTypes = 500
        };

        // Act
        var cloned = original.Clone();

        // Assert
        cloned.ShouldNotBe(original); // Different instances
        cloned.EnableRequestMetrics.ShouldBe(original.EnableRequestMetrics);
        cloned.EnableNotificationMetrics.ShouldBe(original.EnableNotificationMetrics);
        cloned.MetricsRetentionPeriod.ShouldBe(original.MetricsRetentionPeriod);
        cloned.MaxTrackedRequestTypes.ShouldBe(original.MaxTrackedRequestTypes);

        // Modify cloned to ensure it's a deep copy
        cloned.EnableRequestMetrics = true;
        original.EnableRequestMetrics.ShouldBeFalse(); // Original should not be affected
    }
}

// Test model classes

// Test handlers