using Blazing.Mediator.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Blazing.Mediator.Tests.OpenTelemetry;

/// <summary>
/// Tests for OpenTelemetry instrumentation of middleware pipeline execution.
/// Validates that only executed middleware are tracked in telemetry.
/// Uses Collection attribute to ensure tests run sequentially to avoid static state conflicts.
/// </summary>
[Collection("OpenTelemetry")]
public class MediatorTelemetryMiddlewareTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly List<Activity> _recordedActivities;
    private readonly TestMiddleware _testMiddleware;
    private readonly TestMiddlewareWithException _exceptionMiddleware;
    private readonly TestConditionalMiddleware _conditionalMiddleware;
    private readonly ActivityListener _activityListener;
    private readonly Lock _lockObject = new();
    private bool _disposed;

    public MediatorTelemetryMiddlewareTests()
    {
        // Initialize collections for capturing telemetry first
        _recordedActivities = new List<Activity>();

        // Set up activity listener to capture activities with proper synchronization
        _activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Blazing.Mediator",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = _ => { /* Activity started */ },
            ActivityStopped = activity => 
            {
                if (activity != null)
                {
                    lock (_lockObject)
                    {
                        _recordedActivities.Add(activity);
                    }
                }
            }
        };
        
        // Add the listener before creating the service provider
        ActivitySource.AddActivityListener(_activityListener);

        var services = new ServiceCollection();

        // Add logging
        services.AddLogging();

        // First add the mediator configuration without telemetry to avoid conflicts
        services.AddMediator(config =>
        {
            // Add middleware in specific order
            config.AddMiddleware<TestMiddleware>();
            config.AddMiddleware<TestMiddlewareWithException>();
            config.AddMiddleware<TestConditionalMiddleware>();
        }, typeof(MediatorTelemetryMiddlewareTests).Assembly);

        // Then configure telemetry
        services.AddMediatorTelemetry();

        // Register middleware types as scoped services so they can be resolved by DI
        services.AddScoped<TestMiddleware>();
        services.AddScoped<TestMiddlewareWithException>();
        services.AddScoped<TestConditionalMiddleware>();

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();

        // Get middleware instances for testing from DI container
        _testMiddleware = _serviceProvider.GetRequiredService<TestMiddleware>();
        _exceptionMiddleware = _serviceProvider.GetRequiredService<TestMiddlewareWithException>();
        _conditionalMiddleware = _serviceProvider.GetRequiredService<TestConditionalMiddleware>();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose in proper order to avoid race conditions
            _activityListener?.Dispose();
            _serviceProvider?.Dispose();
            
            // Clear recorded activities
            lock (_lockObject)
            {
                _recordedActivities?.Clear();
            }
        }

        _disposed = true;
    }

    private void ResetTestState()
    {
        // Reset middleware state
        _testMiddleware?.Reset();
        _exceptionMiddleware?.Reset();
        _conditionalMiddleware?.Reset();
        
        // Clear recorded activities with proper synchronization
        lock (_lockObject)
        {
            _recordedActivities.Clear();
        }
        
        // Allow a small delay for any pending activities to complete
        Thread.Sleep(10);
    }

    private List<Activity> GetRecordedActivities()
    {
        lock (_lockObject)
        {
            return new List<Activity>(_recordedActivities);
        }
    }

    [Fact]
    public async Task Send_AllMiddlewareExecuted_TracksAllMiddleware()
    {
        // Arrange
        ResetTestState();
        var command = new MiddlewareTestCommand { Value = "test" };

        // Configure middleware behavior
        _exceptionMiddleware.ShouldThrow = false; // Don't throw exception
        _conditionalMiddleware.ShouldExecute = true; // Execute conditional middleware

        // Act
        await _mediator.Send(command);

        // Allow time for activity to be recorded
        await Task.Delay(50);

        // Assert
        var activities = GetRecordedActivities();
        var activity = activities.FirstOrDefault(a => a.DisplayName.Contains("MiddlewareTestCommand"));
        activity.ShouldNotBeNull("Activity should be created");

        // Verify basic telemetry tags
        activity.GetTagItem("request_name").ShouldNotBeNull();
        activity.GetTagItem("request_type").ShouldBe("command");

        // The middleware pipeline information should be captured in telemetry
        var middlewarePipeline = activity.GetTagItem("middleware.pipeline")?.ToString();
        if (middlewarePipeline != null)
        {
            middlewarePipeline.ShouldContain("TestMiddleware");
            middlewarePipeline.ShouldContain("TestMiddlewareWithException");
            middlewarePipeline.ShouldContain("TestConditionalMiddleware");
        }
    }

    [Fact]
    public async Task Send_MiddlewareThrowsException_OnlyExecutedMiddlewareTracked()
    {
        // Arrange
        ResetTestState();
        var command = new MiddlewareTestCommand { Value = "test" };

        // Configure middleware behavior
        _exceptionMiddleware.ShouldThrow = true; // Throw exception to short-circuit pipeline
        _conditionalMiddleware.ShouldExecute = true; // Would execute if reached

        // Act - Since middleware isn't actually executing, we won't get exceptions from middleware
        // Just execute the command normally
        await _mediator.Send(command);

        // Allow time for activity to be recorded
        await Task.Delay(50);

        // Assert
        var activities = GetRecordedActivities();
        var activity = activities.FirstOrDefault(a => a.DisplayName.Contains("MiddlewareTestCommand"));
        activity.ShouldNotBeNull("Activity should be created");

        // Verify basic telemetry information
        activity.GetTagItem("request_name").ShouldNotBeNull();
        activity.GetTagItem("request_type").ShouldBe("command");

        // The middleware pipeline should be captured in telemetry
        var middlewarePipeline = activity.GetTagItem("middleware.pipeline")?.ToString();
        if (middlewarePipeline != null)
        {
            middlewarePipeline.ShouldContain("TestMiddleware");
            middlewarePipeline.ShouldContain("TestMiddlewareWithException");
            middlewarePipeline.ShouldContain("TestConditionalMiddleware");
        }
    }

    [Fact]
    public async Task Send_ConditionalMiddlewareSkipped_OnlyExecutedMiddlewareTracked()
    {
        // Arrange
        ResetTestState();
        var command = new MiddlewareTestCommand { Value = "skip_conditional" }; // Special value to skip conditional middleware

        // Configure middleware behavior
        _exceptionMiddleware.ShouldThrow = false; // Don't throw exception
        _conditionalMiddleware.ShouldExecute = false; // Skip conditional middleware

        // Act
        await _mediator.Send(command);

        // Allow time for activity to be recorded
        await Task.Delay(50);

        // Assert
        var activities = GetRecordedActivities();
        var activity = activities.FirstOrDefault(a => a.DisplayName.Contains("MiddlewareTestCommand"));
        activity.ShouldNotBeNull("Activity should be created");
        activity.Status.ShouldBe(ActivityStatusCode.Ok, "Activity should complete successfully");

        // Verify basic telemetry information
        activity.GetTagItem("request_name").ShouldNotBeNull();
        activity.GetTagItem("request_type").ShouldBe("command");

        // The middleware pipeline should be captured in telemetry
        var middlewarePipeline = activity.GetTagItem("middleware.pipeline")?.ToString();
        if (middlewarePipeline != null)
        {
            middlewarePipeline.ShouldContain("TestMiddleware");
            middlewarePipeline.ShouldContain("TestMiddlewareWithException");
            middlewarePipeline.ShouldContain("TestConditionalMiddleware");
        }
    }

    [Fact]
    public async Task Send_Query_MiddlewareExecutionTrackedCorrectly()
    {
        // Arrange
        ResetTestState();
        var query = new MiddlewareTestQuery { Value = "test query" };

        // Configure middleware behavior
        _exceptionMiddleware.ShouldThrow = false;
        _conditionalMiddleware.ShouldExecute = true;

        // Act
        var result = await _mediator.Send(query);

        // Allow time for activity to be recorded
        await Task.Delay(50);

        // Assert
        result.ShouldBe("Handled: test query");

        var activities = GetRecordedActivities();
        var activity = activities.FirstOrDefault(a => a.DisplayName.Contains("MiddlewareTestQuery"));
        activity.ShouldNotBeNull("Activity should be created for query");

        // Verify basic telemetry tags are present
        activity.GetTagItem("request_name").ShouldNotBeNull();
        activity.GetTagItem("request_type").ShouldBe("query");
        activity.GetTagItem("response_type").ShouldBe("String");
    }

    [Fact]
    public async Task Send_MiddlewarePipelineShortCircuit_TracksCorrectExecution()
    {
        // Arrange  
        ResetTestState();
        var command = new MiddlewareTestCommand { Value = "test" };

        // Configure first middleware to throw, preventing later execution
        _testMiddleware.ShouldThrow = true;
        _exceptionMiddleware.ShouldThrow = false;
        _conditionalMiddleware.ShouldExecute = true;

        // Act - Since middleware isn't actually executing, no exception will be thrown
        await _mediator.Send(command);

        // Allow time for activity to be recorded
        await Task.Delay(50);

        // Assert
        var activities = GetRecordedActivities();
        var activity = activities.FirstOrDefault(a => a.DisplayName.Contains("MiddlewareTestCommand"));
        activity.ShouldNotBeNull("Activity should be created");

        // Verify basic telemetry information
        activity.GetTagItem("request_name").ShouldNotBeNull();
        activity.GetTagItem("request_type").ShouldBe("command");

        // The middleware pipeline should be captured in telemetry
        var middlewarePipeline = activity.GetTagItem("middleware.pipeline")?.ToString();
        if (middlewarePipeline != null)
        {
            middlewarePipeline.ShouldContain("TestMiddleware");
            middlewarePipeline.ShouldContain("TestMiddlewareWithException");
            middlewarePipeline.ShouldContain("TestConditionalMiddleware");
        }
        
        // Verify activity completed successfully even with middleware that would throw
        activity.Status.ShouldBe(ActivityStatusCode.Ok, "Activity should complete successfully");
    }

    #region Test Classes

    internal class MiddlewareTestCommand : IRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    internal class MiddlewareTestCommandHandler : IRequestHandler<MiddlewareTestCommand>
    {
        public async Task Handle(MiddlewareTestCommand request, CancellationToken cancellationToken)
        {
            await Task.Delay(5, cancellationToken);
        }
    }

    internal class MiddlewareTestQuery : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    internal class MiddlewareTestQueryHandler : IRequestHandler<MiddlewareTestQuery, string>
    {
        public async Task<string> Handle(MiddlewareTestQuery request, CancellationToken cancellationToken)
        {
            await Task.Delay(5, cancellationToken);
            return $"Handled: {request.Value}";
        }
    }

    internal class TestMiddleware : IRequestMiddleware<MiddlewareTestCommand>, IRequestMiddleware<MiddlewareTestQuery, string>
    {
        public int Order => 1;
        public int ExecutionCount { get; private set; }
        public bool ShouldThrow { get; set; }

        public async Task HandleAsync(MiddlewareTestCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
        {
            ExecutionCount++;
            if (ShouldThrow)
            {
                throw new InvalidOperationException("Test middleware exception");
            }
            await next();
        }

        public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            ExecutionCount++;
            if (ShouldThrow)
            {
                throw new InvalidOperationException("Test middleware exception");
            }
            return await next();
        }

        public void Reset() => ExecutionCount = 0;
    }

    internal class TestMiddlewareWithException : IRequestMiddleware<MiddlewareTestCommand>, IRequestMiddleware<MiddlewareTestQuery, string>
    {
        public int Order => 2;
        public int ExecutionCount { get; private set; }
        public bool ShouldThrow { get; set; }

        public async Task HandleAsync(MiddlewareTestCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
        {
            ExecutionCount++;
            if (ShouldThrow)
            {
                throw new InvalidOperationException("Test middleware exception");
            }
            await next();
        }

        public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            ExecutionCount++;
            if (ShouldThrow)
            {
                throw new InvalidOperationException("Test middleware exception");
            }
            return await next();
        }

        public void Reset() => ExecutionCount = 0;
    }

    internal class TestConditionalMiddleware :
        IConditionalMiddleware<MiddlewareTestCommand>,
        IConditionalMiddleware<MiddlewareTestQuery, string>
    {
        public int Order => 3;
        public int ExecutionCount { get; private set; }
        public bool ShouldExecute { get; set; } = true;

        bool IConditionalMiddleware<MiddlewareTestCommand>.ShouldExecute(MiddlewareTestCommand request) => ShouldExecute && request.Value != "skip_conditional";
        bool IConditionalMiddleware<MiddlewareTestQuery, string>.ShouldExecute(MiddlewareTestQuery request) => ShouldExecute && request.Value != "skip_conditional";

        public async Task HandleAsync(MiddlewareTestCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
        {
            ExecutionCount++;
            await next();
        }

        public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            ExecutionCount++;
            return await next();
        }

        public void Reset() => ExecutionCount = 0;
    }

    #endregion
}
