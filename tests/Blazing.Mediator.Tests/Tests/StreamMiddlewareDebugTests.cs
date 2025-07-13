using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Blazing.Mediator.Tests.Tests;

/// <summary>
/// Debug test to understand stream middleware execution order
/// </summary>
public class StreamMiddlewareDebugTests
{
    public record DebugStreamRequest(string Value, int Count = 2) : IStreamRequest<string>;

    public class DebugStreamRequestHandler : IStreamRequestHandler<DebugStreamRequest, string>
    {
        public async IAsyncEnumerable<string> Handle(DebugStreamRequest request, 
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 1; i <= request.Count; i++)
            {
                await Task.Delay(1, cancellationToken);
                yield return $"Handler-{request.Value}-{i}";
            }
        }
    }

    public class FirstDebugMiddleware : IStreamRequestMiddleware<DebugStreamRequest, string>
    {
        public int Order => 1;

        public async IAsyncEnumerable<string> HandleAsync(DebugStreamRequest request, 
            StreamRequestHandlerDelegate<string> next, 
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                yield return $"First({item})";
            }
        }
    }

    public class EnhancingDebugMiddleware : IStreamRequestMiddleware<DebugStreamRequest, string>
    {
        public int Order => -1;

        public async IAsyncEnumerable<string> HandleAsync(DebugStreamRequest request, 
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

    [Fact]
    public async Task Debug_StreamMiddleware_ExecutionOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstDebugMiddleware>();    // Order: 1
            config.AddMiddleware<EnhancingDebugMiddleware>(); // Order: -1
        }, typeof(DebugStreamRequestHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new DebugStreamRequest("debug", 2);

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert - Let's see what we actually get
        results.Count.ShouldBeGreaterThan(0);
        
        // Print results for debugging
        Console.WriteLine("Actual results:");
        for (int i = 0; i < results.Count; i++)
        {
            Console.WriteLine($"  [{i}] = \"{results[i]}\"");
        }
        
        // For now, just verify we got some results
        results.ShouldNotBeEmpty();
    }
}
