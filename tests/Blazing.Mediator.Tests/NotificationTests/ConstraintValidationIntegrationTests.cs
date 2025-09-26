using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests.NotificationTests;

/// <summary>
/// Integration tests for Phase 2 Steps 2.6, 2.7, 2.8: Complete constraint validation functionality
/// Tests the full constraint validation system through the public API without relying on internal classes.
/// </summary>
public class ConstraintValidationIntegrationTests
{
    private readonly Assembly _testAssembly = typeof(ConstraintValidationIntegrationTests).Assembly;

    [Fact]
    public void ConstraintValidation_BasicConfiguration_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should not throw
        services.AddMediator(config =>
        {
            config.WithConstraintValidation();
            config.AddNotificationMiddleware<SimpleConstraintTestMiddleware>();
        }, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Should work without errors
        var notification = new SimpleTestNotification { Id = 1 };
        Should.NotThrow(async () => await mediator.Publish(notification));
    }

    [Fact]
    public void ConstraintValidation_StrictMode_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should not throw during registration
        Should.NotThrow(() =>
        {
            services.AddMediator(config =>
            {
                config.WithStrictConstraintValidation();
                config.AddNotificationMiddleware<SimpleConstraintTestMiddleware>();
            }, _testAssembly);
        });

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Should work for compatible notifications
        var notification = new SimpleTestNotification { Id = 1 };
        Should.NotThrow(async () => await mediator.Publish(notification));
    }

    [Fact]
    public void ConstraintValidation_LenientMode_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        services.AddMediator(config =>
        {
            config.WithConstraintValidation(options =>
            {
                options.Strictness = ConstraintValidationOptions.ValidationStrictness.Lenient;
                options.EnableDetailedLogging = true;
            });
            config.AddNotificationMiddleware<SimpleConstraintTestMiddleware>();
        }, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();

        // Should be able to analyze constraints
        var analysis = inspector.AnalyzeConstraints<SimpleTestNotification>(serviceProvider);
        analysis.ShouldNotBeNull();
        analysis.NotificationType.ShouldBe(typeof(SimpleTestNotification));
    }

    [Fact]
    public void ConstraintValidation_DisabledMode_SkipsValidation()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.WithoutConstraintValidation();
            config.AddNotificationMiddleware<SimpleConstraintTestMiddleware>();
        }, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Assert - Should work even with potentially incompatible middleware
        var notification = new SimpleTestNotification { Id = 1 };
        Should.NotThrow(async () => await mediator.Publish(notification));
    }

    [Fact]
    public void ConstraintValidation_CustomOptions_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.WithConstraintValidation(options =>
            {
                options.Strictness = ConstraintValidationOptions.ValidationStrictness.Lenient;
                options.EnableConstraintCaching = true;
                options.MaxConstraintInheritanceDepth = 5;
                options.ValidateCircularDependencies = true;
                options.EnableDetailedLogging = false;
            });
            config.AddNotificationMiddleware<SimpleConstraintTestMiddleware>();
        }, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<MediatorConfiguration>();

        // Assert
        config.ConstraintValidationOptions.ShouldNotBeNull();
        config.ConstraintValidationOptions.Strictness.ShouldBe(ConstraintValidationOptions.ValidationStrictness.Lenient);
        config.ConstraintValidationOptions.EnableConstraintCaching.ShouldBeTrue();
        config.ConstraintValidationOptions.MaxConstraintInheritanceDepth.ShouldBe(5);
    }

    [Fact]
    public void ConstraintValidation_PipelineAnalysis_ProvidesUsefulInfo()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddMediator(config =>
        {
            config.WithConstraintValidation();
            config.AddNotificationMiddleware<SimpleConstraintTestMiddleware>();
            config.AddNotificationMiddleware<SpecialConstraintTestMiddleware>();
        }, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();

        // Act
        var pipelineAnalysis = inspector.AnalyzePipelineConstraints(serviceProvider);
        var constraintUsageMap = inspector.GetConstraintUsageMap(serviceProvider);
        var executionPath = inspector.AnalyzeExecutionPath(new SimpleTestNotification { Id = 1 }, serviceProvider);

        // Assert
        pipelineAnalysis.ShouldNotBeNull();
        pipelineAnalysis.TotalMiddlewareCount.ShouldBe(2);

        constraintUsageMap.ShouldNotBeNull();
        
        executionPath.ShouldNotBeNull();
        executionPath.NotificationType.ShouldBe(typeof(SimpleTestNotification));
        executionPath.TotalMiddlewareCount.ShouldBe(2);
    }

    [Theory]
    [InlineData(ConstraintValidationOptions.ValidationStrictness.Strict)]
    [InlineData(ConstraintValidationOptions.ValidationStrictness.Lenient)]
    [InlineData(ConstraintValidationOptions.ValidationStrictness.Disabled)]
    public void ConstraintValidation_AllStrictnessLevels_WorkWithRealScenarios(ConstraintValidationOptions.ValidationStrictness strictness)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.WithConstraintValidation(options =>
            {
                options.Strictness = strictness;
            });
            config.AddNotificationMiddleware<SimpleConstraintTestMiddleware>();
        }, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Assert - Should work regardless of strictness level
        var notification = new SimpleTestNotification { Id = 1 };
        Should.NotThrow(async () => await mediator.Publish(notification));
    }

    [Fact]
    public void ConstraintValidation_InvalidConfiguration_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
        {
            services.AddMediator(config =>
            {
                config.WithConstraintValidation(options =>
                {
                    options.MaxConstraintInheritanceDepth = 0; // Invalid
                });
            }, _testAssembly);
        });
    }

    [Fact]
    public void ConstraintValidation_OptionsCloning_WorksCorrectly()
    {
        // Arrange
        var original = ConstraintValidationOptions.CreateStrict();
        original.MaxConstraintInheritanceDepth = 15;

        // Act
        var clone = original.Clone();

        // Assert
        clone.ShouldNotBe(original);
        clone.Strictness.ShouldBe(original.Strictness);
        clone.MaxConstraintInheritanceDepth.ShouldBe(15);

        // Modify original to prove independence
        original.MaxConstraintInheritanceDepth = 20;
        clone.MaxConstraintInheritanceDepth.ShouldBe(15); // Should remain unchanged
    }
}

