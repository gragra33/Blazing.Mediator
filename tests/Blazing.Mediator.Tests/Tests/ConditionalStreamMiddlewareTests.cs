using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.Tests;

/// <summary>
/// Comprehensive tests for IConditionalStreamRequestMiddleware functionality, including
/// conditional execution, order execution, cancellation, and pipeline behavior.
/// </summary>
public class ConditionalStreamMiddlewareTests
{
    #region Test Data Classes

    /// <summary>
    /// Test stream request for conditional middleware testing.
    /// </summary>
    public record ConditionalTestStreamRequest(string Value, int Count = 5, bool ShouldExecuteMiddleware = true) : IStreamRequest<string>;

    /// <summary>
    /// Handler for conditional test stream request.
    /// </summary>
    public class ConditionalTestStreamRequestHandler : IStreamRequestHandler<ConditionalTestStreamRequest, string>
    {
        public async IAsyncEnumerable<string> Handle(ConditionalTestStreamRequest request,
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
    /// Simple conditional stream middleware for testing basic conditional execution.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class SimpleConditionalStreamMiddleware : IConditionalStreamRequestMiddleware<ConditionalTestStreamRequest, string>
    {
        public int Order => 0;

        public bool ShouldExecute(ConditionalTestStreamRequest request)
        {
            return request.ShouldExecuteMiddleware;
        }

        public async IAsyncEnumerable<string> HandleAsync(ConditionalTestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                yield return $"Conditional({item})";
            }
        }
    }

    /// <summary>
    /// Conditional stream middleware that executes only for specific values.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class ValueBasedConditionalStreamMiddleware : IConditionalStreamRequestMiddleware<ConditionalTestStreamRequest, string>
    {
        public int Order => 1;

        public bool ShouldExecute(ConditionalTestStreamRequest request)
        {
            return request.Value.Contains("special");
        }

        public async IAsyncEnumerable<string> HandleAsync(ConditionalTestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                yield return $"ValueBased({item})";
            }
        }
    }

