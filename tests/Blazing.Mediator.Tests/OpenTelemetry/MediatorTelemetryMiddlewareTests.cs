using Blazing.Mediator.Configuration;
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
        services.AddMediator();

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

    #region Test Classes

    public class MiddlewareTestCommand : IRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class MiddlewareTestCommandHandler : IRequestHandler<MiddlewareTestCommand>
    {
        public async ValueTask Handle(MiddlewareTestCommand request, CancellationToken cancellationToken)
        {
            await Task.Delay(5, cancellationToken);
        }
    }

    public class MiddlewareTestQuery : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class MiddlewareTestQueryHandler : IRequestHandler<MiddlewareTestQuery, string>
    {
        public async ValueTask<string> Handle(MiddlewareTestQuery request, CancellationToken cancellationToken)
        {
            await Task.Delay(5, cancellationToken);
            return $"Handled: {request.Value}";
        }
    }

    public class TestMiddleware : IRequestMiddleware<MiddlewareTestCommand>, IRequestMiddleware<MiddlewareTestQuery, string>
    {
        public int Order => 1;
        public int ExecutionCount { get; private set; }
        public bool ShouldThrow { get; set; }

        public async ValueTask HandleAsync(MiddlewareTestCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
        {
            ExecutionCount++;
            if (ShouldThrow)
            {
                throw new InvalidOperationException("Test middleware exception");
            }
            await next();
        }

        public async ValueTask<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
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

    public class TestMiddlewareWithException : IRequestMiddleware<MiddlewareTestCommand>, IRequestMiddleware<MiddlewareTestQuery, string>
    {
        public int Order => 2;
        public int ExecutionCount { get; private set; }
        public bool ShouldThrow { get; set; }

        public async ValueTask HandleAsync(MiddlewareTestCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
        {
            ExecutionCount++;
            if (ShouldThrow)
            {
                throw new InvalidOperationException("Test middleware exception");
            }
            await next();
        }

        public async ValueTask<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
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

    public class TestConditionalMiddleware :
        IConditionalMiddleware<MiddlewareTestCommand>,
        IConditionalMiddleware<MiddlewareTestQuery, string>
    {
        public int Order => 3;
        public int ExecutionCount { get; private set; }
        public bool IsEnabled { get; set; } = true;

        public bool ShouldExecute(MiddlewareTestCommand request) => IsEnabled && request.Value != "skip_conditional";
        public bool ShouldExecute(MiddlewareTestQuery request) => IsEnabled && request.Value != "skip_conditional";

        public async ValueTask HandleAsync(MiddlewareTestCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
        {
            ExecutionCount++;
            await next();
        }

        public async ValueTask<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            ExecutionCount++;
            return await next();
        }

        public void Reset() => ExecutionCount = 0;
    }

    #endregion

    #region MiddlewareCaptureMode Tag Accuracy Tests

    [Fact]
    public async Task Command_ApplicableMode_EmitsPipelineTag()
    {
        // Arrange — fresh service provider with Applicable mode
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(new MediatorConfiguration()
            .WithTelemetry(options =>
            {
                options.MiddlewareCaptureMode = MiddlewareCaptureMode.Applicable;
            })
            .AddFromAssembly(typeof(MiddlewareTestCommand).Assembly));

        var localActivities = new List<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Blazing.Mediator",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = _ => { },
            ActivityStopped = activity => localActivities.Add(activity)
        };
        ActivitySource.AddActivityListener(listener);

        await using var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // Act
        await mediator.Send(new MiddlewareTestCommand { Value = "test" });

        // Assert
        var activity = localActivities.FirstOrDefault(a => a.OperationName == "Mediator.Send:MiddlewareTestCommand");
        activity.ShouldNotBeNull();
        activity.GetTagItem("request_middleware.capture_mode").ShouldBe("applicable");
        activity.GetTagItem("request_middleware.pipeline").ShouldNotBeNull();
        activity.GetTagItem("request_middleware.count").ShouldNotBeNull();
        activity.GetTagItem("request_middleware.orders").ShouldNotBeNull();
        activity.GetTagItem("request_middleware.executed_pipeline").ShouldBeNull("Applicable mode must not emit executed_pipeline");
    }

    [Fact]
    public async Task Command_ExecutedMode_WithConditionalSkip_EmitsCorrectCounts()
    {
        // Arrange — fresh service provider with Executed mode; TestConditionalMiddleware skips when value == "skip_conditional"
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(new MediatorConfiguration()
            .WithTelemetry(options =>
            {
                options.MiddlewareCaptureMode = MiddlewareCaptureMode.Executed;
            })
            .AddFromAssembly(typeof(MiddlewareTestCommand).Assembly));

        var localActivities = new List<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Blazing.Mediator",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = _ => { },
            ActivityStopped = activity => localActivities.Add(activity)
        };
        ActivitySource.AddActivityListener(listener);

        await using var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // Act — value triggers skip in TestConditionalMiddleware
        await mediator.Send(new MiddlewareTestCommand { Value = "skip_conditional" });

        // Assert
        var activity = localActivities.FirstOrDefault(a => a.OperationName == "Mediator.Send:MiddlewareTestCommand");
        activity.ShouldNotBeNull();
        activity.GetTagItem("request_middleware.capture_mode").ShouldBe("executed");
        var skippedCount = Convert.ToInt32(activity.GetTagItem("request_middleware.skipped_count"));
        skippedCount.ShouldBeGreaterThanOrEqualTo(1, "TestConditionalMiddleware must appear in the skipped list");
        activity.GetTagItem("request_middleware.pipeline").ShouldBeNull("Executed mode must not emit applicable pipeline tag");
    }

    [Fact]
    public async Task Command_NoneMode_EmitsNoMiddlewareTags()
    {
        // Arrange — fresh service provider with None mode
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(new MediatorConfiguration()
            .WithTelemetry(options =>
            {
                options.MiddlewareCaptureMode = MiddlewareCaptureMode.None;
            })
            .AddFromAssembly(typeof(MiddlewareTestCommand).Assembly));

        var localActivities = new List<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Blazing.Mediator",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = _ => { },
            ActivityStopped = activity => localActivities.Add(activity)
        };
        ActivitySource.AddActivityListener(listener);

        await using var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // Act
        await mediator.Send(new MiddlewareTestCommand { Value = "test" });

        // Assert
        var activity = localActivities.FirstOrDefault(a => a.OperationName == "Mediator.Send:MiddlewareTestCommand");
        activity.ShouldNotBeNull("Activity should still be created in None mode");
        activity.GetTagItem("request_middleware.pipeline").ShouldBeNull("None mode must not emit pipeline tag");
        activity.GetTagItem("request_middleware.executed_pipeline").ShouldBeNull("None mode must not emit executed_pipeline tag");
        activity.GetTagItem("request_middleware.capture_mode").ShouldBeNull("None mode must not emit capture_mode tag");
    }

    #endregion
}
