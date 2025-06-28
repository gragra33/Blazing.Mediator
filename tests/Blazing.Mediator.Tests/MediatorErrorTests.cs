using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests for error scenarios and edge cases in Mediator
/// </summary>
public class MediatorErrorTests
{
    [Fact]
    public async Task Send_CommandRequest_WhenHandlerThrowsException_PropagatesException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(ThrowingCommandHandler).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new ThrowingCommand();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(command));
        exception.Message.Should().Be("Handler threw an exception");
    }

    [Fact]
    public async Task Send_QueryRequest_WhenHandlerThrowsException_PropagatesException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(ThrowingQueryHandler).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new ThrowingQuery();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(query));
        exception.Message.Should().Be("Query handler threw an exception");
    }

    [Fact]
    public async Task Send_CommandRequest_WhenCancellationRequested_ThrowsOperationCancelledException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(CancellationTestCommandHandler).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new CancellationTestCommand();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            mediator.Send(command, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Send_QueryRequest_WhenCancellationRequested_ThrowsOperationCancelledException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(CancellationTestQueryHandler).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new CancellationTestQuery();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            mediator.Send(query, cancellationTokenSource.Token));
    }

    [Fact]
    public void Mediator_Constructor_WithNullServiceProvider_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Mediator(null!));
    }

    [Fact]
    public async Task Send_CommandRequest_WithComplexTypes_HandlesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(ComplexCommandHandler).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new ComplexCommand
        {
            Data = new ComplexData { Id = 1, Name = "Test", Items = ["A", "B", "C"] }
        };

        // Act
        await mediator.Send(command);

        // Assert
        ComplexCommandHandler.LastExecutedCommand.Should().Be(command);
    }

    [Fact]
    public async Task Send_QueryRequest_WithComplexTypes_ReturnsCorrectResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(ComplexQueryHandler).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new ComplexQuery { Filter = "test" };

        // Act
        var result = await mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.FilteredData.Should().Be("Filtered: test");
        result.Count.Should().Be(1);
    }

    [Fact]
    public async Task Send_GenericRequest_WithNestedGenerics_HandlesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(GenericQueryHandler).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new GenericQuery<List<int>> { Data = [1, 2, 3] };

        // Act
        var result = await mediator.Send(query);

        // Assert
        result.Should().Be("Count: 3");
    }
}

// Test commands, queries, and handlers for error scenarios