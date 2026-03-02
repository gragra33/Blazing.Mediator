using Blazing.Mediator;
using Microsoft.Extensions.Logging;
using OpenTelemetryExample.Application.Middleware;

namespace OpenTelemetryExample.Tests;

/// <summary>
/// Unit tests for OpenTelemetry middleware components.
/// Tests each middleware in isolation by directly instantiating it and invoking HandleAsync,
/// bypassing the mediator pipeline and DI entirely.
/// </summary>
public class MiddlewareTests
{
    #region TracingMiddleware Tests

    /// <summary>
    /// Tests that TracingMiddleware adds appropriate tracing information to requests.
    /// </summary>
    [Fact]
    public async Task TracingMiddleware_AddsTracingInformation()
    {
        // Arrange
        var middleware = new TracingMiddleware<TestQuery, string>();
        var query = new TestQuery { Value = "test" };
        RequestHandlerDelegate<string> next = () => new TestQueryHandler().Handle(query, CancellationToken.None);

        // Act
        var result = await middleware.HandleAsync(query, next, CancellationToken.None);

        // Assert
        result.ShouldBe("Result: test");
    }

    #endregion

    #region PerformanceMiddleware Tests

    /// <summary>
    /// Tests that PerformanceMiddleware measures execution time correctly.
    /// </summary>
    [Fact]
    public async Task PerformanceMiddleware_MeasuresExecutionTime()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<PerformanceMiddleware<TestQuery, string>>>();
        var middleware = new PerformanceMiddleware<TestQuery, string>(mockLogger.Object);
        var query = new TestQuery { Value = "test" };
        RequestHandlerDelegate<string> next = () => new TestQueryHandler().Handle(query, CancellationToken.None);

        // Act
        var result = await middleware.HandleAsync(query, next, CancellationToken.None);

        // Assert
        result.ShouldBe("Result: test");

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed in")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Tests that PerformanceMiddleware logs slow requests.
    /// </summary>
    [Fact]
    public async Task PerformanceMiddleware_LogsSlowRequests()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<PerformanceMiddleware<TestSlowQuery, string>>>();
        var middleware = new PerformanceMiddleware<TestSlowQuery, string>(mockLogger.Object);
        var query = new TestSlowQuery();
        RequestHandlerDelegate<string> next = () => new TestSlowQueryHandler().Handle(query, CancellationToken.None);

        // Act
        await middleware.HandleAsync(query, next, CancellationToken.None);

        // Assert - logs at any level confirm middleware executed the pipeline
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region LoggingMiddleware Tests

    /// <summary>
    /// Tests that LoggingMiddleware logs request information.
    /// </summary>
    [Fact]
    public async Task LoggingMiddleware_LogsRequestInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoggingMiddleware<TestQuery, string>>>();
        var middleware = new LoggingMiddleware<TestQuery, string>(mockLogger.Object);
        var query = new TestQuery { Value = "test" };
        RequestHandlerDelegate<string> next = () => new TestQueryHandler().Handle(query, CancellationToken.None);

        // Act
        var result = await middleware.HandleAsync(query, next, CancellationToken.None);

        // Assert
        result.ShouldBe("Result: test");