// Test interfaces and implementations for integration testing
public interface ISimpleTestNotification : INotification
{
    int Id { get; }
}

public interface ISpecialTestNotification : ISimpleTestNotification
{
    string SpecialData { get; }
}

public class SimpleTestNotification : ISimpleTestNotification
{
    public int Id { get; set; }
}

public class SpecialTestNotification : ISpecialTestNotification
{
    public int Id { get; set; }
    public string SpecialData { get; set; } = "";
}

public class SimpleConstraintTestMiddleware : INotificationMiddleware<ISimpleTestNotification>
{
    public int Order => 10;

    public Task InvokeAsync(ISimpleTestNotification notification, NotificationDelegate<ISimpleTestNotification> next, CancellationToken cancellationToken)
    {
        return next(notification, cancellationToken);
    }

    Task INotificationMiddleware.InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        // For testing purposes, just delegate to the base implementation to avoid throwing
        return next(notification, cancellationToken);
    }
}

public class SpecialConstraintTestMiddleware : INotificationMiddleware<ISpecialTestNotification>
{
    public int Order => 20;

    public Task InvokeAsync(ISpecialTestNotification notification, NotificationDelegate<ISpecialTestNotification> next, CancellationToken cancellationToken)
    {
        return next(notification, cancellationToken);
    }

    Task INotificationMiddleware.InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        // For testing purposes, just delegate to the base implementation to avoid throwing
        return next(notification, cancellationToken);
    }
}