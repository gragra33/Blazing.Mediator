using Blazing.Mediator.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Blazing.Mediator.Tests.OpenTelemetry;

/// <summary>
/// Tests for OpenTelemetry instrumentation of the Mediator Publish operations.
/// Validates notification metrics collection, subscriber tracing, and telemetry configuration.
/// Uses Collection attribute to ensure tests run sequentially to avoid static state conflicts.
/// </summary>
[Collection("OpenTelemetry")]
public class MediatorTelemetryPublishTests : IDisposable
{
    private ServiceProvider _serviceProvider;
    private IMediator _mediator;
    private List<Activity>? _recordedActivities;
    private PublishTestNotificationSubscriber _testSubscriber;
    private PublishTestNotificationSubscriberWithException _exceptionSubscriber;
    private ActivityListener? _activityListener;

    public MediatorTelemetryPublishTests()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging();

        // Add mediator with telemetry enabled
        services.AddMediatorTelemetry();
        services.AddMediator(typeof(MediatorTelemetryPublishTests).Assembly);

        // Register test notification subscribers
        _testSubscriber = new PublishTestNotificationSubscriber();
        _exceptionSubscriber = new PublishTestNotificationSubscriberWithException();

        services.AddSingleton<INotificationSubscriber<PublishTestNotification>>(_testSubscriber);
        services.AddSingleton<INotificationSubscriber<PublishTestNotification>>(_exceptionSubscriber);
        services.AddSingleton<INotificationSubscriber<PublishTestNotificationWithSensitiveData>, TestSensitiveNotificationSubscriber>();

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();

        // Initialize collections for capturing telemetry
        _recordedActivities = new List<Activity>();

