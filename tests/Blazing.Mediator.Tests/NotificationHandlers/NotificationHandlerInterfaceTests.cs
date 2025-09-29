using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests.NotificationHandlers;

/// <summary>
/// Basic tests for the new INotificationHandler interface.
/// </summary>
public class NotificationHandlerInterfaceTests
{
    /// <summary>
    /// Test notification for handler interface tests.
    /// </summary>
    public record TestNotification(string Message) : INotification;

    /// <summary>
    /// Test handler implementation for interface tests.
    /// </summary>
    public class TestNotificationHandler : INotificationHandler<TestNotification>
    {
        public bool WasCalled { get; private set; }
        public TestNotification? ReceivedNotification { get; private set; }

        public Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedNotification = notification;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Multiple test handlers for testing multiple handler scenarios.
    /// </summary>
    public class TestNotificationHandler1 : INotificationHandler<TestNotification>
    {
        public bool WasCalled { get; private set; }

        public Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    public class TestNotificationHandler2 : INotificationHandler<TestNotification>
    {
        public bool WasCalled { get; private set; }

        public Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Exception throwing handler for error handling tests.
    /// </summary>
    public class ExceptionThrowingHandler : INotificationHandler<TestNotification>
    {
        public Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Test exception");
        }
    }

    [Fact]
    public void INotificationHandler_Interface_HasCorrectSignature()
    {
        // Arrange & Act
        var handlerType = typeof(INotificationHandler<TestNotification>);
        var handleMethod = handlerType.GetMethod("Handle");

        // Assert
        Assert.NotNull(handleMethod);
        Assert.Equal(typeof(Task), handleMethod.ReturnType);
        
        var parameters = handleMethod.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(TestNotification), parameters[0].ParameterType);
        Assert.Equal(typeof(CancellationToken), parameters[1].ParameterType);
        Assert.True(parameters[1].HasDefaultValue);
    }

    [Fact]
    public void INotificationHandler_Interface_IsContravariant()
    {
        // Arrange
        var baseInterface = typeof(INotificationHandler<>);
        var genericParameter = baseInterface.GetGenericArguments()[0];

        // Assert
        Assert.True(genericParameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant));
    }

    [Fact]
    public void INotificationHandler_Interface_RequiresINotificationConstraint()
    {
        // Arrange
        var baseInterface = typeof(INotificationHandler<>);
        var genericParameter = baseInterface.GetGenericArguments()[0];
        var constraints = genericParameter.GetGenericParameterConstraints();

        // Assert
        Assert.Contains(typeof(INotification), constraints);
    }

    [Fact]
    public async Task TestNotificationHandler_CanHandleNotification()
    {
        // Arrange
        var handler = new TestNotificationHandler();
        var notification = new TestNotification("Test message");

        // Act
        await handler.Handle(notification);

        // Assert
        Assert.True(handler.WasCalled);
        Assert.Equal(notification, handler.ReceivedNotification);
        Assert.Equal("Test message", handler.ReceivedNotification!.Message);
    }

    [Fact]
    public async Task TestNotificationHandler_SupportsCancellation()
    {
        // Arrange
        var handler = new TestNotificationHandler();
        var notification = new TestNotification("Test message");
        using var cts = new CancellationTokenSource();

        // Act
        await handler.Handle(notification, cts.Token);

        // Assert
        Assert.True(handler.WasCalled);
    }

    [Fact]
    public async Task MultipleHandlers_CanBeCreatedForSameNotification()
    {
        // Arrange
        var handler1 = new TestNotificationHandler1();
        var handler2 = new TestNotificationHandler2();
        var notification = new TestNotification("Test message");

        // Act
        await handler1.Handle(notification);
        await handler2.Handle(notification);

        // Assert
        Assert.True(handler1.WasCalled);
        Assert.True(handler2.WasCalled);
    }

    [Fact]
    public void ExceptionThrowingHandler_CanThrowExceptions()
    {
        // Arrange
        var handler = new ExceptionThrowingHandler();
        var notification = new TestNotification("Test message");

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(notification));
        Assert.Equal("Test exception", exception.Result.Message);
    }

    [Fact]
    public void INotificationHandler_CanBeRegisteredInDI()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<INotificationHandler<TestNotification>, TestNotificationHandler>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<INotificationHandler<TestNotification>>();

        // Assert
        Assert.NotNull(handler);
        Assert.IsType<TestNotificationHandler>(handler);
    }

    [Fact]
    public void INotificationHandler_MultipleImplementations_CanBeRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<INotificationHandler<TestNotification>, TestNotificationHandler1>();
        services.AddScoped<INotificationHandler<TestNotification>, TestNotificationHandler2>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var handlers = serviceProvider.GetServices<INotificationHandler<TestNotification>>().ToList();

        // Assert
        Assert.Equal(2, handlers.Count);
        Assert.Contains(handlers, h => h.GetType() == typeof(TestNotificationHandler1));
        Assert.Contains(handlers, h => h.GetType() == typeof(TestNotificationHandler2));
    }
}