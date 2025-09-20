using Blazing.Mediator.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Blazing.Mediator.Tests.OpenTelemetry;

/// <summary>
/// Tests for OpenTelemetry instrumentation of streaming requests.
/// Validates metrics collection and tracing for IStreamRequest operations.
/// Uses Collection attribute to ensure tests run sequentially to avoid static state conflicts.
/// </summary>
[Collection("OpenTelemetry")]
public class MediatorTelemetryStreamingTests : IDisposable
{
    private ServiceProvider _serviceProvider;
    private IMediator _mediator;
    private List<Activity>? _recordedActivities;
    private ActivityListener? _activityListener;

    public MediatorTelemetryStreamingTests()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging();

        // Add mediator with telemetry enabled
        services.AddMediatorTelemetry();
        services.AddMediator(typeof(MediatorTelemetryStreamingTests).Assembly);

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
    }

    public void Dispose()
    {
        _activityListener?.Dispose();
        _serviceProvider.Dispose();
    }

    [Fact]
    public async Task SendStream_Success_GeneratesCorrectTelemetry()
    {
        // Arrange
        var request = new StreamingTestStreamRequest { Count = 3 };
        _recordedActivities?.Clear();

        // Act
        var results = new List<string>();
        await foreach (var item in _mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(3);
        results[0].ShouldBe("Item 0");
        results[1].ShouldBe("Item 1");
        results[2].ShouldBe("Item 2");

        // Verify activity was created
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("StreamingTestStreamRequest"));
        activity.ShouldNotBeNull("Activity should be created for stream request");
        activity.Status.ShouldBe(ActivityStatusCode.Ok, "Activity should complete successfully");

        // Verify activity tags
        activity.GetTagItem("request_name").ShouldBe("StreamingTestStreamRequest");
        activity.GetTagItem("request_type").ShouldBe("stream");
        activity.GetTagItem("response_type").ShouldBe("String");
        activity.GetTagItem("handler.type").ShouldBe("TestStreamHandler");

        // Verify stream-specific tags
        activity.GetTagItem("stream.items_count").ShouldNotBeNull();
        Convert.ToInt32(activity.GetTagItem("stream.items_count")).ShouldBe(3);

        // Verify duration is recorded
        var durationTag = activity.GetTagItem("duration_ms");
        durationTag.ShouldNotBeNull();
        Convert.ToDouble(durationTag).ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task SendStream_WithException_GeneratesErrorTelemetry()
    {
        // Arrange
        var request = new StreamingTestStreamRequestWithException { Count = 2 };
        _recordedActivities?.Clear();

        // Act & Assert
        var results = new List<string>();
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await foreach (var item in _mediator.SendStream(request))
            {
                results.Add(item);
            }
        });

        exception.Message.ShouldBe("Test stream exception");

        // Should have received some items before the exception
        results.Count.ShouldBe(1, "Should receive one item before exception");
        results[0].ShouldBe("Item 0");

        // Verify activity was created with error status
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("StreamingTestStreamRequestWithException"));
        activity.ShouldNotBeNull("Activity should be created for failed stream");
        activity.Status.ShouldBe(ActivityStatusCode.Error, "Activity should have error status");

        // Verify exception details in activity
        activity.GetTagItem("exception.type").ShouldBe("InvalidOperationException");
        var exceptionMessage = activity.GetTagItem("exception.message")?.ToString();
        exceptionMessage.ShouldNotBeNull();
        exceptionMessage.ShouldContain("Test stream exception");

        // Verify partial stream data
        activity.GetTagItem("stream.items_count").ShouldNotBeNull();
        Convert.ToInt32(activity.GetTagItem("stream.items_count")).ShouldBe(1, "Should track partial items count");
    }

    [Fact]
    public async Task SendStream_DisabledTelemetry_DoesNotGenerateTelemetry()
    {
        // Arrange
        var originalTelemetryState = Mediator.TelemetryEnabled;
        Mediator.TelemetryEnabled = false;
        var request = new StreamingTestStreamRequest { Count = 2 };
        _recordedActivities?.Clear();

        try
        {
            // Act
            var results = new List<string>();
            await foreach (var item in _mediator.SendStream(request))
            {
                results.Add(item);
            }

            // Assert
            results.Count.ShouldBe(2);
            var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("StreamingTestStreamRequest"));
            activity.ShouldBeNull("No activity should be created when telemetry is disabled");
        }
        finally
        {
            // Restore original state
            Mediator.TelemetryEnabled = originalTelemetryState;
        }
    }

    [Fact]
    public async Task SendStream_EmptyStream_TracksCorrectly()
    {
        // Arrange
        var request = new StreamingTestStreamRequest { Count = 0 };
        _recordedActivities?.Clear();

        // Act
        var results = new List<string>();
        await foreach (var item in _mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(0);

        // Verify activity was created for empty stream
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("StreamingTestStreamRequest"));
        activity.ShouldNotBeNull("Activity should be created even for empty stream");
        activity.Status.ShouldBe(ActivityStatusCode.Ok, "Activity should complete successfully");

        // Verify empty stream metrics
        activity.GetTagItem("stream.items_count").ShouldNotBeNull();
        Convert.ToInt32(activity.GetTagItem("stream.items_count")).ShouldBe(0);
    }

    [Fact]
    public async Task SendStream_LargeStream_TracksPerformanceCorrectly()
    {
        // Arrange
        var request = new StreamingTestStreamRequest { Count = 100 };
        _recordedActivities?.Clear();

        // Act
        var results = new List<string>();
        await foreach (var item in _mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(100);

        // Verify activity tracks large stream correctly
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("StreamingTestStreamRequest"));
        activity.ShouldNotBeNull();
        activity.Status.ShouldBe(ActivityStatusCode.Ok);

        // Verify large stream metrics
        Convert.ToInt32(activity.GetTagItem("stream.items_count")).ShouldBe(100);

        // Verify duration is reasonable for large stream
        var durationTag = activity.GetTagItem("duration_ms");
        durationTag.ShouldNotBeNull();
        var duration = Convert.ToDouble(durationTag);
        duration.ShouldBeGreaterThan(0);
        duration.ShouldBeLessThan(10000); // Should complete within 10 seconds
    }

    [Fact]
    public async Task SendStream_CancellationRequested_TracksCorrectly()
    {
        // Arrange
        var request = new StreamingTestStreamRequest { Count = 10 };
        _recordedActivities?.Clear();
        using var cts = new CancellationTokenSource();

        // Act
        var results = new List<string>();
        var cancelled = false;

        try
        {
            await foreach (var item in _mediator.SendStream(request, cts.Token))
            {
                results.Add(item);
                if (results.Count >= 3)
                {
                    cts.Cancel(); // Cancel after receiving 3 items
                }
            }
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
        }

        // Assert
        cancelled.ShouldBeTrue("Stream should be cancelled");
        results.Count.ShouldBe(3, "Should receive items before cancellation");

        // Verify activity tracks cancellation
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("StreamingTestStreamRequest"));
        activity.ShouldNotBeNull();

        // Activity status might be Error due to cancellation
        (activity.Status == ActivityStatusCode.Error || activity.Status == ActivityStatusCode.Unset).ShouldBeTrue();

        // Should track partial results
        var itemsCount = Convert.ToInt32(activity.GetTagItem("stream.items_count"));
        itemsCount.ShouldBeGreaterThanOrEqualTo(3);
    }

    #region Test Classes

    public class StreamingTestStreamRequest : IStreamRequest<string>
    {
        public int Count { get; set; }
    }

    public class TestStreamHandler : IStreamRequestHandler<StreamingTestStreamRequest, string>
    {
        public async IAsyncEnumerable<string> Handle(StreamingTestStreamRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(1, cancellationToken); // Minimal delay to simulate work
                yield return $"Item {i}";
            }
        }
    }

    public class StreamingTestStreamRequestWithException : IStreamRequest<string>
    {
        public int Count { get; set; }
    }

    public class TestStreamHandlerWithException : IStreamRequestHandler<StreamingTestStreamRequestWithException, string>
    {
        public async IAsyncEnumerable<string> Handle(StreamingTestStreamRequestWithException request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(1, cancellationToken);

                if (i == 1) // Throw exception after first item
                {
                    throw new InvalidOperationException("Test stream exception");
                }

                yield return $"Item {i}";
            }
        }
    }

    #endregion
}
