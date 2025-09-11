using Blazing.Mediator;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Reflection;

namespace Blazing.Mediator.Tests.NotificationTests;

public class BasicNotificationTests
{
    // Test notification
    public class TestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    // Test subscriber
    public class TestSubscriber : INotificationSubscriber<TestNotification>
    {
        public List<TestNotification> ReceivedNotifications { get; } = new();

        public Task OnNotification(TestNotification notification, CancellationToken cancellationToken = default)
        {
            ReceivedNotifications.Add(notification);
            return Task.CompletedTask;
        }
    }

    // Generic subscriber
    public class GenericSubscriber : INotificationSubscriber
    {
        public List<INotification> ReceivedNotifications { get; } = new();

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