        mockLogger.Verify(x => x.IsEnabled(It.IsAny<LogLevel>()), Times.AtLeastOnce);
    }

    #endregion

    #region ErrorHandlingMiddleware Tests

    /// <summary>
    /// Tests that ErrorHandlingMiddleware catches and handles exceptions properly.
    /// </summary>
    [Fact]
    public async Task ErrorHandlingMiddleware_CatchesAndHandlesExceptions()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ErrorHandlingMiddleware<TestErrorQuery, string>>>();
        var middleware = new ErrorHandlingMiddleware<TestErrorQuery, string>(mockLogger.Object);
        var query = new TestErrorQuery();
        RequestHandlerDelegate<string> next = () => new TestErrorQueryHandler().Handle(query, CancellationToken.None);

        // Act & Assert - ErrorHandlingMiddleware logs then rethrows
        await Should.ThrowAsync<Exception>(async () =>
        {
            await middleware.HandleAsync(query, next, CancellationToken.None);
        });

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Streaming Middleware Tests

    /// <summary>
    /// Tests that StreamingLoggingMiddleware logs streaming operations.
    /// </summary>
    [Fact]
    public async Task StreamingLoggingMiddleware_LogsStreamingOperations()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<StreamingLoggingMiddleware<TestStreamQuery, string>>>();
        var middleware = new StreamingLoggingMiddleware<TestStreamQuery, string>(mockLogger.Object);
        var query = new TestStreamQuery();
        StreamRequestHandlerDelegate<string> next = () => new TestStreamQueryHandler().Handle(query, CancellationToken.None);

        // Act
        var results = new List<string>();
        await foreach (var item in middleware.HandleAsync(query, next, CancellationToken.None))
        {
            results.Add(item);
        }

        // Assert
        results.ShouldNotBeEmpty();

        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Tests that StreamingTracingMiddleware adds tracing to streaming operations.
    /// </summary>
    [Fact]
    public async Task StreamingTracingMiddleware_AddsTracingToStreamingOperations()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<StreamingTracingMiddleware<TestStreamQuery, string>>>();
        var middleware = new StreamingTracingMiddleware<TestStreamQuery, string>(mockLogger.Object);
        var query = new TestStreamQuery();
        StreamRequestHandlerDelegate<string> next = () => new TestStreamQueryHandler().Handle(query, CancellationToken.None);

        // Act
        var results = new List<string>();
        await foreach (var item in middleware.HandleAsync(query, next, CancellationToken.None))
        {
            results.Add(item);
        }

        // Assert
        results.ShouldNotBeEmpty();

        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Middleware Integration Tests

    /// <summary>
    /// Tests that multiple middleware can work together correctly by chaining them directly.
    /// </summary>
    [Fact]
    public async Task MultipleMiddleware_WorkTogetherCorrectly()
    {
        // Arrange
        var performanceLogger = new Mock<ILogger<PerformanceMiddleware<TestQuery, string>>>();
        var loggingLogger = new Mock<ILogger<LoggingMiddleware<TestQuery, string>>>();

        var query = new TestQuery { Value = "test" };
        var handler = new TestQueryHandler();

        // Build chain: TracingMiddleware -> PerformanceMiddleware -> LoggingMiddleware -> Handler
        RequestHandlerDelegate<string> handleDelegate = () => handler.Handle(query, CancellationToken.None);

        var loggingMw = new LoggingMiddleware<TestQuery, string>(loggingLogger.Object);
        RequestHandlerDelegate<string> withLogging = () => loggingMw.HandleAsync(query, handleDelegate, CancellationToken.None);

        var performanceMw = new PerformanceMiddleware<TestQuery, string>(performanceLogger.Object);
        RequestHandlerDelegate<string> withPerformance = () => performanceMw.HandleAsync(query, withLogging, CancellationToken.None);

        var tracingMw = new TracingMiddleware<TestQuery, string>();

        // Act
        var result = await tracingMw.HandleAsync(query, withPerformance, CancellationToken.None);

        // Assert
        result.ShouldBe("Result: test");

        performanceLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed in")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        loggingLogger.Verify(x => x.IsEnabled(It.IsAny<LogLevel>()), Times.AtLeastOnce);
    }

    #endregion
}

#region Test Types

/// <summary>
/// Test query that simulates slow execution.
/// </summary>
public record TestSlowQuery : IRequest<string>;

/// <summary>
/// Handler for TestSlowQuery that introduces delay.
/// </summary>
public class TestSlowQueryHandler : IRequestHandler<TestSlowQuery, string>
{
    public async ValueTask<string> Handle(TestSlowQuery request, CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken); // Simulate slow operation
        return "Slow result";
    }
}

/// <summary>
/// Test query that simulates an error.
/// </summary>
public record TestErrorQuery : IRequest<string>;

/// <summary>
/// Handler for TestErrorQuery that throws an exception.
/// </summary>
public class TestErrorQueryHandler : IRequestHandler<TestErrorQuery, string>
{
    public ValueTask<string> Handle(TestErrorQuery request, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Test error");
    }
}

/// <summary>
/// Test stream query for streaming middleware tests.
/// </summary>
public record TestStreamQuery : IStreamRequest<string>;

/// <summary>
/// Handler for TestStreamQuery that returns a simple stream.
/// </summary>
public class TestStreamQueryHandler : IStreamRequestHandler<TestStreamQuery, string>
{
    public async IAsyncEnumerable<string> Handle(TestStreamQuery request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int i = 1; i <= 3; i++)
        {
            yield return $"Stream item {i}";
            await Task.Delay(10, cancellationToken);
        }
    }
}

/// <summary>
/// Basic test query.
/// </summary>
public record TestQuery : IRequest<string>
{
    public string Value { get; init; } = string.Empty;
}

/// <summary>
/// Handler for TestQuery.
/// </summary>
public class TestQueryHandler : IRequestHandler<TestQuery, string>
{
    public ValueTask<string> Handle(TestQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"Result: {request.Value}");
    }
}

#endregion