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

    #endregion
}
