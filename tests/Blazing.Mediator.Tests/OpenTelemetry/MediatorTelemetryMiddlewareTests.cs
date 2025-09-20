using Blazing.Mediator.Abstractions;
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
    private ServiceProvider _serviceProvider = null!;
    private IMediator _mediator = null!;
    private List<Activity>? _recordedActivities;
    private TestMiddleware _testMiddleware = null!;
    private TestMiddlewareWithException _exceptionMiddleware = null!;
    private TestConditionalMiddleware _conditionalMiddleware = null!;
    private ActivityListener? _activityListener;

    public MediatorTelemetryMiddlewareTests()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging();

        // Add mediator with telemetry enabled
        services.AddMediatorTelemetry();

        // Create middleware instances to track execution
        _testMiddleware = new TestMiddleware();
        _exceptionMiddleware = new TestMiddlewareWithException();
        _conditionalMiddleware = new TestConditionalMiddleware();

        services.AddSingleton(_testMiddleware);
        services.AddSingleton(_exceptionMiddleware);
        services.AddSingleton(_conditionalMiddleware);

        services.AddMediator(config =>
        {
            // Add middleware in specific order
            config.AddMiddleware<TestMiddleware>();
            config.AddMiddleware<TestMiddlewareWithException>();
            config.AddMiddleware<TestConditionalMiddleware>();
        }, typeof(MediatorTelemetryMiddlewareTests).Assembly);

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
    public async Task Send_AllMiddlewareExecuted_TracksAllMiddleware()
    {
        // Arrange
        var command = new MiddlewareTestCommand { Value = "test" };
        _testMiddleware.Reset();
        _exceptionMiddleware.Reset();
        _conditionalMiddleware.Reset();
        _recordedActivities?.Clear();

        // Configure middleware behavior
        _exceptionMiddleware.ShouldThrow = false; // Don't throw exception
        _conditionalMiddleware.ShouldExecute = true; // Execute conditional middleware

        // Act
        await _mediator.Send(command);

        // Assert
        // Verify all middleware executed
        _testMiddleware.ExecutionCount.ShouldBe(1, "TestMiddleware should execute");
        _exceptionMiddleware.ExecutionCount.ShouldBe(1, "TestMiddlewareWithException should execute");
        _conditionalMiddleware.ExecutionCount.ShouldBe(1, "TestConditionalMiddleware should execute");

        // Verify activity contains all executed middleware
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("MiddlewareTestCommand"));
        activity.ShouldNotBeNull("Activity should be created");

        var middlewarePipeline = activity.GetTagItem("middleware.pipeline")?.ToString();
        var middlewareExecuted = activity.GetTagItem("middleware.executed")?.ToString();

        if (middlewarePipeline != null)
        {
            middlewarePipeline.ShouldContain("TestMiddleware");
            middlewarePipeline.ShouldContain("TestMiddlewareWithException");
            middlewarePipeline.ShouldContain("TestConditionalMiddleware");
        }

        if (middlewareExecuted != null)
        {
            middlewareExecuted.ShouldContain("TestMiddleware");
            middlewareExecuted.ShouldContain("TestMiddlewareWithException");
            middlewareExecuted.ShouldContain("TestConditionalMiddleware");
        }
    }

    [Fact]
    public async Task Send_MiddlewareThrowsException_OnlyExecutedMiddlewareTracked()
    {
        // Arrange
        var command = new MiddlewareTestCommand { Value = "test" };
        _testMiddleware.Reset();
        _exceptionMiddleware.Reset();
        _conditionalMiddleware.Reset();
        _recordedActivities?.Clear();

        // Configure middleware behavior
        _exceptionMiddleware.ShouldThrow = true; // Throw exception to short-circuit pipeline
        _conditionalMiddleware.ShouldExecute = true; // Would execute if reached

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => _mediator.Send(command));
        exception.Message.ShouldBe("Test middleware exception");

        // Verify execution counts
        _testMiddleware.ExecutionCount.ShouldBe(1, "TestMiddleware should execute");
        _exceptionMiddleware.ExecutionCount.ShouldBe(1, "TestMiddlewareWithException should execute and throw");
        _conditionalMiddleware.ExecutionCount.ShouldBe(0, "TestConditionalMiddleware should NOT execute due to exception");

        // Verify activity contains only executed middleware
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("MiddlewareTestCommand"));
        activity.ShouldNotBeNull("Activity should be created");
        activity.Status.ShouldBe(ActivityStatusCode.Error, "Activity should have error status");

        var middlewareExecuted = activity.GetTagItem("middleware.executed")?.ToString();

        if (middlewareExecuted != null)
        {
            middlewareExecuted.ShouldContain("TestMiddleware");
            middlewareExecuted.ShouldContain("TestMiddlewareWithException");
            middlewareExecuted.ShouldNotContain("TestConditionalMiddleware");
        }

        // Verify exception details
        activity.GetTagItem("exception.type").ShouldBe("InvalidOperationException");
        var exceptionMessage = activity.GetTagItem("exception.message")?.ToString();
        exceptionMessage.ShouldNotBeNull();
        exceptionMessage.ShouldContain("Test middleware exception");
    }

    [Fact]
    public async Task Send_ConditionalMiddlewareSkipped_OnlyExecutedMiddlewareTracked()
    {
        // Arrange
        var command = new MiddlewareTestCommand { Value = "skip_conditional" }; // Special value to skip conditional middleware
        _testMiddleware.Reset();
        _exceptionMiddleware.Reset();
        _conditionalMiddleware.Reset();
        _recordedActivities?.Clear();

        // Configure middleware behavior
        _exceptionMiddleware.ShouldThrow = false; // Don't throw exception
        _conditionalMiddleware.ShouldExecute = false; // Skip conditional middleware

        // Act
        await _mediator.Send(command);

        // Assert
        // Verify execution counts
        _testMiddleware.ExecutionCount.ShouldBe(1, "TestMiddleware should execute");
        _exceptionMiddleware.ExecutionCount.ShouldBe(1, "TestMiddlewareWithException should execute");
        _conditionalMiddleware.ExecutionCount.ShouldBe(0, "TestConditionalMiddleware should be skipped");

        // Verify activity contains only executed middleware
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("MiddlewareTestCommand"));
        activity.ShouldNotBeNull("Activity should be created");
        activity.Status.ShouldBe(ActivityStatusCode.Ok, "Activity should complete successfully");

        var middlewareExecuted = activity.GetTagItem("middleware.executed")?.ToString();

        if (middlewareExecuted != null)
        {
            middlewareExecuted.ShouldContain("TestMiddleware");
            middlewareExecuted.ShouldContain("TestMiddlewareWithException");
            middlewareExecuted.ShouldNotContain("TestConditionalMiddleware");
        }
    }

    [Fact]
    public async Task Send_Query_MiddlewareExecutionTrackedCorrectly()
    {
        // Arrange
        var query = new MiddlewareTestQuery { Value = "test query" };
        _testMiddleware.Reset();
        _exceptionMiddleware.Reset();
        _conditionalMiddleware.Reset();
        _recordedActivities?.Clear();

        // Configure middleware behavior
        _exceptionMiddleware.ShouldThrow = false;
        _conditionalMiddleware.ShouldExecute = true;

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.ShouldBe("Handled: test query");

        // Verify all middleware executed
        _testMiddleware.ExecutionCount.ShouldBe(1);
        _exceptionMiddleware.ExecutionCount.ShouldBe(1);
        _conditionalMiddleware.ExecutionCount.ShouldBe(1);

        // Verify activity contains middleware information
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("MiddlewareTestQuery"));
        activity.ShouldNotBeNull("Activity should be created for query");

        var middlewareExecuted = activity.GetTagItem("middleware.executed")?.ToString();
        if (middlewareExecuted != null)
        {
            middlewareExecuted.ShouldContain("TestMiddleware");
            middlewareExecuted.ShouldContain("TestMiddlewareWithException");
            middlewareExecuted.ShouldContain("TestConditionalMiddleware");
        }

        // Verify query-specific tags
        activity.GetTagItem("request_type").ShouldBe("query");
        activity.GetTagItem("response_type").ShouldBe("String");
    }

    [Fact]
    public async Task Send_MiddlewarePipelineShortCircuit_TracksCorrectExecution()
    {
        // Arrange  
        var command = new MiddlewareTestCommand { Value = "test" };
        _testMiddleware.Reset();
        _exceptionMiddleware.Reset();
        _conditionalMiddleware.Reset();
        _recordedActivities?.Clear();

        // Configure first middleware to throw, preventing later execution
        _testMiddleware.ShouldThrow = true;
        _exceptionMiddleware.ShouldThrow = false;
        _conditionalMiddleware.ShouldExecute = true;

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _mediator.Send(command));

        // Assert execution counts - only first middleware should execute
        _testMiddleware.ExecutionCount.ShouldBe(1, "TestMiddleware should execute and throw");
        _exceptionMiddleware.ExecutionCount.ShouldBe(0, "TestMiddlewareWithException should NOT execute");
        _conditionalMiddleware.ExecutionCount.ShouldBe(0, "TestConditionalMiddleware should NOT execute");

        // Verify activity tracking
        var activity = _recordedActivities?.FirstOrDefault(a => a.DisplayName.Contains("MiddlewareTestCommand"));
        activity.ShouldNotBeNull();
        activity.Status.ShouldBe(ActivityStatusCode.Error);

        var middlewareExecuted = activity.GetTagItem("middleware.executed")?.ToString();
        if (middlewareExecuted != null)
        {
            middlewareExecuted.ShouldContain("TestMiddleware");
            middlewareExecuted.ShouldNotContain("TestMiddlewareWithException");
            middlewareExecuted.ShouldNotContain("TestConditionalMiddleware");
        }
    }

    #region Test Classes

    public class MiddlewareTestCommand : IRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class MiddlewareTestCommandHandler : IRequestHandler<MiddlewareTestCommand>
    {
        public async Task Handle(MiddlewareTestCommand request, CancellationToken cancellationToken)
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
        public async Task<string> Handle(MiddlewareTestQuery request, CancellationToken cancellationToken)
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

    public class TestMiddlewareWithException : IRequestMiddleware<MiddlewareTestCommand>, IRequestMiddleware<MiddlewareTestQuery, string>
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

    public class TestConditionalMiddleware :
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
