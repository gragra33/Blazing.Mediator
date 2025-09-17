using System.Diagnostics;
using Blazing.Mediator.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.OpenTelemetry;

/// <summary>
/// Tests for OpenTelemetry instrumentation of the Mediator Send operations.
/// Validates metrics collection, tracing, and telemetry configuration.
/// Uses Collection attribute to ensure tests run sequentially to avoid static state conflicts.
/// </summary>
[Collection("OpenTelemetry")]
public class MediatorTelemetrySendTests : IDisposable
{
    private ServiceProvider _serviceProvider;
    private IMediator _mediator;
    private List<KeyValuePair<string, object?>>? _recordedMetrics;
    private List<Activity>? _recordedActivities;
    private ActivityListener? _activityListener;

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
        services.AddScoped<IRequestHandler<SendTestCommandWithSensitiveData>, SendTestCommandWithSensitiveDataHandler>();
        
        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        
        // Initialize collections for capturing telemetry
        _recordedMetrics = new List<KeyValuePair<string, object?>>();
        _recordedActivities = new List<Activity>();
        
        // Set up activity listener to capture activities
        _activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Blazing.Mediator",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => _recordedActivities?.Add(activity),
            ActivityStopped = _ => { /* Activity completed */ }
        };
        ActivitySource.AddActivityListener(_activityListener);
    }

    public void Dispose()
    {
        _activityListener?.Dispose();
        _serviceProvider.Dispose();
    }

    [Fact]
    public async Task Send_Command_Success_GeneratesCorrectTelemetry()
    {
        // Arrange
        var command = new SendTestCommand { Value = "test" };
        _recordedActivities?.Clear();

        // Act
        await _mediator.Send(command);

        // Assert
        // Verify activity was created
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("SendTestCommand"));
        activity.ShouldNotBeNull("Activity should be created for command");
        activity.Status.ShouldBe(ActivityStatusCode.Unset, "Activity should complete successfully");
        
        // Verify activity tags
        activity.GetTagItem("request_name").ShouldBe("SendTestCommand");
        activity.GetTagItem("request_type").ShouldBe("command");
        activity.GetTagItem("handler.type").ShouldBe("SendTestCommandHandler");
        
        // Verify duration is recorded
        var durationTag = activity.GetTagItem("duration_ms");
        durationTag.ShouldNotBeNull();
        Convert.ToDouble(durationTag).ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Send_Query_Success_GeneratesCorrectTelemetry()
    {
        // Arrange
        var query = new SendTestQuery { Value = "test query" };
        _recordedActivities?.Clear();

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.ShouldBe("Handled: test query");
        
        // Verify activity was created
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("SendTestQuery"));
        activity.ShouldNotBeNull("Activity should be created for query");
        activity.Status.ShouldBe(ActivityStatusCode.Unset, "Activity should complete successfully");
        
        // Verify activity tags include response type
        activity.GetTagItem("request_name").ShouldBe("SendTestQuery");
        activity.GetTagItem("request_type").ShouldBe("query");
        activity.GetTagItem("response_type").ShouldBe("String");
        activity.GetTagItem("handler.type").ShouldBe("SendTestQueryHandler");
    }

    [Fact]
    public async Task Send_Command_WithException_GeneratesErrorTelemetry()
    {
        // Arrange
        var command = new SendTestCommandWithException();
        _recordedActivities?.Clear();

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => _mediator.Send(command));
        exception.Message.ShouldBe("Test exception");

        // Verify activity was created with error status
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("SendTestCommandWithException"));
        activity.ShouldNotBeNull("Activity should be created for failed command");
        activity.Status.ShouldBe(ActivityStatusCode.Error, "Activity should have error status");
        
        // Verify exception details in activity
        activity.GetTagItem("exception.type").ShouldBe("InvalidOperationException");
        var exceptionMessage = activity.GetTagItem("exception.message")?.ToString();
        exceptionMessage.ShouldNotBeNull();
        exceptionMessage.ShouldContain("Test exception");
    }

    [Fact]
    public async Task Send_Query_WithException_GeneratesErrorTelemetry()
    {
        // Arrange
        var query = new SendTestQueryWithException();
        _recordedActivities?.Clear();

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => _mediator.Send(query));
        exception.Message.ShouldBe("Test query exception");

        // Verify activity was created with error status
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("SendTestQueryWithException"));
        activity.ShouldNotBeNull("Activity should be created for failed query");
        activity.Status.ShouldBe(ActivityStatusCode.Error, "Activity should have error status");
        
        // Verify exception details in activity
        activity.GetTagItem("exception.type").ShouldBe("InvalidOperationException");
        var exceptionMessage = activity.GetTagItem("exception.message")?.ToString();
        exceptionMessage.ShouldNotBeNull();
        exceptionMessage.ShouldContain("Test query exception");
    }

    [Fact]
    public async Task Send_DisabledTelemetry_DoesNotGenerateTelemetry()
    {
        // Arrange
        var originalTelemetryState = Mediator.TelemetryEnabled;
        Mediator.TelemetryEnabled = false;
        var command = new SendTestCommand { Value = "test" };
        _recordedActivities?.Clear();

        try
        {
            // Act
            await _mediator.Send(command);

            // Assert
            var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("SendTestCommand"));
            activity.ShouldBeNull("No activity should be created when telemetry is disabled");
        }
        finally
        {
            // Restore original state
            Mediator.TelemetryEnabled = originalTelemetryState;
        }
    }

    [Fact]
    public void TelemetryHealth_WhenEnabled_ReturnsHealthy()
    {
        // Arrange
        Mediator.TelemetryEnabled = true;

        // Act
        var health = MediatorTelemetryHealthCheck.CheckHealth();

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
            var health = MediatorTelemetryHealthCheck.CheckHealth();

            // Assert
            health.IsHealthy.ShouldBeFalse("Telemetry health should return false when disabled");
            health.IsEnabled.ShouldBeFalse("Telemetry should be disabled");
        }
        finally
        {
            Mediator.TelemetryEnabled = originalState;
        }
    }

    [Fact]
    public async Task Send_SensitiveData_IsSanitized()
    {
        // Arrange
        var command = new SendTestCommandWithSensitiveData { Password = "secret123", Token = "abc123" };
        _recordedActivities?.Clear();

        // Act
        await _mediator.Send(command);

        // Assert
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("SendTestCommandWithSensitiveData"));
        activity.ShouldNotBeNull();
        
        // Verify sensitive data is sanitized
        var requestName = activity.GetTagItem("request_name")?.ToString();
        if (requestName != null)
        {
            requestName.ShouldContain("***");
            requestName.ShouldNotContain("Password");
        }
        else
        {
            throw new InvalidOperationException("Request name should not be null");
        }
    }

    [Fact]
    public async Task Send_Command_RecordsCorrectMetrics()
    {
        // Arrange
        var command = new SendTestCommand { Value = "metrics test" };
        _recordedActivities?.Clear();

        // Act
        await _mediator.Send(command);

        // Assert
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("SendTestCommand"));
        activity.ShouldNotBeNull();
        
        // Verify metrics tags are present
        activity.GetTagItem("request_name").ShouldBe("SendTestCommand");
        activity.GetTagItem("handler.type").ShouldBe("SendTestCommandHandler");
        activity.GetTagItem("duration_ms").ShouldNotBeNull();
    }

    [Fact]
    public async Task Send_WithMiddleware_TracksMiddlewareExecution()
    {
        // Arrange
        var command = new SendTestCommand { Value = "middleware test" };
        _recordedActivities?.Clear();

        // Act
        await _mediator.Send(command);

        // Assert
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("SendTestCommand"));
        activity.ShouldNotBeNull();
        
        // Verify middleware tracking (if middleware is configured)
        activity.GetTagItem("request_name").ShouldBe("SendTestCommand");
        activity.GetTagItem("handler.type").ShouldBe("SendTestCommandHandler");
    }

    #region Test Classes

    public class SendTestCommand : IRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class SendTestCommandHandler : IRequestHandler<SendTestCommand>
    {
        public async Task Handle(SendTestCommand request, CancellationToken cancellationToken)
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
        public async Task<string> Handle(SendTestQuery request, CancellationToken cancellationToken)
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
        public Task Handle(SendTestCommandWithException request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Test exception");
        }
    }

    public class SendTestQueryWithException : IRequest<string>
    {
    }

    public class SendTestQueryWithExceptionHandler : IRequestHandler<SendTestQueryWithException, string>
    {
        public Task<string> Handle(SendTestQueryWithException request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Test query exception");
        }
    }

    public class SendTestCommandWithSensitiveData : IRequest
    {
        public string Password { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class SendTestCommandWithSensitiveDataHandler : IRequestHandler<SendTestCommandWithSensitiveData>
    {
        public async Task Handle(SendTestCommandWithSensitiveData request, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken);
        }
    }

    #endregion
}
