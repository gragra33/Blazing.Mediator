using Blazing.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests for streaming functionality with middleware integration.
/// Verifies that middleware works correctly with streaming requests.
/// </summary>
public class StreamingMiddlewareIntegrationTests
{
    private readonly Assembly _testAssembly = typeof(StreamingMiddlewareIntegrationTests).Assembly;

    // Test streaming request
    public class TestStreamRequest : IStreamRequest<string>
    {
        public int Count { get; set; } = 5;
        public string Prefix { get; set; } = "Item";
    }

    // Test streaming handler
    public class TestStreamHandler : IStreamRequestHandler<TestStreamRequest, string>
    {
        public async IAsyncEnumerable<string> Handle(TestStreamRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 1; i <= request.Count; i++)
            {
                await Task.Delay(10, cancellationToken); // Simulate some work
                yield return $"{request.Prefix} {i}";
            }
        }
    }

    // Test streaming middleware
    public class StreamingLoggingMiddleware<TRequest, TResponse> : IStreamRequestMiddleware<TRequest, TResponse>
        where TRequest : IStreamRequest<TResponse>
    {
        public int Order => 1;
        public List<string> LogEntries { get; } = new();

        public async IAsyncEnumerable<TResponse> HandleAsync(
            TRequest request,
            StreamRequestHandlerDelegate<TResponse> next,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            LogEntries.Add($"Stream started: {typeof(TRequest).Name}");
            var itemCount = 0;

            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                itemCount++;
                LogEntries.Add($"Stream item {itemCount}: {item}");
                yield return item;
            }

            LogEntries.Add($"Stream completed: {typeof(TRequest).Name}, Items: {itemCount}");
        }
    }

    // Conditional streaming middleware
    public class ConditionalStreamingMiddleware<TRequest, TResponse> : IConditionalStreamRequestMiddleware<TRequest, TResponse>
        where TRequest : IStreamRequest<TResponse>
    {
        public int Order => 2;
        public List<string> ProcessedRequests { get; } = new();

        public bool ShouldExecute(TRequest request)
        {
            return request is TestStreamRequest testRequest && testRequest.Count > 3;
        }

        public async IAsyncEnumerable<TResponse> HandleAsync(
            TRequest request,
            StreamRequestHandlerDelegate<TResponse> next,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ProcessedRequests.Add($"Processing: {typeof(TRequest).Name}");

            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Test that streaming middleware works correctly with streaming requests
    /// </summary>
    [Fact]
    public async Task StreamingMiddleware_WithStreamRequest_ExecutesCorrectly()
    {
        // Arrange
        var loggingMiddleware = new StreamingLoggingMiddleware<TestStreamRequest, string>();
        var services = new ServiceCollection();

        services.AddMediator(config =>
        {
            config.AddMiddleware<StreamingLoggingMiddleware<TestStreamRequest, string>>();
        }, _testAssembly);

        services.AddSingleton(loggingMiddleware);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest { Count = 3, Prefix = "Test" };

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(3);
        results[0].ShouldBe("Test 1");
        results[1].ShouldBe("Test 2");
        results[2].ShouldBe("Test 3");

        // Check middleware logging
        loggingMiddleware.LogEntries.Count.ShouldBe(5); // Start + 3 items + Complete
        loggingMiddleware.LogEntries[0].ShouldBe("Stream started: TestStreamRequest");
        loggingMiddleware.LogEntries[1].ShouldBe("Stream item 1: Test 1");
        loggingMiddleware.LogEntries[2].ShouldBe("Stream item 2: Test 2");
        loggingMiddleware.LogEntries[3].ShouldBe("Stream item 3: Test 3");
        loggingMiddleware.LogEntries[4].ShouldBe("Stream completed: TestStreamRequest, Items: 3");
    }

    /// <summary>
    /// Test conditional streaming middleware
    /// </summary>
    [Fact]
    public async Task ConditionalStreamingMiddleware_OnlyExecutesWhenConditionMet()
    {
        // Arrange
        var conditionalMiddleware = new ConditionalStreamingMiddleware<TestStreamRequest, string>();
        var services = new ServiceCollection();

        services.AddMediator(config =>
        {
            config.AddMiddleware<ConditionalStreamingMiddleware<TestStreamRequest, string>>();
        }, _testAssembly);

        services.AddSingleton(conditionalMiddleware);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert - Should NOT execute for count <= 3
        var smallRequest = new TestStreamRequest { Count = 2, Prefix = "Small" };
        var smallResults = new List<string>();
        await foreach (var item in mediator.SendStream(smallRequest))
        {
            smallResults.Add(item);
        }

        smallResults.Count.ShouldBe(2);
        conditionalMiddleware.ProcessedRequests.Count.ShouldBe(0);

        // Act & Assert - SHOULD execute for count > 3
        var largeRequest = new TestStreamRequest { Count = 5, Prefix = "Large" };
        var largeResults = new List<string>();
        await foreach (var item in mediator.SendStream(largeRequest))
        {
            largeResults.Add(item);
        }

        largeResults.Count.ShouldBe(5);
        conditionalMiddleware.ProcessedRequests.Count.ShouldBe(1);
        conditionalMiddleware.ProcessedRequests[0].ShouldBe("Processing: TestStreamRequest");
    }

    /// <summary>
    /// Test multiple streaming middleware executing in order
    /// </summary>
    [Fact]
    public async Task MultipleStreamingMiddleware_ExecuteInCorrectOrder()
    {
        // Arrange
        var loggingMiddleware = new StreamingLoggingMiddleware<TestStreamRequest, string>();
        var conditionalMiddleware = new ConditionalStreamingMiddleware<TestStreamRequest, string>();

        var services = new ServiceCollection();

        services.AddMediator(config =>
        {
            config.AddMiddleware<StreamingLoggingMiddleware<TestStreamRequest, string>>(); // Order 1
            config.AddMiddleware<ConditionalStreamingMiddleware<TestStreamRequest, string>>(); // Order 2
        }, _testAssembly);

        services.AddSingleton(loggingMiddleware);
        services.AddSingleton(conditionalMiddleware);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest { Count = 4, Prefix = "Multi" }; // Count > 3 so conditional will execute

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(4);

        // Both middleware should have executed
        loggingMiddleware.LogEntries.Count.ShouldBe(6); // Start + 4 items + Complete
        conditionalMiddleware.ProcessedRequests.Count.ShouldBe(1);

        // Logging middleware (Order 1) should execute around conditional middleware (Order 2)
        loggingMiddleware.LogEntries[0].ShouldBe("Stream started: TestStreamRequest");
        conditionalMiddleware.ProcessedRequests[0].ShouldBe("Processing: TestStreamRequest");
    }

    /// <summary>
    /// Test streaming middleware with auto-discovery
    /// </summary>
    [Fact]
    public async Task StreamingMiddleware_AutoDiscovery_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Enable auto-discovery for request middleware (should include streaming middleware)
        services.AddMediator(
            configureMiddleware: null,
            discoverMiddleware: true,
            discoverNotificationMiddleware: false,
            _testAssembly
        );

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Verify that streaming middleware was discovered
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var registeredMiddleware = inspector.GetRegisteredMiddleware();

        // Should include the streaming middleware types defined in this test class
        // Note: This depends on the auto-discovery implementation finding these middleware types
        registeredMiddleware.ShouldContain(typeof(StreamingLoggingMiddleware<,>));

        var request = new TestStreamRequest { Count = 2, Prefix = "Auto" };

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Auto 1");
        results[1].ShouldBe("Auto 2");
    }

    /// <summary>
    /// Test streaming with cancellation through middleware
    /// </summary>
    [Fact]
    public async Task StreamingMiddleware_WithCancellation_HandlesCorrectly()
    {
        // Arrange
        var loggingMiddleware = new StreamingLoggingMiddleware<TestStreamRequest, string>();
        var services = new ServiceCollection();

        services.AddMediator(config =>
        {
            config.AddMiddleware<StreamingLoggingMiddleware<TestStreamRequest, string>>();
        }, _testAssembly);

        services.AddSingleton(loggingMiddleware);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest { Count = 10, Prefix = "Cancel" };
        using var cts = new CancellationTokenSource();

        // Act
        var results = new List<string>();
        var task = Task.Run(async () =>
        {
            await foreach (var item in mediator.SendStream(request, cts.Token))
            {
                results.Add(item);
                if (results.Count >= 3)
                {
                    cts.Cancel(); // Cancel after 3 items
                    break;
                }
            }
        });

        // Wait for cancellation to propagate
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        results.Count.ShouldBe(3);

        // Middleware should have logged the items that were processed
        loggingMiddleware.LogEntries.Count.ShouldBeGreaterThanOrEqualTo(4); // Start + at least 3 items
        loggingMiddleware.LogEntries[0].ShouldBe("Stream started: TestStreamRequest");
    }
}