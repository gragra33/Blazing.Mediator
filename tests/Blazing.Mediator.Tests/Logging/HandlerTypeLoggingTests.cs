using Blazing.Mediator.Logging;
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
        public Task Handle(TestCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    // Test query with response
    public record TestQuery(int Id) : IRequest<string>;

    public class TestQueryHandler : IRequestHandler<TestQuery, string>
    {
        public Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Result: {request.Id}");
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

        services.AddMediator(config =>
        {
            config.WithLogging(options =>
            {
                options.EnableSend = true;
                options.EnableDetailedHandlerInfo = true;
            });
            config.AddAssembly(typeof(HandlerTypeLoggingTests).Assembly);
        });

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var command = new TestCommand("test");

        // Act
        await mediator.Send(command);

        // Assert
        // Debug output for troubleshooting
        var allLogs = string.Join("\n", testLogger.LogMessages);
        
        var handlerResolutionLog = testLogger.LogMessages
            .FirstOrDefault(m => m.Contains("Resolving handler") || m.Contains("IRequestHandler"));

        // Should have handler resolution logs
        Assert.True(testLogger.LogMessages.Any(), "Expected log messages but none were captured");
        
        // Should contain IRequestHandler with TestCommand (might be in different log messages)
        Assert.Contains("IRequestHandler", allLogs);
        Assert.Contains("TestCommand", allLogs);
        
        // Should NOT contain backtick notation
        Assert.DoesNotContain("`1", allLogs);
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

        services.AddMediator(config =>
        {
            config.WithLogging(options =>
            {
                options.EnableSend = true;
                options.EnableDetailedHandlerInfo = true;
            });
            config.AddAssembly(typeof(HandlerTypeLoggingTests).Assembly);
        });

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var query = new TestQuery(42);

        // Act
        var result = await mediator.Send(query);

        // Assert
        var allLogs = string.Join("\n", testLogger.LogMessages);
        
        // Should have handler resolution logs
        Assert.True(testLogger.LogMessages.Any(), "Expected log messages but none were captured");
        
        // Should contain fully qualified generic type with both type parameters
        Assert.Contains("IRequestHandler", allLogs);
        Assert.Contains("TestQuery", allLogs);
        Assert.Contains("String", allLogs);
        
        // Should NOT contain backtick notation
        Assert.DoesNotContain("IRequestHandler`2", allLogs);
        Assert.DoesNotContain("`", allLogs);
        
        // Verify the query actually executed
        Assert.Equal("Result: 42", result);
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

        services.AddMediator(config =>
        {
            config.WithLogging(options =>
            {
                options.EnableSendStream = true;
                options.EnableDetailedHandlerInfo = true;
            });
            config.AddAssembly(typeof(HandlerTypeLoggingTests).Assembly);
        });

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var streamRequest = new TestStreamRequest(3);

        // Act
        var results = new List<int>();
        await foreach (var item in mediator.SendStream(streamRequest))
        {
            results.Add(item);
        }

        // Assert
        var allLogs = string.Join("\n", testLogger.LogMessages);
        
        // Should have handler resolution logs
        Assert.True(testLogger.LogMessages.Any(), "Expected log messages but none were captured");
        
        // Should contain fully qualified generic type with both type parameters
        Assert.Contains("IStreamRequestHandler", allLogs);
        Assert.Contains("TestStreamRequest", allLogs);
        Assert.Contains("Int32", allLogs);
        
        // Should NOT contain backtick notation
        Assert.DoesNotContain("IStreamRequestHandler`2", allLogs);
        Assert.DoesNotContain("`", allLogs);
        
        // Verify the stream actually executed
        Assert.Equal(new[] { 0, 1, 2 }, results);
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

        services.AddMediator(config =>
        {
            config.WithLogging(options =>
            {
                options.EnableSend = true;
                options.EnableDetailedHandlerInfo = true;
            });
            config.AddAssembly(typeof(HandlerTypeLoggingTests).Assembly);
        });

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var query = new TestQuery(100);

        // Act
        await mediator.Send(query);

        // Assert
        var allLogs = string.Join("\n", testLogger.LogMessages);
        
        // Verify handler resolution logging
        Assert.Contains("IRequestHandler<", allLogs);
        Assert.Contains("TestQuery", allLogs);
        
        // Verify NO backtick notation anywhere in logs
        Assert.DoesNotContain("`1", allLogs);
        Assert.DoesNotContain("`2", allLogs);
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

        services.AddMediator(config =>
        {
            config.WithLogging(options =>
            {
                options.EnableSend = true;
                options.EnableSendStream = true;
                options.EnableDetailedHandlerInfo = true;
            });
            config.AddAssembly(typeof(HandlerTypeLoggingTests).Assembly);
        });

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act - Execute all three operation types
        await mediator.Send(new TestCommand("test"));
        await mediator.Send(new TestQuery(1));
        
        var streamResults = new List<int>();
        await foreach (var item in mediator.SendStream(new TestStreamRequest(2)))
        {
            streamResults.Add(item);
        }

        // Assert
        var allLogs = string.Join("\n", testLogger.LogMessages);
        
        // Verify all handler types are fully qualified
        Assert.Contains("IRequestHandler<TestCommand>", allLogs);
        Assert.Contains("IRequestHandler<TestQuery, String>", allLogs);
        Assert.Contains("IStreamRequestHandler<TestStreamRequest, Int32>", allLogs);
        
        // Verify NO backtick notation in any logs
        var backtickMatches = System.Text.RegularExpressions.Regex.Matches(allLogs, @"`\d+");
        Assert.Empty(backtickMatches);
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

        services.AddMediator(config =>
        {
            config.AddAssembly(typeof(HandlerTypeLoggingTests).Assembly);
        });

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

        services.AddMediator(config =>
        {
            config.WithLogging(options =>
            {
                options.EnableSend = true;
                options.EnableDetailedHandlerInfo = true;
            });
            config.AddAssembly(typeof(HandlerTypeLoggingTests).Assembly);
        });

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        await mediator.Send(new TestCommand("test"));

        // Assert
        var handlerFoundLog = testLogger.LogMessages
            .FirstOrDefault(m => m.Contains("Handler found") || m.Contains("TestCommandHandler"));

        Assert.NotNull(handlerFoundLog);
        Assert.Contains("TestCommandHandler", handlerFoundLog);
    }
}
