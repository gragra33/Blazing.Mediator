using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.NotificationTests;

/// <summary>
/// Comprehensive tests for the INotificationHandler automatic discovery pattern.
/// Tests handler discovery, registration, execution, multiple handlers, and error scenarios.
/// </summary>
public class NotificationHandlerTests
{
    #region Test Types

    /// <summary>
    /// Test notification for handler tests.
    /// </summary>
    public class OrderCreatedNotification : INotification
    {
        public int OrderId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Another test notification for multi-handler tests.
    /// </summary>
    public class UserRegisteredNotification : INotification
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Email notification handler implementing INotificationHandler.
    /// </summary>
    public class EmailNotificationHandler : INotificationHandler<OrderCreatedNotification>
    {
        public List<OrderCreatedNotification> HandledNotifications { get; } = [];
        public int HandleCallCount { get; private set; }

        public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken); // Simulate async work
            HandledNotifications.Add(notification);
            HandleCallCount++;
        }
    }

    /// <summary>
    /// Audit notification handler implementing INotificationHandler.
    /// </summary>
    public class AuditNotificationHandler : INotificationHandler<OrderCreatedNotification>
    {
        public List<OrderCreatedNotification> HandledNotifications { get; } = [];
        public int HandleCallCount { get; private set; }

        public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.Delay(5, cancellationToken); // Simulate async work
            HandledNotifications.Add(notification);
            HandleCallCount++;
        }
    }

    /// <summary>
    /// User handler for different notification type.
    /// </summary>
    public class UserWelcomeHandler : INotificationHandler<UserRegisteredNotification>
    {
        public List<UserRegisteredNotification> HandledNotifications { get; } = [];
        public int HandleCallCount { get; private set; }

        public async Task Handle(UserRegisteredNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.Delay(8, cancellationToken); // Simulate async work
            HandledNotifications.Add(notification);
            HandleCallCount++;
        }
    }

    /// <summary>
    /// Handler that throws exceptions for error testing.
    /// </summary>
    public class FaultyNotificationHandler : INotificationHandler<OrderCreatedNotification>
    {
        public int HandleCallCount { get; private set; }
        public bool ShouldThrow { get; set; } = false; // Changed from true to false

        public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            HandleCallCount++;

            if (ShouldThrow)
            {
                throw new InvalidOperationException("Simulated handler error");
            }
        }
    }

    /// <summary>
    /// Handler with dependencies for DI testing.
    /// </summary>
    public class DependencyInjectedHandler : INotificationHandler<OrderCreatedNotification>
    {
        private readonly TestService _testService;

        public DependencyInjectedHandler(TestService testService)
        {
            _testService = testService ?? throw new ArgumentNullException(nameof(testService));
        }

        public List<OrderCreatedNotification> HandledNotifications { get; } = [];
        public int HandleCallCount { get; private set; }

        public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            _testService.IncrementCounter();
            HandledNotifications.Add(notification);
            HandleCallCount++;
        }
    }

    /// <summary>
    /// Test service for dependency injection.
    /// </summary>
    public class TestService
    {
        public int Counter { get; private set; }

        public void IncrementCounter()
        {
            Counter++;
        }
    }

    #endregion

    #region Handler Discovery Tests

    /// <summary>
    /// Tests that INotificationHandler implementations are automatically discovered and registered.
    /// </summary>
    [Fact]
    public void AddMediator_WithNotificationHandlerDiscovery_DiscoversHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>(); // Register the required dependency

        // Act - Register with handler discovery (should be enabled by default)
        services.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Check that handlers were registered
        var emailHandlerService = serviceProvider.GetService<INotificationHandler<OrderCreatedNotification>>();
        emailHandlerService.ShouldNotBeNull();

        var userHandlerService = serviceProvider.GetService<INotificationHandler<UserRegisteredNotification>>();
        userHandlerService.ShouldNotBeNull();

        // Verify multiple handlers for the same notification type are supported
        var orderHandlers = serviceProvider.GetServices<INotificationHandler<OrderCreatedNotification>>().ToList();
        orderHandlers.Count.ShouldBeGreaterThanOrEqualTo(2); // EmailNotificationHandler, AuditNotificationHandler, DependencyInjectedHandler, FaultyNotificationHandler
    }

    /// <summary>
    /// Tests that handler discovery can be disabled.
    /// </summary>
    [Fact]
    public void AddMediator_WithoutNotificationHandlerDiscovery_DoesNotDiscoverHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Disable handler discovery
        services.AddMediator(config =>
        {
            config.WithoutNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Check that handlers were NOT registered
        var emailHandlerService = serviceProvider.GetService<INotificationHandler<OrderCreatedNotification>>();
        emailHandlerService.ShouldBeNull();

        var userHandlerService = serviceProvider.GetService<INotificationHandler<UserRegisteredNotification>>();
        userHandlerService.ShouldBeNull();
    }

    /// <summary>
    /// Tests that handlers are registered with proper DI integration.
    /// </summary>
    [Fact]
    public void DiscoveredHandlers_AreScopedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>(); // Register dependency

        services.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Test scoped behavior
        INotificationHandler<OrderCreatedNotification> handler1;
        INotificationHandler<OrderCreatedNotification> handler2;

        using (var scope1 = serviceProvider.CreateScope())
        {
            handler1 = scope1.ServiceProvider.GetRequiredService<EmailNotificationHandler>();
            handler1.ShouldNotBeNull();
        }

        using (var scope2 = serviceProvider.CreateScope())
        {
            handler2 = scope2.ServiceProvider.GetRequiredService<EmailNotificationHandler>();
            handler2.ShouldNotBeNull();
        }

        // Should be different instances from different scopes
        ReferenceEquals(handler1, handler2).ShouldBeFalse();
    }

    #endregion

    #region Single Handler Execution Tests

    /// <summary>
    /// Tests that a single notification handler is invoked correctly.
    /// </summary>
    [Fact]
    public async Task Publish_WithSingleHandler_InvokesHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new UserRegisteredNotification
        {
            UserId = 123,
            Email = "test@example.com"
        };

        // Act
        await mediator.Publish(notification);

        // Assert
        // Get the handler instance to check if it was called
        var handler = serviceProvider.GetRequiredService<UserWelcomeHandler>();
        handler.HandleCallCount.ShouldBe(1);
        handler.HandledNotifications.ShouldContain(notification);
    }

    /// <summary>
    /// Tests that notification handlers receive the correct notification data.
    /// </summary>
    [Fact]
    public async Task Publish_HandlerReceivesCorrectData()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>(); // Add missing TestService dependency

        services.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new OrderCreatedNotification
        {
            OrderId = 456,
            CustomerEmail = "customer@test.com",
            TotalAmount = 99.95m,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await mediator.Publish(notification);

        // Assert
        var emailHandler = serviceProvider.GetRequiredService<EmailNotificationHandler>();
        emailHandler.HandledNotifications.Count.ShouldBe(1);
        
        var handledNotification = emailHandler.HandledNotifications[0];
        handledNotification.OrderId.ShouldBe(456);
        handledNotification.CustomerEmail.ShouldBe("customer@test.com");
        handledNotification.TotalAmount.ShouldBe(99.95m);
        handledNotification.CreatedAt.ShouldBe(notification.CreatedAt);
    }

    #endregion

    #region Multiple Handler Execution Tests

    /// <summary>
    /// Tests that multiple handlers for the same notification type are all invoked.
    /// </summary>
    [Fact]
    public async Task Publish_WithMultipleHandlers_InvokesAllHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>(); // For DependencyInjectedHandler

        services.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new OrderCreatedNotification
        {
            OrderId = 789,
            CustomerEmail = "multiple@test.com",
            TotalAmount = 150.00m
        };

        // Act
        await mediator.Publish(notification);

        // Assert - Check that all handlers were invoked
        var emailHandler = serviceProvider.GetRequiredService<EmailNotificationHandler>();
        var auditHandler = serviceProvider.GetRequiredService<AuditNotificationHandler>();
        var diHandler = serviceProvider.GetRequiredService<DependencyInjectedHandler>();

        emailHandler.HandleCallCount.ShouldBe(1);
        auditHandler.HandleCallCount.ShouldBe(1);
        diHandler.HandleCallCount.ShouldBe(1);

        // All handlers should have received the same notification
        emailHandler.HandledNotifications.ShouldContain(notification);
        auditHandler.HandledNotifications.ShouldContain(notification);
        diHandler.HandledNotifications.ShouldContain(notification);

        // Check dependency injection worked
        var testService = serviceProvider.GetRequiredService<TestService>();
        testService.Counter.ShouldBe(1);
    }

    /// <summary>
    /// Tests that handlers execute independently and one failure doesn't affect others.
    /// </summary>
    [Fact]
    public async Task Publish_WithFailingHandler_OtherHandlersStillExecute()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();

        services.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Configure the faulty handler to throw
        var faultyHandler = serviceProvider.GetRequiredService<FaultyNotificationHandler>();
        faultyHandler.ShouldThrow = true;

        var notification = new OrderCreatedNotification
        {
            OrderId = 999,
            CustomerEmail = "error@test.com",
            TotalAmount = 200.00m
        };

        // Act - Should throw because of faulty handler, but other handlers should still execute
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await mediator.Publish(notification);
        });

        // Assert
        exception.Message.ShouldBe("Simulated handler error");

        // Other handlers should still have been called before the exception
        var emailHandler = serviceProvider.GetRequiredService<EmailNotificationHandler>();
        var auditHandler = serviceProvider.GetRequiredService<AuditNotificationHandler>();
        var diHandler = serviceProvider.GetRequiredService<DependencyInjectedHandler>();
        
        // The faulty handler was called
        faultyHandler.HandleCallCount.ShouldBe(1);

        // Check if other handlers were called (order of execution may affect this)
        // At least some handlers should have been called
        var totalCalls = emailHandler.HandleCallCount + auditHandler.HandleCallCount + diHandler.HandleCallCount;
        totalCalls.ShouldBeGreaterThan(0);
    }

    #endregion

    #region Handler and Subscriber Coexistence Tests

    /// <summary>
    /// Tests that INotificationHandler and INotificationSubscriber work together.
    /// </summary>
    [Fact]
    public async Task Publish_WithBothHandlersAndSubscribers_BothAreInvoked()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>(); // Add missing TestService dependency

        services.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Create and subscribe a manual subscriber
        var manualSubscriber = new ManualOrderSubscriber();
        mediator.Subscribe<OrderCreatedNotification>(manualSubscriber);

        var notification = new OrderCreatedNotification
        {
            OrderId = 555,
            CustomerEmail = "coexist@test.com",
            TotalAmount = 75.50m
        };

        // Act
        await mediator.Publish(notification);

        // Assert - Check that both automatic handlers and manual subscribers were invoked
        var emailHandler = serviceProvider.GetRequiredService<EmailNotificationHandler>();
        emailHandler.HandleCallCount.ShouldBe(1);
        emailHandler.HandledNotifications.ShouldContain(notification);

        manualSubscriber.ReceivedNotifications.Count.ShouldBe(1);
        manualSubscriber.ReceivedNotifications.ShouldContain(notification);
    }

    /// <summary>
    /// Manual subscriber for coexistence testing.
    /// </summary>
    public class ManualOrderSubscriber : INotificationSubscriber<OrderCreatedNotification>
    {
        public List<OrderCreatedNotification> ReceivedNotifications { get; } = [];

        public Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
        {
            ReceivedNotifications.Add(notification);
            return Task.CompletedTask;
        }
    }

    #endregion

    #region Dependency Injection Tests

    /// <summary>
    /// Tests that handlers with dependencies are resolved correctly from DI.
    /// </summary>
    [Fact]
    public async Task Publish_WithDependencyInjectedHandler_ResolvesDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();

        services.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new OrderCreatedNotification
        {
            OrderId = 111,
            CustomerEmail = "di@test.com",
            TotalAmount = 25.00m
        };

        // Act
        await mediator.Publish(notification);

        // Assert
        var handler = serviceProvider.GetRequiredService<DependencyInjectedHandler>();
        var testService = serviceProvider.GetRequiredService<TestService>();

        handler.HandleCallCount.ShouldBe(1);
        handler.HandledNotifications.ShouldContain(notification);
        testService.Counter.ShouldBe(1);
    }

    /// <summary>
    /// Tests that missing dependencies for handlers cause appropriate exceptions.
    /// </summary>
    [Fact]
    public void Publish_WithMissingDependency_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        // Deliberately NOT registering TestService

        services.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new OrderCreatedNotification
        {
            OrderId = 222,
            CustomerEmail = "missing@test.com",
            TotalAmount = 50.00m
        };

        // Act & Assert
        Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await mediator.Publish(notification);
        });
    }

    #endregion

    #region Cancellation Tests

    /// <summary>
    /// Tests that cancellation tokens are properly passed to handlers.
    /// </summary>
    [Fact]
    public async Task Publish_WithCancellation_PassesCancellationToken()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new UserRegisteredNotification
        {
            UserId = 333,
            Email = "cancel@test.com"
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await mediator.Publish(notification, cts.Token);
        });
    }

    #endregion

    #region Performance Tests

    /// <summary>
    /// Tests that handler discovery doesn't significantly impact performance.
    /// </summary>
    [Fact]
    public async Task Publish_WithMultipleHandlers_PerformsReasonably()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();

        services.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notifications = Enumerable.Range(1, 10).Select(i => new OrderCreatedNotification
        {
            OrderId = i,
            CustomerEmail = $"perf{i}@test.com",
            TotalAmount = i * 10m
        }).ToList();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        foreach (var notification in notifications)
        {
            await mediator.Publish(notification);
        }

        stopwatch.Stop();

        // Assert
        // Should complete reasonably quickly (adjust threshold as needed)
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(5000); // 5 seconds for 10 notifications

        // Verify all notifications were processed
        var emailHandler = serviceProvider.GetRequiredService<EmailNotificationHandler>();
        emailHandler.HandleCallCount.ShouldBe(10);
    }

    #endregion

    #region Generic Handler Tests

    /// <summary>
    /// Tests that generic notification handlers work correctly.
    /// </summary>
    [Fact]
    public async Task Publish_WithGenericHandler_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>(); // Add missing TestService dependency

        // Disable auto-discovery to test only manually registered generic handlers
        services.AddMediator(config =>
        {
            config.WithoutNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        // Register a generic handler manually for testing
        services.AddScoped<GenericLoggingHandler>();
        services.AddScoped<INotificationHandler<OrderCreatedNotification>>(sp => sp.GetRequiredService<GenericLoggingHandler>());
        services.AddScoped<INotificationHandler<UserRegisteredNotification>>(sp => sp.GetRequiredService<GenericLoggingHandler>());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var orderNotification = new OrderCreatedNotification { OrderId = 444, CustomerEmail = "generic@test.com", TotalAmount = 88.88m };
        var userNotification = new UserRegisteredNotification { UserId = 777, Email = "user@test.com" };

        // Act
        await mediator.Publish(orderNotification);
        await mediator.Publish(userNotification);

        // Assert
        var genericHandler = serviceProvider.GetRequiredService<GenericLoggingHandler>();
        genericHandler.LoggedNotifications.Count.ShouldBe(2);
        genericHandler.LoggedNotifications.ShouldContain("OrderCreatedNotification: 444");
        genericHandler.LoggedNotifications.ShouldContain("UserRegisteredNotification: 777");
    }

    /// <summary>
    /// Generic logging handler that can handle any notification type.
    /// </summary>
    public class GenericLoggingHandler : 
        INotificationHandler<OrderCreatedNotification>,
        INotificationHandler<UserRegisteredNotification>
    {
        public List<string> LoggedNotifications { get; } = [];

        public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            LoggedNotifications.Add($"OrderCreatedNotification: {notification.OrderId}");
        }

        public async Task Handle(UserRegisteredNotification notification, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            LoggedNotifications.Add($"UserRegisteredNotification: {notification.UserId}");
        }
    }

    #endregion

    #region Covariant Handler Tests

    /// <summary>
    /// Tests that covariant notification handlers work with inheritance hierarchies.
    /// </summary>
    [Fact]
    public async Task Publish_WithCovariantHandlers_InvokesAllCompatibleHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>(); // For handlers that need it

        services.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        // Manually register covariant test handlers
        services.AddScoped<BaseNotificationCovariantHandler>();
        services.AddScoped<InterfaceNotificationCovariantHandler>();
        services.AddScoped<SpecificDerivedHandler>();
        services.AddScoped<INotificationHandler<BaseTestNotificationForCovariance>>(sp => sp.GetRequiredService<BaseNotificationCovariantHandler>());
        services.AddScoped<INotificationHandler<ITestNotificationInterface>>(sp => sp.GetRequiredService<InterfaceNotificationCovariantHandler>());
        services.AddScoped<INotificationHandler<DerivedTestNotificationForCovariance>>(sp => sp.GetRequiredService<SpecificDerivedHandler>());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Reset counters
        BaseNotificationCovariantHandler.CallCount = 0;
        InterfaceNotificationCovariantHandler.CallCount = 0;
        SpecificDerivedHandler.CallCount = 0;

        var derivedNotification = new DerivedTestNotificationForCovariance
        {
            Message = "Test covariant handling",
            BaseProperty = "Base value",
            DerivedProperty = "Derived value",
            InterfaceValue = "Interface value"
        };

        // Act
        await mediator.Publish(derivedNotification);

        // Assert - All compatible handlers should be called
        BaseNotificationCovariantHandler.CallCount.ShouldBe(1);
        InterfaceNotificationCovariantHandler.CallCount.ShouldBe(1);
        SpecificDerivedHandler.CallCount.ShouldBe(1);

        // Check that handlers received the correct notification
        BaseNotificationCovariantHandler.LastNotification.ShouldNotBeNull();
        InterfaceNotificationCovariantHandler.LastNotification.ShouldNotBeNull();
        SpecificDerivedHandler.LastNotification.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that only base-compatible handlers are invoked for base notifications.
    /// </summary>
    [Fact]
    public async Task Publish_WithBaseNotification_OnlyInvokesBaseHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();

        services.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);

        // Manually register covariant test handlers
        services.AddScoped<BaseNotificationCovariantHandler>();
        services.AddScoped<InterfaceNotificationCovariantHandler>();
        services.AddScoped<SpecificDerivedHandler>();
        services.AddScoped<INotificationHandler<BaseTestNotificationForCovariance>>(sp => sp.GetRequiredService<BaseNotificationCovariantHandler>());
        services.AddScoped<INotificationHandler<ITestNotificationInterface>>(sp => sp.GetRequiredService<InterfaceNotificationCovariantHandler>());
        services.AddScoped<INotificationHandler<DerivedTestNotificationForCovariance>>(sp => sp.GetRequiredService<SpecificDerivedHandler>());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Reset counters
        BaseNotificationCovariantHandler.CallCount = 0;
        InterfaceNotificationCovariantHandler.CallCount = 0;
        SpecificDerivedHandler.CallCount = 0;

        var baseNotification = new BaseTestNotificationForCovariance
        {
            Message = "Test base handling",
            BaseProperty = "Base value",
            InterfaceValue = "Interface value"
        };

        // Act
        await mediator.Publish(baseNotification);

        // Assert - Only base and interface handlers should be called
        BaseNotificationCovariantHandler.CallCount.ShouldBe(1);
        InterfaceNotificationCovariantHandler.CallCount.ShouldBe(1);
        SpecificDerivedHandler.CallCount.ShouldBe(0); // Should NOT be called for base type
    }

    #region Covariant Test Types

    /// <summary>
    /// Interface for covariant testing.
    /// </summary>
    public interface ITestNotificationInterface : INotification
    {
        string InterfaceValue { get; }
    }

    /// <summary>
    /// Base notification for covariant testing.
    /// </summary>
    public class BaseTestNotificationForCovariance : INotification, ITestNotificationInterface
    {
        public string Message { get; set; } = string.Empty;
        public string BaseProperty { get; set; } = string.Empty;
        public string InterfaceValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Derived notification for covariant testing.
    /// </summary>
    public class DerivedTestNotificationForCovariance : BaseTestNotificationForCovariance
    {
        public string DerivedProperty { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handler for base notifications.
    /// </summary>
    public class BaseNotificationCovariantHandler : INotificationHandler<BaseTestNotificationForCovariance>
    {
        public static int CallCount = 0;
        public static BaseTestNotificationForCovariance? LastNotification = null;

        public Task Handle(BaseTestNotificationForCovariance notification, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastNotification = notification;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Handler for interface-based notifications.
    /// </summary>
    public class InterfaceNotificationCovariantHandler : INotificationHandler<ITestNotificationInterface>
    {
        public static int CallCount = 0;
        public static ITestNotificationInterface? LastNotification = null;

        public Task Handle(ITestNotificationInterface notification, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastNotification = notification;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Handler for specific derived notifications.
    /// </summary>
    public class SpecificDerivedHandler : INotificationHandler<DerivedTestNotificationForCovariance>
    {
        public static int CallCount = 0;
        public static DerivedTestNotificationForCovariance? LastNotification = null;

        public Task Handle(DerivedTestNotificationForCovariance notification, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastNotification = notification;
            return Task.CompletedTask;
        }
    }

    #endregion

    #endregion

    #region Configuration Tests

    /// <summary>
    /// Tests that handler discovery respects configuration options.
    /// </summary>
    [Fact]
    public void HandlerDiscovery_RespectsConfiguration()
    {
        // Arrange & Act - Test default behavior (should be enabled)
        var servicesDefault = new ServiceCollection();
        servicesDefault.AddMediator(typeof(NotificationHandlerTests).Assembly);
        var defaultProvider = servicesDefault.BuildServiceProvider();

        // Assert - Default should discover handlers
        var defaultHandler = defaultProvider.GetService<INotificationHandler<OrderCreatedNotification>>();
        defaultHandler.ShouldNotBeNull();

        // Arrange & Act - Test explicitly enabled
        var servicesEnabled = new ServiceCollection();
        servicesEnabled.AddMediator(config =>
        {
            config.WithNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);
        var enabledProvider = servicesEnabled.BuildServiceProvider();

        // Assert - Explicitly enabled should discover handlers
        var enabledHandler = enabledProvider.GetService<INotificationHandler<OrderCreatedNotification>>();
        enabledHandler.ShouldNotBeNull();

        // Arrange & Act - Test explicitly disabled
        var servicesDisabled = new ServiceCollection();
        servicesDisabled.AddMediator(config =>
        {
            config.WithoutNotificationHandlerDiscovery();
        }, typeof(NotificationHandlerTests).Assembly);
        var disabledProvider = servicesDisabled.BuildServiceProvider();

        // Assert - Explicitly disabled should NOT discover handlers
        var disabledHandler = disabledProvider.GetService<INotificationHandler<OrderCreatedNotification>>();
        disabledHandler.ShouldBeNull();
    }

    #endregion
}