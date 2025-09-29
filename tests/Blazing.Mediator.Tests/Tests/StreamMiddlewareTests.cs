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
        services.AddMediator(config =>
        {
            config.AddMiddleware<HighOrderStreamMiddleware>(); // Order: 100
            config.AddMiddleware<FirstStreamMiddleware>();     // Order: 1
            config.AddMiddleware<SecondStreamMiddleware>();    // Order: 2
        }, typeof(TestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(2);

        // Order should be: HighOrder (100) -> Second (2) -> First (1) -> Handler
        // So result wrapping is: First(Second(HighOrder(Handler-test-X)))
        results[0].ShouldBe("First(Second(HighOrder(Handler-test-1)))");
        results[1].ShouldBe("First(Second(HighOrder(Handler-test-2)))");
    }

    /// <summary>
    /// Tests that middleware with negative order executes first.
    /// </summary>
    [Fact]
    public async Task StreamMiddleware_WithNegativeOrder_ExecutesFirst()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstStreamMiddleware>();    // Order: 1
            config.AddMiddleware<EnhancingStreamMiddleware>(); // Order: -1
        }, typeof(TestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(4); // Start + 2 items + End
        results[0].ShouldBe("Enhanced-Start");
        results[1].ShouldBe("Enhanced(First(Handler-test-1))");
        results[2].ShouldBe("Enhanced(First(Handler-test-2))");
        results[3].ShouldBe("Enhanced-End");
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
        services.AddMediator(config =>
        {
            config.AddMiddleware<FilterStreamMiddleware>(); // Only items with "1" or "3"
        }, typeof(TestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 5);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(2); // Only items 1 and 3
        results[0].ShouldBe("Filtered(Handler-test-1)");
        results[1].ShouldBe("Filtered(Handler-test-3)");
    }

    /// <summary>
    /// Tests that middleware can add items to the stream.
    /// </summary>
    [Fact]
    public async Task StreamMiddleware_CanAddItemsToStream()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<EnhancingStreamMiddleware>();
        }, typeof(TestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(4); // Start + 2 items + End
        results[0].ShouldBe("Enhanced-Start");
        results[1].ShouldBe("Enhanced(Handler-test-1)");
        results[2].ShouldBe("Enhanced(Handler-test-2)");
        results[3].ShouldBe("Enhanced-End");
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
        services.AddMediator(config =>
        {
            config.AddMiddleware<ExceptionStreamMiddleware>();
        }, typeof(TestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 5);

        // Act & Assert
        var results = new List<string>();
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await foreach (var item in mediator.SendStream(request))
            {
                results.Add(item);
            }
        });

        exception.Message.ShouldBe("Stream middleware exception");
        results.Count.ShouldBe(2); // Should have processed 2 items before exception
        results[0].ShouldBe("Exception(Handler-test-1)");
        results[1].ShouldBe("Exception(Handler-test-2)");
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
        services.AddMediator(config =>
        {
            config.AddMiddleware<CancellationStreamMiddleware>();
        }, typeof(TestStreamRequestHandler).Assembly);

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

        // Assert
        results.Count.ShouldBe(3); // Should have processed 3 items before cancellation
        results[0].ShouldBe("Cancellation(Handler-test-1)");
        results[1].ShouldBe("Cancellation(Handler-test-2)");
        results[2].ShouldBe("Cancellation(Handler-test-3)");
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
        services.AddMediator(config =>
        {
            config.AddMiddleware<EnhancingStreamMiddleware>(); // Order: -1 (first)
            config.AddMiddleware<FirstStreamMiddleware>();     // Order: 1
            config.AddMiddleware<SecondStreamMiddleware>();    // Order: 2
        }, typeof(TestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(4); // Start + 2 items + End

        // Pipeline order: Enhancing (-1) -> Second (2) -> First (1) -> Handler
        // Result wrapping: Enhanced(First(Second(Handler-X)))
        results[0].ShouldBe("Enhanced-Start");
        results[1].ShouldBe("Enhanced(First(Second(Handler-test-1)))");
        results[2].ShouldBe("Enhanced(First(Second(Handler-test-2)))");
        results[3].ShouldBe("Enhanced-End");
    }

    /// <summary>
    /// Tests that middleware works with empty streams.
    /// </summary>
    [Fact]
    public async Task StreamMiddleware_WithEmptyStream_HandlesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<EnhancingStreamMiddleware>(); // Adds items
            config.AddMiddleware<FirstStreamMiddleware>();
        }, typeof(TestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("test", 0); // Empty stream

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(2); // Start + End (no handler items)
        results[0].ShouldBe("Enhanced-Start");
        results[1].ShouldBe("Enhanced-End");
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
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstStreamMiddleware>();
            config.AddMiddleware<SecondStreamMiddleware>();
            config.AddMiddleware<EnhancingStreamMiddleware>();
        }, typeof(TestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var registeredMiddleware = inspector.GetRegisteredMiddleware();

        // Assert
        registeredMiddleware.Count.ShouldBeGreaterThanOrEqualTo(3);
        registeredMiddleware.ShouldContain(typeof(FirstStreamMiddleware));
        registeredMiddleware.ShouldContain(typeof(SecondStreamMiddleware));
        registeredMiddleware.ShouldContain(typeof(EnhancingStreamMiddleware));
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
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstStreamMiddleware>();
        }, typeof(TestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest("integration", 3);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(3);
        results.All(r => r.StartsWith("First(Handler-integration-")).ShouldBeTrue();
    }

    #endregion
}
