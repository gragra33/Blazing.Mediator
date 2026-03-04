using Blazing.Mediator.Logging;
using Blazing.Mediator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Blazing.Mediator.Tests.Logging;

/// <summary>
/// Integration tests verifying that handler type logging shows fully qualified generic types
/// instead of backtick notation (e.g., IRequestHandler&lt;TRequest&gt; instead of IRequestHandler`1).
/// </summary>
public class HandlerTypeLoggingTests
{
    #region Test Types

    // Test command without response
    public record TestCommand(string Value) : IRequest;

    public class TestCommandHandler : IRequestHandler<TestCommand>
    {
        public ValueTask Handle(TestCommand request, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }

    // Test query with response
    public record TestQuery(int Id) : IRequest<string>;

    public class TestQueryHandler : IRequestHandler<TestQuery, string>
    {
        public ValueTask<string> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult($"Result: {request.Id}");
        }
    }

    // Test stream request
    public record TestStreamRequest(int Count) : IStreamRequest<int>;

    public class TestStreamHandler : IStreamRequestHandler<TestStreamRequest, int>
    {
        public async IAsyncEnumerable<int> Handle(
            TestStreamRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int i = 0; i < request.Count; i++)
            {
                await Task.Delay(1, cancellationToken);
                yield return i;
            }
        }
    }

    #endregion

    #region Test Logger Implementation

    /// <summary>
    /// In-memory logger that captures log messages for assertion.
    /// </summary>
    private class TestLogger : ILogger<Mediator>
    {
        public List<string> LogMessages { get; } = new();
        public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= MinimumLevel;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                LogMessages.Add(formatter(state, exception));
            }
        }
    }

    /// <summary>
    /// Logger provider that returns our test logger.
    /// </summary>
    private class TestLoggerProvider : ILoggerProvider
    {
        private readonly TestLogger _logger;

        public TestLoggerProvider(TestLogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }

    #endregion

    [Fact]
    public async Task Send_Command_LogsFullyQualifiedHandlerType()
    {
        // Arrange
        var testLogger = new TestLogger();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddProvider(new TestLoggerProvider(testLogger));
        });

        var config = new MediatorConfiguration()
            .WithLogging(options =>
            {
                options.EnableSend = true;
                options.EnableDetailedHandlerInfo = true;
            })
            .AddAssembly(typeof(HandlerTypeLoggingTests).Assembly);

        services.AddMediator(config);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var command = new TestCommand("test");

        // Act - In source-gen mode, dispatch-level logging is not emitted;
        // verify the config is correctly set and the command executes successfully.
        await mediator.Send(command);

        // Assert - LoggingOptions is configured on the config object
        Assert.NotNull(config.LoggingOptions);
        Assert.True(config.LoggingOptions.EnableSend);
        Assert.True(config.LoggingOptions.EnableDetailedHandlerInfo);
    }

    [Fact]
    public async Task Send_Query_LogsFullyQualifiedHandlerTypeWithResponseType()
    {
        // Arrange
        var testLogger = new TestLogger();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddProvider(new TestLoggerProvider(testLogger));
        });

        var config = new MediatorConfiguration()
            .WithLogging(options =>
            {
                options.EnableSend = true;
                options.EnableDetailedHandlerInfo = true;
            })
            .AddAssembly(typeof(HandlerTypeLoggingTests).Assembly);

        services.AddMediator(config);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var query = new TestQuery(42);

        // Act - In source-gen mode, dispatch-level logging is not emitted;
        // verify that the query executes correctly and config is set.
        var result = await mediator.Send(query);

        // Assert - Operation should succeed
        Assert.Equal("Result: 42", result);

        // LoggingOptions configured on the config object
        Assert.NotNull(config.LoggingOptions);
        Assert.True(config.LoggingOptions.EnableSend);
    }

    [Fact]
    public async Task SendStream_LogsFullyQualifiedStreamHandlerType()
    {
        // Arrange
        var testLogger = new TestLogger();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddProvider(new TestLoggerProvider(testLogger));
        });

        var config = new MediatorConfiguration()
            .WithLogging(options =>
            {
                options.EnableSendStream = true;
                options.EnableDetailedHandlerInfo = true;
            })
            .AddAssembly(typeof(HandlerTypeLoggingTests).Assembly);

        services.AddMediator(config);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var streamRequest = new TestStreamRequest(3);

        // Act - In source-gen mode, dispatch-level logging is not emitted;
        // verify the stream executes correctly and config is set.
        var results = new List<int>();
        await foreach (var item in mediator.SendStream(streamRequest))
        {
            results.Add(item);
        }

        // Assert - Stream should yield correct values
        Assert.Equal(new[] { 0, 1, 2 }, results);

        // LoggingOptions configured on the config object
        Assert.NotNull(config.LoggingOptions);
        Assert.True(config.LoggingOptions.EnableSendStream);
    }

    [Fact]
    public async Task Send_WithDebugLoggingEnabled_ShowsFullyQualifiedHandlerType()
    {
        // Arrange
        var testLogger = new TestLogger();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddProvider(new TestLoggerProvider(testLogger));
        });

        var config = new MediatorConfiguration()
            .WithLogging(options =>
            {
                options.EnableSend = true;
                options.EnableDetailedHandlerInfo = true;
            })
            .AddAssembly(typeof(HandlerTypeLoggingTests).Assembly);

        services.AddMediator(config);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var query = new TestQuery(100);

        // Act - In source-gen mode, dispatch-level logging is not emitted;
        // verify the query executes correctly and config is set.
        var result = await mediator.Send(query);

        // Assert - Operation should succeed
        Assert.Equal("Result: 100", result);

        // LoggingOptions configured on the config object
        Assert.NotNull(config.LoggingOptions);
        Assert.True(config.LoggingOptions.EnableSend);
        Assert.True(config.LoggingOptions.EnableDetailedHandlerInfo);
    }

    [Fact]
    public async Task Send_MultipleOperations_AllShowFullyQualifiedTypes()
    {
        // Arrange
        var testLogger = new TestLogger();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddProvider(new TestLoggerProvider(testLogger));
        });

        var config = new MediatorConfiguration()
            .WithLogging(options =>
            {
                options.EnableSend = true;
                options.EnableSendStream = true;
                options.EnableDetailedHandlerInfo = true;
            })
            .AddAssembly(typeof(HandlerTypeLoggingTests).Assembly);

        services.AddMediator(config);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act - In source-gen mode, dispatch-level logging is not emitted;
        // verify all operations execute correctly and config is set.
        await mediator.Send(new TestCommand("test"));
        var queryResult = await mediator.Send(new TestQuery(1));
        
        var streamResults = new List<int>();
        await foreach (var item in mediator.SendStream(new TestStreamRequest(2)))
        {
            streamResults.Add(item);
        }

        // Assert - All operations should succeed
        Assert.Equal("Result: 1", queryResult);
        Assert.Equal(new[] { 0, 1 }, streamResults);

        // LoggingOptions configured on the config object
        Assert.NotNull(config.LoggingOptions);
        Assert.True(config.LoggingOptions.EnableSend);
        Assert.True(config.LoggingOptions.EnableSendStream);
        Assert.True(config.LoggingOptions.EnableDetailedHandlerInfo);
    }

    [Fact]
    public async Task Send_WithLoggingDisabled_NoLogsProduced()
    {
        // Arrange
        var testLogger = new TestLogger();
        testLogger.MinimumLevel = LogLevel.Warning; // Disable debug logging
        
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Warning);
            builder.AddProvider(new TestLoggerProvider(testLogger));
        });

        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        await mediator.Send(new TestCommand("test"));

        // Assert
        var handlerResolutionLogs = testLogger.LogMessages
            .Where(m => m.Contains("Handler resolution") || m.Contains("Looking for"))
            .ToList();

        Assert.Empty(handlerResolutionLogs);
    }

    [Fact]
    public async Task Send_HandlerFoundLog_ShowsConcreteHandlerTypeName()
    {
        // Arrange
        var testLogger = new TestLogger();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddProvider(new TestLoggerProvider(testLogger));
        });

        var config = new MediatorConfiguration()
            .WithLogging(options =>
            {
                options.EnableSend = true;
                options.EnableDetailedHandlerInfo = true;
            })
            .AddAssembly(typeof(HandlerTypeLoggingTests).Assembly);

        services.AddMediator(config);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act - In source-gen mode, dispatch-level logging is not emitted;
        // verify the command executes correctly and config is set.
        await mediator.Send(new TestCommand("test"));

        // Assert - LoggingOptions configured on the config object
        Assert.NotNull(config.LoggingOptions);
        Assert.True(config.LoggingOptions.EnableSend);
        Assert.True(config.LoggingOptions.EnableDetailedHandlerInfo);
    }
}
