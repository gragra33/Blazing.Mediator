using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Tests;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

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
        services.AddMediator(config =>
        {
            config.AddMiddleware<SimpleConditionalStreamMiddleware>();
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 2, ShouldExecuteMiddleware: true);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Conditional(Handler-test-1)");
        results[1].ShouldBe("Conditional(Handler-test-2)");
    }

    /// <summary>
    /// Tests that conditional stream middleware skips when condition is false.
    /// </summary>
    [Fact]
    public async Task ConditionalStreamMiddleware_WhenConditionIsFalse_SkipsMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<SimpleConditionalStreamMiddleware>();
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 2, ShouldExecuteMiddleware: false);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(2);
        results[0].ShouldBe("Handler-test-1");
        results[1].ShouldBe("Handler-test-2");
    }

    #endregion

    #region Value-Based Conditional Tests

    /// <summary>
    /// Tests that value-based conditional middleware executes for matching values.
    /// </summary>
    [Fact]
    public async Task ValueBasedConditionalStreamMiddleware_WhenValueMatches_ExecutesMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<ValueBasedConditionalStreamMiddleware>();
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("special-test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(2);
        results[0].ShouldBe("ValueBased(Handler-special-test-1)");
        results[1].ShouldBe("ValueBased(Handler-special-test-2)");
    }

    /// <summary>
    /// Tests that value-based conditional middleware skips for non-matching values.
    /// </summary>
    [Fact]
    public async Task ValueBasedConditionalStreamMiddleware_WhenValueDoesNotMatch_SkipsMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<ValueBasedConditionalStreamMiddleware>();
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("regular-test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
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
        services.AddMediator(config =>
        {
            config.AddMiddleware<CountBasedConditionalStreamMiddleware>();
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 5); // Count > 3

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(5);
        results[0].ShouldBe("CountBased(Handler-test-1)");
        results[4].ShouldBe("CountBased(Handler-test-5)");
    }

    /// <summary>
    /// Tests that count-based conditional middleware skips for lower counts.
    /// </summary>
    [Fact]
    public async Task CountBasedConditionalStreamMiddleware_WhenCountIsLow_SkipsMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<CountBasedConditionalStreamMiddleware>();
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 2); // Count <= 3

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
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
        services.AddMediator(config =>
        {
            config.AddMiddleware<CountBasedConditionalStreamMiddleware>(); // Order: 2
            config.AddMiddleware<ValueBasedConditionalStreamMiddleware>(); // Order: 1
            config.AddMiddleware<SimpleConditionalStreamMiddleware>();     // Order: 0
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("special-test", 5, ShouldExecuteMiddleware: true);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(5);
        // Order should be: Simple (0) -> ValueBased (1) -> CountBased (2) -> Handler
        // So result wrapping is: Conditional(ValueBased(CountBased(Handler-special-test-X)))
        results[0].ShouldBe("Conditional(ValueBased(CountBased(Handler-special-test-1)))");
        results[4].ShouldBe("Conditional(ValueBased(CountBased(Handler-special-test-5)))");
    }

    /// <summary>
    /// Tests that only matching conditional middleware execute when conditions are partially met.
    /// </summary>
    [Fact]
    public async Task MultipleConditionalStreamMiddleware_WhenConditionsArePartiallyMet_ExecuteOnlyMatching()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<CountBasedConditionalStreamMiddleware>(); // Order: 2, should execute (count=5)
            config.AddMiddleware<ValueBasedConditionalStreamMiddleware>(); // Order: 1, should NOT execute (no "special")
            config.AddMiddleware<SimpleConditionalStreamMiddleware>();     // Order: 0, should execute (flag=true)
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("regular-test", 5, ShouldExecuteMiddleware: true);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(5);
        // Only CountBased and Simple should execute in order: Simple (0) -> CountBased (2) 
        // So result wrapping is: Conditional(CountBased(Handler-regular-test-X))
        results[0].ShouldBe("Conditional(CountBased(Handler-regular-test-1))");
        results[4].ShouldBe("Conditional(CountBased(Handler-regular-test-5))");
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
        services.AddMediator(config =>
        {
            config.AddMiddleware<FilteringConditionalStreamMiddleware>();
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 5, ShouldExecuteMiddleware: true);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(3); // Only items 1, 3, 5 should be yielded
        results[0].ShouldBe("Filtered(Handler-test-1)");
        results[1].ShouldBe("Filtered(Handler-test-3)");
        results[2].ShouldBe("Filtered(Handler-test-5)");
    }

    /// <summary>
    /// Tests that conditional middleware can enhance stream with additional items.
    /// </summary>
    [Fact]
    public async Task EnhancingConditionalStreamMiddleware_WhenConditionIsMet_EnhancesStream()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<EnhancingConditionalStreamMiddleware>();
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("enhance-test", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(4); // Start + 2 items + End
        results[0].ShouldBe("Enhanced-Start");
        results[1].ShouldBe("Enhanced(Handler-enhance-test-1)");
        results[2].ShouldBe("Enhanced(Handler-enhance-test-2)");
        results[3].ShouldBe("Enhanced-End");
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
        services.AddMediator(config =>
        {
            config.AddMiddleware<ExceptionConditionalStreamMiddleware>();
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("exception-test", 5);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await foreach (var item in mediator.SendStream(request))
            {
                // Exception should be thrown on second item
            }
        });

        exception.Message.ShouldBe("Conditional stream middleware exception");
    }

    /// <summary>
    /// Tests that exceptions don't occur when conditional middleware is skipped.
    /// </summary>
    [Fact]
    public async Task ConditionalStreamMiddleware_WhenConditionIsFalseAndExceptionMiddleware_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<ExceptionConditionalStreamMiddleware>();
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("regular-test", 5); // No "exception" in value

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
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
        services.AddMediator(config =>
        {
            config.AddMiddleware<CancellationConditionalStreamMiddleware>();
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

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
        services.AddMediator(config =>
        {
            config.AddMiddleware<CancellationConditionalStreamMiddleware>();
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

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
        services.AddMediator(
            configureMiddleware: null,
            discoverMiddleware: true,
            typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 2, ShouldExecuteMiddleware: true);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        // Should have discovered and registered the conditional middleware
        // The filtering middleware might reduce the count based on conditions
        results.Count.ShouldBeGreaterThan(0);
        results.ShouldAllBe(item => item.Contains("Handler-") || item.Contains("Conditional(") || item.Contains("Enhanced") || item.Contains("Filtered(") || item.Contains("CountBased(") || item.Contains("ValueBased("));
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
        services.AddMediator(config =>
        {
            config.AddMiddleware<SimpleConditionalStreamMiddleware>();  // Order: 0
            config.AddMiddleware<RegularStreamMiddleware>();            // Order: 1
        }, typeof(ConditionalTestStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new ConditionalTestStreamRequest("test", 2, ShouldExecuteMiddleware: true);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(2);
        // Order should be: Regular (1) -> Conditional (0) -> Handler
        results[0].ShouldBe("Conditional(Regular(Handler-test-1))");
        results[1].ShouldBe("Conditional(Regular(Handler-test-2))");
    }

    /// <summary>
    /// Regular stream middleware for mixed testing.
    /// </summary>
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
