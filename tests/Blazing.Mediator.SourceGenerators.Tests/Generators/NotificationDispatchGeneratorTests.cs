using Blazing.Mediator.SourceGenerators.Generators;
using Blazing.Mediator.SourceGenerators.Tests.Helpers;

namespace Blazing.Mediator.SourceGenerators.Tests.Generators;

/// <summary>
/// Tests for <see cref="IncrementalMediatorGenerator"/> notification handler discovery and dispatch code generation.
/// </summary>
public class NotificationDispatchGeneratorTests
{
    [Fact]
    public void Generator_WithSimpleNotification_GeneratesDispatchCode()
    {
        // Arrange
        var inputSource = GeneratorTestHelper.CreateTestSource(
            GeneratorTestHelper.CommonSources.SimpleNotification);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert - Verify generated code is present and contains expected content
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(generatedSources, "Mediator");
        generatedCode.ShouldNotBeNull();
        generatedCode.ShouldContain("UserCreatedNotification");
        generatedCode.ShouldContain("UserCreatedEmailHandler");
        generatedCode.ShouldContain("UserCreatedCacheHandler");
        generatedCode.ShouldContain("NotificationHandlerWrapper");
        // IsNotificationHandled<T>() override must be emitted — it is the sentinel used by
        // Mediator.Notification.Publish<T> to bypass the try/catch NotImplementedException overhead.
        generatedCode.ShouldContain("IsNotificationHandled<TNotification>");
    }

    [Fact]
    public void Generator_WithCovariantNotifications_GeneratesCorrectDispatch()
    {
        // Arrange - Notification hierarchy for covariant handling
        var inputSource = GeneratorTestHelper.CreateTestSource("""
            // Base notification
            public record EntityEvent : INotification
            {
                public Guid EntityId { get; init; }
            }

            // Derived notification
            public record UserEvent : EntityEvent
            {
                public string UserName { get; init; } = string.Empty;
            }

            // Most derived notification
            public record UserCreatedEvent : UserEvent
            {
                public DateTime CreatedAt { get; init; }
            }

            // Handler for base type (handles all derived)
            public class EntityEventHandler : INotificationHandler<EntityEvent>
            {
                public ValueTask Handle(EntityEvent notification, CancellationToken ct)
                    => Task.CompletedTask;
            }

            // Handler for User events (handles UserEvent and UserCreatedEvent)
            public class UserEventHandler : INotificationHandler<UserEvent>
            {
                public ValueTask Handle(UserEvent notification, CancellationToken ct)
                    => Task.CompletedTask;
            }

            // Handler for specific event
            public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
            {
                public ValueTask Handle(UserCreatedEvent notification, CancellationToken ct)
                    => Task.CompletedTask;
            }
            """);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert - Handlers appear in the generated notification wrapper code
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(generatedSources, "Mediator");
        generatedCode.ShouldNotBeNull();
        
        // Publishing UserCreatedEvent should invoke all 3 handlers
        generatedCode.ShouldContain("EntityEventHandler");
        generatedCode.ShouldContain("UserEventHandler");
        generatedCode.ShouldContain("UserCreatedEventHandler");
    }

    [Fact]
    public void Generator_WithMultipleHandlersPerNotification_GeneratesParallelExecution()
    {
        // Arrange - Multiple handlers for same notification
        var inputSource = GeneratorTestHelper.CreateTestSource("""
            public record OrderPlacedNotification(int OrderId, decimal Total) : INotification;

            public class OrderPlacedEmailHandler : INotificationHandler<OrderPlacedNotification>
            {
                public ValueTask Handle(OrderPlacedNotification notification, CancellationToken ct)
                    => Task.CompletedTask;
            }

            public class OrderPlacedInventoryHandler : INotificationHandler<OrderPlacedNotification>
            {
                public ValueTask Handle(OrderPlacedNotification notification, CancellationToken ct)
                    => Task.CompletedTask;
            }

            public class OrderPlacedAnalyticsHandler : INotificationHandler<OrderPlacedNotification>
            {
                public ValueTask Handle(OrderPlacedNotification notification, CancellationToken ct)
                    => Task.CompletedTask;
            }

            public class OrderPlacedAuditHandler : INotificationHandler<OrderPlacedNotification>
            {
                public ValueTask Handle(OrderPlacedNotification notification, CancellationToken ct)
                    => Task.CompletedTask;
            }
            """);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert - Should execute all handlers
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(generatedSources, "Mediator");
        generatedCode.ShouldNotBeNull();
        generatedCode.ShouldContain("OrderPlacedEmailHandler");
        generatedCode.ShouldContain("OrderPlacedInventoryHandler");
        generatedCode.ShouldContain("OrderPlacedAnalyticsHandler");
        generatedCode.ShouldContain("OrderPlacedAuditHandler");
    }

    [Fact]
    public void Generator_WithNoHandlers_HandlesGracefully()
    {
        // Arrange - Notification with no handlers
        var inputSource = GeneratorTestHelper.CreateTestSource("""
            public record UnhandledNotification : INotification;

            // No handlers defined for this notification
            """);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert - Should generate code or handle gracefully (no exception)
        compilation.ShouldNotBeNull();
    }

    [Fact]
    public void Generator_GeneratesSubscriberIntegration()
    {
        // Arrange
        var inputSource = GeneratorTestHelper.CreateTestSource(
            GeneratorTestHelper.CommonSources.SimpleNotification);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert - Notification wrapper and handler types present in generated code
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(generatedSources, "Mediator");
        generatedCode.ShouldNotBeNull();

        // The generated wrapper and dispatch switch reference INotification types
        generatedCode.ShouldContain("INotification");
    }
}