        // Set up activity listener to capture activities
        _activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Blazing.Mediator",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = _ => { /* Activity started */ },
            ActivityStopped = activity => _recordedActivities?.Add(activity)
        };
        ActivitySource.AddActivityListener(_activityListener);

        // Subscribe notification handlers
        _mediator.Subscribe(_testSubscriber);
        _mediator.Subscribe(_exceptionSubscriber);
    }

    public void Dispose()
    {
        _activityListener?.Dispose();
        _serviceProvider.Dispose();
    }

    [Fact]
    public async Task Publish_Notification_Success_GeneratesCorrectTelemetry()
    {
        // Arrange
        var notification = new PublishTestNotification { Message = "Test message" };
        _testSubscriber.Reset();
        _exceptionSubscriber.Reset();
        _recordedActivities?.Clear();

        // Act & Assert - expecting one subscriber to throw exception
        await Should.ThrowAsync<InvalidOperationException>(() => _mediator.Publish(notification));

        // Verify subscribers were called
        _testSubscriber.CallCount.ShouldBe(1, "Successful subscriber should be called");
        _exceptionSubscriber.CallCount.ShouldBe(1, "Exception subscriber should be called");

        // Verify activity was created
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("PublishTestNotification"));
        activity.ShouldNotBeNull("Activity should be created for notification");
        activity.Status.ShouldBe(ActivityStatusCode.Error, "Activity should have error status due to subscriber exception");

        // Verify activity tags
        activity.GetTagItem("notification_name").ShouldBe("PublishTestNotification");
        activity.GetTagItem("subscriber_count").ShouldBe(2);

        // Verify duration is recorded
        var durationTag = activity.GetTagItem("duration_ms");
        durationTag.ShouldNotBeNull();
        Convert.ToDouble(durationTag).ShouldBeGreaterThan(0);

        // Verify subscriber events in activity
        var subscriberEvents = activity.Events.Where(e => e.Name.StartsWith("subscriber:")).ToList();
        subscriberEvents.Count.ShouldBe(2, "Should have events for both subscribers");

        // Check successful subscriber event
        var successEvent = subscriberEvents.FirstOrDefault(e =>
            e.Tags.Any(t => t.Key == "subscriber_type" && t.Value?.ToString()?.Contains("PublishTestNotificationSubscriber") == true &&
                           !t.Value.ToString()!.Contains("Exception")));
        if (successEvent.Name != null)
        {
            successEvent.Tags.Any(t => t.Key == "success" && t.Value?.ToString() == "True").ShouldBeTrue();
        }

        // Check exception subscriber event
        var exceptionEvent = subscriberEvents.FirstOrDefault(e =>
            e.Tags.Any(t => t.Key == "subscriber_type" && t.Value?.ToString()?.Contains("Exception") == true));
        if (exceptionEvent.Name != null)
        {
            exceptionEvent.Tags.Any(t => t.Key == "success" && t.Value?.ToString() == "False").ShouldBeTrue();
            exceptionEvent.Tags.Any(t => t.Key == "exception_type" && t.Value?.ToString() == "InvalidOperationException").ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Publish_WithSuccessfulSubscribers_GeneratesSuccessTelemetry()
    {
        // Arrange
        var notification = new PublishTestNotification { Message = "Success test" };
        _testSubscriber.Reset();
        _recordedActivities?.Clear();

        // Unsubscribe the exception subscriber for this test
        _mediator.Unsubscribe(_exceptionSubscriber);

        // Act
        await _mediator.Publish(notification);

        // Assert
        _testSubscriber.CallCount.ShouldBe(1, "Subscriber should be called");

        // Verify activity was created successfully
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("PublishTestNotification"));
        activity.ShouldNotBeNull("Activity should be created for notification");
        activity.Status.ShouldBe(ActivityStatusCode.Ok, "Activity should complete successfully");

        // Verify subscriber count
        activity.GetTagItem("subscriber_count").ShouldBe(1);

        // Verify subscriber event
        var subscriberEvents = activity.Events.Where(e => e.Name.StartsWith("subscriber:")).ToList();
        subscriberEvents.Count.ShouldBe(1, "Should have event for one subscriber");

        var successEvent = subscriberEvents.First();
        successEvent.Tags.Any(t => t.Key == "success" && t.Value?.ToString() == "True").ShouldBeTrue();
    }

    [Fact]
    public async Task Publish_DisabledTelemetry_DoesNotGenerateTelemetry()
    {
        // Arrange
        var originalTelemetryState = Mediator.TelemetryEnabled;
        Mediator.TelemetryEnabled = false;
        var notification = new PublishTestNotification { Message = "No telemetry test" };
        _recordedActivities?.Clear();

        try
        {
            // Act - Unsubscribe exception subscriber to avoid exceptions during test
            _mediator.Unsubscribe(_exceptionSubscriber);
            await _mediator.Publish(notification);

            // Assert
            var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("PublishTestNotification"));
            activity.ShouldBeNull("No activity should be created when telemetry is disabled");
        }
        finally
        {
            // Restore original state
            Mediator.TelemetryEnabled = originalTelemetryState;
        }
    }

    [Fact]
    public async Task Publish_SensitiveData_IsSanitized()
    {
        // Arrange
        var notification = new PublishTestNotificationWithSensitiveData
        {
            Password = "secret123",
            Token = "abc123",
            Message = "Test with sensitive data"
        };
        _recordedActivities?.Clear();

        // Act
        await _mediator.Publish(notification);

        // Assert
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("PublishTestNotificationWithSensitiveData"));
        activity.ShouldNotBeNull();

        // Verify sensitive data is sanitized
        var notificationName = activity.GetTagItem("notification_name")?.ToString();
        if (notificationName != null)
        {
            notificationName.ShouldContain("***");
            notificationName.ShouldNotContain("Password");
        }
        else
        {
            throw new InvalidOperationException("Notification name should not be null");
        }
    }

    [Fact]
    public async Task Subscribe_And_Unsubscribe_WorksCorrectly()
    {
        // Arrange
        var testSubscriber = new PublishTestNotificationSubscriber();
        _recordedActivities?.Clear();

        // Act - Subscribe
        _mediator.Subscribe(testSubscriber);

        // Verify subscription by publishing notification
        var notification = new PublishTestNotification { Message = "Subscribe test" };

        // Unsubscribe exception subscriber to avoid test complications
        _mediator.Unsubscribe(_exceptionSubscriber);

        // Now we should have 2 successful subscribers
        await _mediator.Publish(notification);

        // Verify activity shows increased subscriber count
        var activity = _recordedActivities?.LastOrDefault(a => a.DisplayName.Contains("PublishTestNotification"));
        activity.ShouldNotBeNull();
        Convert.ToInt32(activity.GetTagItem("subscriber_count")).ShouldBe(2);

        // Act - Unsubscribe
        _mediator.Unsubscribe(testSubscriber);

        // Clear activities and test again
        _recordedActivities?.Clear();
        await _mediator.Publish(new PublishTestNotification { Message = "Unsubscribe test" });

        // Verify subscriber count decreased
        activity = _recordedActivities?.LastOrDefault(a => a.DisplayName.Contains("PublishTestNotification"));
        activity.ShouldNotBeNull();
        Convert.ToInt32(activity.GetTagItem("subscriber_count")).ShouldBe(1);
    }

    [Fact]
    public async Task Publish_MultipleNotifications_TracksEachCorrectly()
    {
        // Arrange
        _recordedActivities?.Clear();
        _mediator.Unsubscribe(_exceptionSubscriber); // Avoid exceptions

        var notification1 = new PublishTestNotification { Message = "First notification" };
        var notification2 = new PublishTestNotification { Message = "Second notification" };

        // Act
        await _mediator.Publish(notification1);
        await _mediator.Publish(notification2);

        // Assert
        var activities = _recordedActivities?.Where(a => a.DisplayName.Contains("PublishTestNotification")).ToList();
        activities.ShouldNotBeNull();
        activities.Count.ShouldBe(2, "Should have activities for both notifications");

        foreach (var activity in activities)
        {
            activity.Status.ShouldBe(ActivityStatusCode.Ok, "All activities should complete successfully");
            activity.GetTagItem("notification_name").ShouldBe("PublishTestNotification");
            activity.GetTagItem("subscriber_count").ShouldBe(1);
        }
    }

    #region Test Classes

    public class PublishTestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    public class PublishTestNotificationWithSensitiveData : INotification
    {
        public string Password { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class PublishTestNotificationSubscriber : INotificationSubscriber<PublishTestNotification>
    {
        public int CallCount { get; private set; }

        public async Task OnNotification(PublishTestNotification notification, CancellationToken cancellationToken = default)
        {
            CallCount++;
            await Task.Delay(5, cancellationToken); // Simulate work
        }

        public void Reset() => CallCount = 0;
    }

    public class PublishTestNotificationSubscriberWithException : INotificationSubscriber<PublishTestNotification>
    {
        public int CallCount { get; private set; }

        public Task OnNotification(PublishTestNotification notification, CancellationToken cancellationToken = default)
        {
            CallCount++;
            throw new InvalidOperationException("Test subscriber exception");
        }

        public void Reset() => CallCount = 0;
    }

    public class TestSensitiveNotificationSubscriber : INotificationSubscriber<PublishTestNotificationWithSensitiveData>
    {
        public async Task OnNotification(PublishTestNotificationWithSensitiveData notification, CancellationToken cancellationToken = default)
        {
            await Task.Delay(5, cancellationToken); // Simulate work
        }
    }

    #endregion
}
