using Blazing.Mediator.Configuration;
using Blazing.Mediator.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Blazing.Mediator.Tests.OpenTelemetry;

/// <summary>
/// Tests for OpenTelemetry instrumentation of the Mediator Send operations.
/// Validates metrics collection, tracing, and telemetry configuration.
/// Uses Collection attribute to ensure tests run sequentially to avoid static state conflicts.
/// </summary>
[Collection("OpenTelemetry")]
public class MediatorTelemetrySendTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly List<Activity>? _recordedActivities;
    private readonly ActivityListener? _activityListener;

    public MediatorTelemetrySendTests()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging();

        // Add mediator with telemetry enabled - use unique assembly to avoid conflicts
        services.AddMediatorTelemetry();
        services.AddMediator(); // Don't scan assemblies to avoid conflicts

        // Register test handlers manually to avoid conflicts
        services.AddScoped<IRequestHandler<SendTestCommand>, SendTestCommandHandler>();
        services.AddScoped<IRequestHandler<SendTestQuery, string>, SendTestQueryHandler>();
        services.AddScoped<IRequestHandler<SendTestCommandWithException>, SendTestCommandWithExceptionHandler>();
        services.AddScoped<IRequestHandler<SendTestQueryWithException, string>, SendTestQueryWithExceptionHandler>();
        services.AddScoped<IRequestHandler<SendTestCommandWithPassword>, SendTestCommandWithPasswordHandler>();

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();

        // Initialize collections for capturing telemetry
        //_ = new List<KeyValuePair<string, object?>>();
        _recordedActivities = [];

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
    public async Task Send_DisabledTelemetry_DoesNotGenerateTelemetry()
    {
        // Arrange - Create a separate mediator with telemetry explicitly disabled
        var services = new ServiceCollection();
        services.AddLogging();

        // Configure mediator with telemetry disabled
        services.AddMediator(new MediatorConfiguration()
            .WithTelemetry(options =>
            {
                options.Enabled = false;
            })
            .AddFromAssembly(typeof(SendTestCommand).Assembly));

        await using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new SendTestCommand { Value = "test" };
        var recordedActivities = new List<Activity>();

        // Set up activity listener specifically for this test
        using var activityListener = new ActivityListener();

        activityListener.ShouldListenTo = source => source.Name == "Blazing.Mediator";
        activityListener.Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData;
        activityListener.ActivityStarted = _ => { /* Activity started */ };
        activityListener.ActivityStopped = activity => recordedActivities.Add(activity);
        ActivitySource.AddActivityListener(activityListener);

        // Act
        await mediator.Send(command);

        // Assert
        var activity = recordedActivities.FirstOrDefault(a => a.DisplayName.Contains("SendTestCommand"));
        activity.ShouldBeNull("No activity should be created when telemetry is disabled");
    }

    [Fact]
    public void TelemetryHealth_WhenEnabled_ReturnsHealthy()
    {
        // Arrange
        Mediator.TelemetryEnabled = true;

        // Act
        var health = TelemetryHealthCheck.CheckHealth();

        // Assert
        health.IsHealthy.ShouldBeTrue("Telemetry health should return true when enabled");
        health.IsEnabled.ShouldBeTrue("Telemetry should be enabled");
        health.CanRecordMetrics.ShouldBeTrue("Should be able to record metrics");
    }

    [Fact]
    public void TelemetryHealth_WhenDisabled_ReturnsUnhealthy()
    {
        // Arrange
        var originalState = Mediator.TelemetryEnabled;
        Mediator.TelemetryEnabled = false;

        try
        {
            // Act
            var health = TelemetryHealthCheck.CheckHealth();

            // Assert
            health.IsHealthy.ShouldBeFalse("Telemetry health should return false when disabled");
            health.IsEnabled.ShouldBeFalse("Telemetry should be disabled");
        }
        finally
        {
            Mediator.TelemetryEnabled = originalState;
        }
    }

    #region Test Classes

    public class SendTestCommand : IRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class SendTestCommandHandler : IRequestHandler<SendTestCommand>
    {
        public async ValueTask Handle(SendTestCommand request, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken); // Simulate work
        }
    }

    public class SendTestQuery : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class SendTestQueryHandler : IRequestHandler<SendTestQuery, string>
    {
        public async ValueTask<string> Handle(SendTestQuery request, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken); // Simulate work
            return $"Handled: {request.Value}";
        }
    }

    public class SendTestCommandWithException : IRequest
    {
    }

    public class SendTestCommandWithExceptionHandler : IRequestHandler<SendTestCommandWithException>
    {
        public ValueTask Handle(SendTestCommandWithException request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Test exception");
        }
    }

    public class SendTestQueryWithException : IRequest<string>
    {
    }

    public class SendTestQueryWithExceptionHandler : IRequestHandler<SendTestQueryWithException, string>
    {
        public ValueTask<string> Handle(SendTestQueryWithException request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Test query exception");
        }
    }

    public class SendTestCommandWithPassword : IRequest
    {
        public string Password { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class SendTestCommandWithPasswordHandler : IRequestHandler<SendTestCommandWithPassword>
    {
        public async ValueTask Handle(SendTestCommandWithPassword request, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken);
        }
    }

    public class SendAlwaysSkipMiddleware : IConditionalMiddleware<SendTestCommand>
    {
        public int Order => 10;
        public bool ShouldExecute(SendTestCommand request) => false;

        public ValueTask HandleAsync(SendTestCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
            => next();
    }

    #endregion

    #region MiddlewareCaptureMode Tag Tests

    [Fact]
    public async Task Send_NoneMode_EmitsNoMiddlewareTags()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(new MediatorConfiguration()
            .WithTelemetry(options =>
            {
                options.MiddlewareCaptureMode = MiddlewareCaptureMode.None;
            })
            .AddFromAssembly(typeof(SendTestCommand).Assembly));

        var recordedActivities = new List<Activity>();
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Blazing.Mediator",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = _ => { },
            ActivityStopped = activity => recordedActivities.Add(activity)
        };
        ActivitySource.AddActivityListener(activityListener);

        await using var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // Act
        await mediator.Send(new SendTestCommand { Value = "test" });

        // Assert
        var activity = recordedActivities.FirstOrDefault(a => a.OperationName == "Mediator.Send:SendTestCommand");
        activity.ShouldNotBeNull("Telemetry activity should be created even in None mode");
        activity.GetTagItem("request_middleware.pipeline").ShouldBeNull("None mode must not emit pipeline tag");
        activity.GetTagItem("request_middleware.executed_pipeline").ShouldBeNull("None mode must not emit executed_pipeline tag");
        activity.GetTagItem("request_middleware.capture_mode").ShouldBeNull("None mode must not emit capture_mode tag");
    }

    [Fact]
    public async Task Send_ApplicableMode_EmitsFourMiddlewareTags()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(new MediatorConfiguration()
            .WithTelemetry(options =>
            {
                options.MiddlewareCaptureMode = MiddlewareCaptureMode.Applicable;
            })
            .AddFromAssembly(typeof(SendTestCommand).Assembly));

        var recordedActivities = new List<Activity>();
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Blazing.Mediator",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = _ => { },
            ActivityStopped = activity => recordedActivities.Add(activity)
        };
        ActivitySource.AddActivityListener(activityListener);

        await using var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // Act
        await mediator.Send(new SendTestCommand { Value = "test" });

        // Assert
        var activity = recordedActivities.FirstOrDefault(a => a.OperationName == "Mediator.Send:SendTestCommand");
        activity.ShouldNotBeNull();
        activity.GetTagItem("request_middleware.pipeline").ShouldNotBeNull("Applicable mode must emit pipeline tag");
        activity.GetTagItem("request_middleware.count").ShouldNotBeNull("Applicable mode must emit count tag");
        activity.GetTagItem("request_middleware.orders").ShouldNotBeNull("Applicable mode must emit orders tag");
        activity.GetTagItem("request_middleware.capture_mode").ShouldBe("applicable");
    }

    [Fact]
    public async Task Send_ExecutedMode_EmitsFiveSummaryTags()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(new MediatorConfiguration()
            .WithTelemetry(options =>
            {
                options.MiddlewareCaptureMode = MiddlewareCaptureMode.Executed;
            })
            .AddFromAssembly(typeof(SendTestCommand).Assembly));

        var recordedActivities = new List<Activity>();
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Blazing.Mediator",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = _ => { },
            ActivityStopped = activity => recordedActivities.Add(activity)
        };
        ActivitySource.AddActivityListener(activityListener);

        await using var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // Act
        await mediator.Send(new SendTestCommand { Value = "test" });

        // Assert
        var activity = recordedActivities.FirstOrDefault(a => a.OperationName == "Mediator.Send:SendTestCommand");
        activity.ShouldNotBeNull();
        activity.GetTagItem("request_middleware.executed_pipeline").ShouldNotBeNull("Executed mode must emit executed_pipeline tag");
        activity.GetTagItem("request_middleware.executed_count").ShouldNotBeNull("Executed mode must emit executed_count tag");
        activity.GetTagItem("request_middleware.skipped_pipeline").ShouldNotBeNull("Executed mode must emit skipped_pipeline tag");
        activity.GetTagItem("request_middleware.skipped_count").ShouldNotBeNull("Executed mode must emit skipped_count tag");
        activity.GetTagItem("request_middleware.capture_mode").ShouldBe("executed");
    }

    [Fact]
    public async Task Send_ExecutedMode_ConditionalMiddleware_CountedAsSkipped()
    {
        // Arrange — SendAlwaysSkipMiddleware.ShouldExecute() always returns false
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(new MediatorConfiguration()
            .WithTelemetry(options =>
            {
                options.MiddlewareCaptureMode = MiddlewareCaptureMode.Executed;
            })
            .AddFromAssembly(typeof(SendTestCommand).Assembly));

        var recordedActivities = new List<Activity>();
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Blazing.Mediator",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = _ => { },
            ActivityStopped = activity => recordedActivities.Add(activity)
        };
        ActivitySource.AddActivityListener(activityListener);

        await using var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // Act
        await mediator.Send(new SendTestCommand { Value = "test" });

        // Assert
        var activity = recordedActivities.FirstOrDefault(a => a.OperationName == "Mediator.Send:SendTestCommand");
        activity.ShouldNotBeNull();
        var skippedCount = Convert.ToInt32(activity.GetTagItem("request_middleware.skipped_count"));
        skippedCount.ShouldBeGreaterThanOrEqualTo(1,
            "SendAlwaysSkipMiddleware always returns false from ShouldExecute and must appear in the skipped count");
    }

    #endregion
}
