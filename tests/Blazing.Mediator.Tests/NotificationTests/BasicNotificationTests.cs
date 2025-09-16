using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.NotificationTests;

/// <summary>
/// Contains basic tests for notification publishing and subscription in the mediator.
/// </summary>
public class BasicNotificationTests
{
    /// <summary>
    /// Test notification type for verifying notification delivery.
    /// </summary>
    public class TestNotification : INotification
    {
        /// <summary>
        /// Gets or sets the message for the notification.
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the value for the notification.
        /// </summary>
        public int Value { get; set; }
    }

    /// <summary>
    /// Subscriber for specific notification type used in tests.
    /// </summary>
    public class TestSubscriber : INotificationSubscriber<TestNotification>
    {
        /// <summary>
        /// Gets the list of received notifications.
        /// </summary>
        public List<TestNotification> ReceivedNotifications { get; } = new();

        /// <summary>
        /// Handles the notification when published.
        /// </summary>
        public Task OnNotification(TestNotification notification, CancellationToken cancellationToken = default)
        {
            ReceivedNotifications.Add(notification);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Subscriber for any notification type used in tests.
    /// </summary>
    public class GenericSubscriber : INotificationSubscriber
    {
        /// <summary>
        /// Gets the list of received notifications.
        /// </summary>
        public List<INotification> ReceivedNotifications { get; } = new();

        /// <summary>
        /// Handles the notification when published.
        /// </summary>
        public Task OnNotification(INotification notification, CancellationToken cancellationToken = default)
        {
            ReceivedNotifications.Add(notification);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Publish_Should_Notify_Specific_Subscribers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var subscriber = new TestSubscriber();
        mediator.Subscribe(subscriber);

        var notification = new TestNotification { Message = "Hello", Value = 42 };

        // Act
        await mediator.Publish(notification);

        // Assert
        subscriber.ReceivedNotifications.Count.ShouldBe(1);
        subscriber.ReceivedNotifications[0].Message.ShouldBe("Hello");
        subscriber.ReceivedNotifications[0].Value.ShouldBe(42);
    }

    [Fact]
    public async Task Publish_Should_Notify_Generic_Subscribers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var subscriber = new GenericSubscriber();
        mediator.Subscribe(subscriber);

        var notification = new TestNotification { Message = "Hello", Value = 42 };

        // Act
        await mediator.Publish(notification);

        // Assert
        subscriber.ReceivedNotifications.Count.ShouldBe(1);
        subscriber.ReceivedNotifications[0].ShouldBeOfType<TestNotification>();
        var typedNotification = (TestNotification)subscriber.ReceivedNotifications[0];
        typedNotification.Message.ShouldBe("Hello");
        typedNotification.Value.ShouldBe(42);
    }

    [Fact]
    public async Task Publish_Should_Notify_Both_Specific_And_Generic_Subscribers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var specificSubscriber = new TestSubscriber();
        var genericSubscriber = new GenericSubscriber();

        mediator.Subscribe(specificSubscriber);
        mediator.Subscribe(genericSubscriber);

        var notification = new TestNotification { Message = "Hello", Value = 42 };

        // Act
        await mediator.Publish(notification);

        // Assert
        specificSubscriber.ReceivedNotifications.Count.ShouldBe(1);
        genericSubscriber.ReceivedNotifications.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Unsubscribe_Should_Remove_Specific_Subscriber()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var subscriber = new TestSubscriber();
        mediator.Subscribe(subscriber);

        // Act
        mediator.Unsubscribe(subscriber);

        // Assert - publish notification and verify subscriber doesn't receive it
        var notification = new TestNotification { Message = "Test", Value = 123 };
        await mediator.Publish(notification);

        subscriber.ReceivedNotifications.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Unsubscribe_Should_Remove_Generic_Subscriber()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var subscriber = new GenericSubscriber();
        mediator.Subscribe(subscriber);

        // Act
        mediator.Unsubscribe(subscriber);

        // Assert - publish notification and verify subscriber doesn't receive it
        var notification = new TestNotification { Message = "Test", Value = 123 };
        await mediator.Publish(notification);

        subscriber.ReceivedNotifications.Count.ShouldBe(0);
    }
}
