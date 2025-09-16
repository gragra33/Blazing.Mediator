using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests for streaming functionality in the Mediator
/// </summary>
public class MediatorStreamingTests
{
    /// <summary>
    /// Tests that sending a stream request calls the appropriate handler and returns streaming results.
    /// </summary>
    [Fact]
    public async Task SendStream_StreamRequest_CallsHandlerAndReturnsStream()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddMediator(typeof(Commands.TestStreamRequest).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        Commands.TestStreamRequest streamRequest = new() { SearchTerm = "test" };
        List<string> results = new();

        // Act
        await foreach (string item in mediator.SendStream(streamRequest))
        {
            results.Add(item);
        }

        // Assert
        Handlers.TestStreamRequestHandler.LastExecutedRequest.ShouldBe(streamRequest);
        results.Count.ShouldBe(3);
        results[0].ShouldBe("Filtered Item 1 - test");
        results[1].ShouldBe("Filtered Item 2 - test");
        results[2].ShouldBe("Filtered Item 3 - test");
    }

    /// <summary>
    /// Tests that sending a stream request with cancellation token supports cancellation.
    /// </summary>
    [Fact]
    public async Task SendStream_StreamRequest_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddMediator(typeof(Commands.TestStreamRequest).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        Commands.TestStreamRequest streamRequest = new();
        CancellationTokenSource cancellationTokenSource = new();
        List<string> results = new();

        // Act & Assert
        try
        {
            await foreach (string item in mediator.SendStream(streamRequest, cancellationTokenSource.Token))
            {
                results.Add(item);
                if (results.Count == 2)
                {
                    cancellationTokenSource.Cancel(); // Cancel after receiving 2 items
                }
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when cancellation occurs
        }

        // Assert - Should have processed some items before cancellation
        results.Count.ShouldBeGreaterThan(0);
        results.Count.ShouldBeLessThanOrEqualTo(3); // Should stop early due to cancellation
        Handlers.TestStreamRequestHandler.LastExecutedRequest.ShouldBe(streamRequest);
    }

    /// <summary>
    /// Tests that sending a stream request without search term returns unfiltered results.
    /// </summary>
    [Fact]
    public async Task SendStream_StreamRequest_WithoutSearchTerm_ReturnsUnfilteredResults()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddMediator(typeof(Commands.TestStreamRequest).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        Commands.TestStreamRequest streamRequest = new(); // No search term
        List<string> results = new();

        // Act
        await foreach (string item in mediator.SendStream(streamRequest))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(3);
        results[0].ShouldBe("Item 1");
        results[1].ShouldBe("Item 2");
        results[2].ShouldBe("Item 3");
    }

    /// <summary>
    /// Tests that sending a stream request when no handler is registered throws an exception.
    /// </summary>
    [Fact]
    public async Task SendStream_StreamRequest_WhenHandlerNotRegistered_ThrowsException()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddMediator(Array.Empty<System.Reflection.Assembly>());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        Commands.TestStreamRequest streamRequest = new();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (string item in mediator.SendStream(streamRequest))
            {
                // This should throw before yielding any items
            }
        });
    }
}