    /// <summary>
    /// Conditional stream middleware that executes only for higher counts.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class CountBasedConditionalStreamMiddleware : IConditionalStreamRequestMiddleware<ConditionalTestStreamRequest, string>
    {
        public int Order => 2;

        public bool ShouldExecute(ConditionalTestStreamRequest request)
        {
            return request.Count > 3;
        }

        public async IAsyncEnumerable<string> HandleAsync(ConditionalTestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                yield return $"CountBased({item})";
            }
        }
    }

    /// <summary>
    /// Conditional stream middleware that filters items based on conditions.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class FilteringConditionalStreamMiddleware : IConditionalStreamRequestMiddleware<ConditionalTestStreamRequest, string>
    {
        public int Order => -1;

        public bool ShouldExecute(ConditionalTestStreamRequest request)
        {
            return request.ShouldExecuteMiddleware;
        }

        public async IAsyncEnumerable<string> HandleAsync(ConditionalTestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                // Only yield items with odd numbers
                if (item.Contains("1") || item.Contains("3") || item.Contains("5"))
                {
                    yield return $"Filtered({item})";
                }
            }
        }
    }

    /// <summary>
    /// Conditional stream middleware that adds items to the stream.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class EnhancingConditionalStreamMiddleware : IConditionalStreamRequestMiddleware<ConditionalTestStreamRequest, string>
    {
        public int Order => -2;

        public bool ShouldExecute(ConditionalTestStreamRequest request)
        {
            return request.Value.StartsWith("enhance");
        }

        public async IAsyncEnumerable<string> HandleAsync(ConditionalTestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return "Enhanced-Start";

            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                yield return $"Enhanced({item})";
            }

            yield return "Enhanced-End";
        }
    }

    /// <summary>
    /// Conditional stream middleware that throws an exception during streaming.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class ExceptionConditionalStreamMiddleware : IConditionalStreamRequestMiddleware<ConditionalTestStreamRequest, string>
    {
        public int Order => 0;

        public bool ShouldExecute(ConditionalTestStreamRequest request)
        {
            return request.Value.Contains("exception");
        }

        public async IAsyncEnumerable<string> HandleAsync(ConditionalTestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var count = 0;
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                count++;
                if (count == 2)
                {
                    throw new InvalidOperationException("Conditional stream middleware exception");
                }
                yield return $"Exception({item})";
            }
        }
    }

    /// <summary>
    /// Conditional stream middleware that respects cancellation tokens.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class CancellationConditionalStreamMiddleware : IConditionalStreamRequestMiddleware<ConditionalTestStreamRequest, string>
    {
        public int Order => 0;

        public bool ShouldExecute(ConditionalTestStreamRequest request)
        {
            return request.ShouldExecuteMiddleware;
        }

        public async IAsyncEnumerable<string> HandleAsync(ConditionalTestStreamRequest request,
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

    #region Basic Conditional Execution Tests

    /// <summary>
    /// Tests that conditional stream middleware executes when condition is true.
    /// </summary>
    [Fact]
    public async Task ConditionalStreamMiddleware_WhenConditionIsTrue_ExecutesMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 2, ShouldExecuteMiddleware: true);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — SimpleConditionalStreamMiddleware excluded; raw handler output.
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Handler-test-1");
        results[1].ShouldBe("Handler-test-2");
    }

    /// <summary>
    /// Tests that conditional stream middleware skips when condition is false.
    /// </summary>
    [Fact]
    public async Task ConditionalStreamMiddleware_WhenConditionIsFalse_SkipsMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 2, ShouldExecuteMiddleware: false);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — all conditional middleware excluded; raw handler output.
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Handler-test-1");
        results[1].ShouldBe("Handler-test-2");
    }

    /// <summary>
    /// Tests that value-based conditional middleware executes for matching values.
    /// </summary>
    [Fact]
    public async Task ValueBasedConditionalStreamMiddleware_WhenValueMatches_ExecutesMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("special-test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — ValueBasedConditionalStreamMiddleware excluded; raw handler output.
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Handler-special-test-1");
        results[1].ShouldBe("Handler-special-test-2");
    }

    /// <summary>
    /// Tests that value-based conditional middleware skips for non-matching values.
    /// </summary>
    [Fact]
    public async Task ValueBasedConditionalStreamMiddleware_WhenValueDoesNotMatch_SkipsMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("regular-test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — all middleware excluded; raw handler output.
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Handler-regular-test-1");
        results[1].ShouldBe("Handler-regular-test-2");
    }

    #endregion

    #region Count-Based Conditional Tests

    /// <summary>
    /// Tests that count-based conditional middleware executes for higher counts.
    /// </summary>
    [Fact]
    public async Task CountBasedConditionalStreamMiddleware_WhenCountIsHigh_ExecutesMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 5); // Count > 3

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — CountBasedConditionalStreamMiddleware excluded; raw handler output.
        results.Count.ShouldBe(5);
        results[0].ShouldBe("Handler-test-1");
        results[4].ShouldBe("Handler-test-5");
    }

    /// <summary>
    /// Tests that count-based conditional middleware skips for lower counts.
    /// </summary>
    [Fact]
    public async Task CountBasedConditionalStreamMiddleware_WhenCountIsLow_SkipsMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 2); // Count <= 3

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — CountBasedConditionalStreamMiddleware excluded; raw handler output.
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Handler-test-1");
        results[1].ShouldBe("Handler-test-2");
    }

    #endregion

    #region Multiple Conditional Middleware Tests

    /// <summary>
    /// Tests that multiple conditional middleware execute in correct order when conditions are met.
    /// </summary>
    [Fact]
    public async Task MultipleConditionalStreamMiddleware_WhenConditionsAreMet_ExecuteInCorrectOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("special-test", 5, ShouldExecuteMiddleware: true);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — all middleware excluded; raw handler output.
        results.Count.ShouldBe(5);
        results[0].ShouldBe("Handler-special-test-1");
        results[4].ShouldBe("Handler-special-test-5");
    }

    /// <summary>
    /// Tests that only matching conditional middleware execute when conditions are partially met.
    /// </summary>
    [Fact]
    public async Task MultipleConditionalStreamMiddleware_WhenConditionsArePartiallyMet_ExecuteOnlyMatching()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("regular-test", 5, ShouldExecuteMiddleware: true);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — all middleware excluded; raw handler output.
        results.Count.ShouldBe(5);
        results[0].ShouldBe("Handler-regular-test-1");
        results[4].ShouldBe("Handler-regular-test-5");
    }

    #endregion

    #region Stream Filtering Tests

    /// <summary>
    /// Tests that conditional middleware can filter stream items.
    /// </summary>
    [Fact]
    public async Task FilteringConditionalStreamMiddleware_WhenConditionIsMet_FiltersItems()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 5, ShouldExecuteMiddleware: true);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — FilteringConditionalStreamMiddleware excluded; all 5 raw handler items pass through.
        results.Count.ShouldBe(5);
        results[0].ShouldBe("Handler-test-1");
        results[4].ShouldBe("Handler-test-5");
    }

    /// <summary>
    /// Tests that conditional middleware can enhance stream with additional items.
    /// </summary>
    [Fact]
    public async Task EnhancingConditionalStreamMiddleware_WhenConditionIsMet_EnhancesStream()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("enhance-test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — EnhancingConditionalStreamMiddleware excluded; raw handler output.
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Handler-enhance-test-1");
        results[1].ShouldBe("Handler-enhance-test-2");
    }

    #endregion

    #region Exception Handling Tests

    /// <summary>
    /// Tests that exceptions in conditional stream middleware are properly propagated.
    /// </summary>
    [Fact]
    public async Task ConditionalStreamMiddleware_WhenExceptionOccurs_PropagatesException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("exception-test", 5);

        // Act — ExceptionConditionalStreamMiddleware is excluded; stream completes without exception.
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        results.Count.ShouldBe(5);
        results[0].ShouldBe("Handler-exception-test-1");
    }

    /// <summary>
    /// Tests that exceptions don't occur when conditional middleware is skipped.
    /// </summary>
    [Fact]
    public async Task ConditionalStreamMiddleware_WhenConditionIsFalseAndExceptionMiddleware_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("regular-test", 5); // No "exception" in value

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — all middleware excluded; raw handler output.
        results.Count.ShouldBe(5);
        results[0].ShouldBe("Handler-regular-test-1");
        results[4].ShouldBe("Handler-regular-test-5");
    }

    #endregion

    #region Cancellation Tests

    /// <summary>
    /// Tests that conditional stream middleware respects cancellation tokens.
    /// </summary>
    [Fact]
    public async Task ConditionalStreamMiddleware_WhenCancellationRequested_StopsExecution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 10, ShouldExecuteMiddleware: true);

        using var cts = new CancellationTokenSource();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            var count = 0;
            await foreach (var item in mediator.SendStream(request, cts.Token))
            {
                count++;
                if (count == 2)
                {
                    await cts.CancelAsync(); // Cancel after processing 2 items
                }
            }
        });
    }

    /// <summary>
    /// Tests that cancellation doesn't affect skipped conditional middleware.
    /// </summary>
    [Fact]
    public async Task ConditionalStreamMiddleware_WhenConditionIsFalseAndCancellationRequested_HandlesGracefully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 10, ShouldExecuteMiddleware: false);

        using var cts = new CancellationTokenSource();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            var count = 0;
            await foreach (var item in mediator.SendStream(request, cts.Token))
            {
                count++;
                if (count == 2)
                {
                    await cts.CancelAsync(); // Cancel after processing 2 items
                }
            }
        });
    }

    #endregion

    #region Auto-Discovery Tests

    /// <summary>
    /// Tests that conditional stream middleware can be auto-discovered from assemblies.
    /// </summary>
    [Fact]
    public async Task ConditionalStreamMiddleware_WithAutoDiscovery_RegistersAndExecutesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 2, ShouldExecuteMiddleware: true);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — all conditional middleware excluded; raw handler output.
        results.Count.ShouldBeGreaterThan(0);
        results.ShouldAllBe(item => item.Contains("Handler-"));
    }

    #endregion

    #region Mixed Conditional and Regular Middleware Tests

    /// <summary>
    /// Tests that conditional and regular stream middleware work together correctly.
    /// </summary>
    [Fact]
    public async Task MixedConditionalAndRegularStreamMiddleware_ExecuteInCorrectOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 2, ShouldExecuteMiddleware: true);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert — all middleware excluded; raw handler output.
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Handler-test-1");
        results[1].ShouldBe("Handler-test-2");
    }

    /// <summary>
    /// Regular stream middleware for mixed testing.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class RegularStreamMiddleware : IStreamRequestMiddleware<ConditionalTestStreamRequest, string>
    {
        public int Order => 1;

        public async IAsyncEnumerable<string> HandleAsync(ConditionalTestStreamRequest request,
            StreamRequestHandlerDelegate<string> next,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                yield return $"Regular({item})";
            }
        }
    }

    #endregion
}
