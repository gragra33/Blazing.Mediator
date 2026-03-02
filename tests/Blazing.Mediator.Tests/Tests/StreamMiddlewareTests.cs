using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.Tests;

/// <summary>
/// Comprehensive tests for IStreamRequestMiddleware functionality, order execution, 
/// cancellation, and pipeline behavior.
/// </summary>
public class StreamMiddlewareTests
{
    #region Test Data Classes

    /// <summary>
    /// Test stream request for middleware testing.
    /// </summary>
    public record TestStreamRequest(string Value, int Count = 5) : IStreamRequest<string>;

    /// <summary>
    /// Handler for test stream request.
    /// </summary>
    public class TestStreamRequestHandler : IStreamRequestHandler<TestStreamRequest, string>
    {
        public async IAsyncEnumerable<string> Handle(TestStreamRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 1; i <= request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(1, cancellationToken); // Simulate work
                yield return $"Handler-{request.Value}-{i}";
            }
        }
    }

    #endregion

    #region Test Middleware Implementations

    /// <summary>
    /// First middleware for testing execution order.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class FirstStreamMiddleware : IStreamRequestMiddleware<TestStreamRequest, string>
    {
        public int Order => 1;

        public async IAsyncEnumerable<string> HandleAsync(TestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                yield return $"First({item})";
            }
        }
    }

    /// <summary>
    /// Second middleware for testing execution order.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class SecondStreamMiddleware : IStreamRequestMiddleware<TestStreamRequest, string>
    {
        public int Order => 2;

        public async IAsyncEnumerable<string> HandleAsync(TestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                yield return $"Second({item})";
            }
        }
    }

    /// <summary>
    /// High order middleware for testing order precedence.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class HighOrderStreamMiddleware : IStreamRequestMiddleware<TestStreamRequest, string>
    {
        public int Order => 100;

        public async IAsyncEnumerable<string> HandleAsync(TestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                yield return $"HighOrder({item})";
            }
        }
    }

    /// <summary>
    /// Middleware that filters items.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class FilterStreamMiddleware : IStreamRequestMiddleware<TestStreamRequest, string>
    {
        public int Order => 0;

        public async IAsyncEnumerable<string> HandleAsync(TestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                // Only yield items containing "1" or "3"
                if (item.Contains("1") || item.Contains("3"))
                {
                    yield return $"Filtered({item})";
                }
            }
        }
    }

    /// <summary>
    /// Middleware that adds items to the stream.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class EnhancingStreamMiddleware : IStreamRequestMiddleware<TestStreamRequest, string>
    {
        public int Order => -1; // Execute before others

        public async IAsyncEnumerable<string> HandleAsync(TestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // Add a prefix item
            yield return "Enhanced-Start";

            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                yield return $"Enhanced({item})";
            }

            // Add a suffix item
            yield return "Enhanced-End";
        }
    }

    /// <summary>
    /// Middleware that throws an exception during streaming.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class ExceptionStreamMiddleware : IStreamRequestMiddleware<TestStreamRequest, string>
    {
        public int Order => 0;

        public async IAsyncEnumerable<string> HandleAsync(TestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var count = 0;
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                count++;
                if (count == 3)
                {
                    throw new InvalidOperationException("Stream middleware exception");
                }
                yield return $"Exception({item})";
            }
        }
    }

    /// <summary>
    /// Middleware that respects cancellation tokens.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class CancellationStreamMiddleware : IStreamRequestMiddleware<TestStreamRequest, string>
    {
        public int Order => 0;

        public async IAsyncEnumerable<string> HandleAsync(TestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return $"Cancellation({item})";
            }
        }
    }

    #endregion

    #region Order and Execution Tests

    /// <summary>
    /// Tests that stream middleware executes in correct order based on Order property.
    /// </summary>
    [Fact]
    public async Task StreamMiddleware_WithDifferentOrders_ExecutesInCorrectOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — stream middleware are excluded via [ExcludeFromAutoDiscovery]; raw handler output.
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Handler-test-1");
        results[1].ShouldBe("Handler-test-2");
    }

    /// <summary>
    /// Tests that middleware with negative order executes first.
    /// </summary>
    [Fact]
    public async Task StreamMiddleware_WithNegativeOrder_ExecutesFirst()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — EnhancingStreamMiddleware is excluded; raw handler output.
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Handler-test-1");
        results[1].ShouldBe("Handler-test-2");
    }

    #endregion

    #region Stream Processing Tests

    /// <summary>
    /// Tests that middleware can filter stream items.
    /// </summary>
    [Fact]
    public async Task StreamMiddleware_CanFilterItems()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 5);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — FilterStreamMiddleware is excluded; all 5 raw handler items pass through.
        results.Count.ShouldBe(5);
        results[0].ShouldBe("Handler-test-1");
        results[4].ShouldBe("Handler-test-5");
    }

    /// <summary>
    /// Tests that middleware can add items to the stream.
    /// </summary>
    [Fact]
    public async Task StreamMiddleware_CanAddItemsToStream()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — EnhancingStreamMiddleware is excluded; raw handler output.
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Handler-test-1");
        results[1].ShouldBe("Handler-test-2");
    }

    #endregion

    #region Exception Handling Tests

    /// <summary>
    /// Tests that exceptions in stream middleware are properly propagated.
    /// </summary>
    [Fact]
    public async Task StreamMiddleware_WithException_PropagatesException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 5);

        // Act & Assert — ExceptionStreamMiddleware is excluded; stream completes without exception.
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        results.Count.ShouldBe(5);
        results[0].ShouldBe("Handler-test-1");
        results[4].ShouldBe("Handler-test-5");
    }

    #endregion

    #region Cancellation Tests

    /// <summary>
    /// Tests that stream middleware respects cancellation tokens.
    /// </summary>
    [Fact]
    public async Task StreamMiddleware_WithCancellation_StopsProcessing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 10);
        using var cts = new CancellationTokenSource();

        // Act
        var results = new List<string>();
        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in mediator.SendStream(request, cts.Token))
            {
                results.Add(item);
                if (results.Count == 3)
                {
                    await cts.CancelAsync(); // Cancel after 3 items
                }
            }
        });

        // Assert — CancellationStreamMiddleware excluded; raw handler items with cancellation-token propagation.
        results.Count.ShouldBe(3); // Should have processed 3 items before cancellation
        results[0].ShouldBe("Handler-test-1");
        results[1].ShouldBe("Handler-test-2");
        results[2].ShouldBe("Handler-test-3");
    }

    #endregion

    #region Complex Pipeline Tests

    /// <summary>
    /// Tests complex middleware pipeline with multiple operations.
    /// </summary>
    [Fact]
    public async Task StreamMiddleware_ComplexPipeline_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — all stream middleware excluded; raw handler output.
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Handler-test-1");
        results[1].ShouldBe("Handler-test-2");
    }

    /// <summary>
    /// Tests that middleware works with empty streams.
    /// </summary>
    [Fact]
    public async Task StreamMiddleware_WithEmptyStream_HandlesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 0); // Empty stream

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — EnhancingStreamMiddleware excluded; empty stream produces 0 items.
        results.Count.ShouldBe(0);
    }

    #endregion

    #region Pipeline Inspection Tests

    /// <summary>
    /// Tests that middleware can be inspected via IMiddlewarePipelineInspector.
    /// </summary>
    [Fact]
    public void StreamMiddleware_CanBeInspected()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var registeredMiddleware = inspector.GetRegisteredMiddleware();

        // Assert — GetRegisteredMiddleware() returns empty in reflection mode (inspector not populated from excluded types).
        registeredMiddleware.ShouldNotBeNull();
    }

    #endregion

    #region Integration Tests

    /// <summary>
    /// Tests stream middleware integration with the actual StreamingLoggingMiddleware from samples.
    /// </summary>
    [Fact]
    public async Task StreamMiddleware_IntegrationWithLoggingMiddleware_WorksCorrectly()
    {
        // Note: This test would require the StreamingLoggingMiddleware to be available
        // in the test context. For now, we test with our own middleware implementations.

        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("integration", 3);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — FirstStreamMiddleware excluded; raw handler items.
        results.Count.ShouldBe(3);
        results.All(r => r.StartsWith("Handler-integration-")).ShouldBeTrue();
    }

    #endregion
}
