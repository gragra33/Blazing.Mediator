using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests for the core Mediator functionality
/// </summary>
public class MediatorTests
{
    /// <summary>
    /// Tests that sending a command request calls the appropriate handler.
    /// </summary>
    [Fact]
    public async Task Send_CommandRequest_CallsHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(TestCommand).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new TestCommand { Value = "test" };

        // Act
        await mediator.Send(command);

        // Assert
        TestCommandHandler.LastExecutedCommand.Should().Be(command);
    }

    /// <summary>
    /// Tests that sending a query request returns the expected result.
    /// </summary>
    [Fact]
    public async Task Send_QueryRequest_ReturnsExpectedResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(TestQuery).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new TestQuery { Value = 42 };

        // Act
        var result = await mediator.Send(query);

        // Assert
        result.Should().Be("Result: 42");
    }

    /// <summary>
    /// Tests that sending a command request with a cancellation token passes the token to the handler.
    /// </summary>
    [Fact]
    public async Task Send_CommandRequest_WithCancellationToken_PassesToHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(TestCancellableCommand).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new TestCancellableCommand();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        await mediator.Send(command, cancellationToken);

        // Assert
        TestCancellableCommandHandler.LastCancellationToken.Should().Be(cancellationToken);
    }

    /// <summary>
    /// Tests that sending a query request with a cancellation token passes the token to the handler.
    /// </summary>
    [Fact]
    public async Task Send_QueryRequest_WithCancellationToken_PassesToHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(TestCancellableQuery).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new TestCancellableQuery();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        var result = await mediator.Send(query, cancellationToken);

        // Assert
        TestCancellableQueryHandler.LastCancellationToken.Should().Be(cancellationToken);
        result.Should().Be("Cancellable result");
    }

    /// <summary>
    /// Tests that sending a command request when no handler is registered throws an exception.
    /// </summary>
    [Fact]
    public async Task Send_CommandRequest_WhenHandlerNotRegistered_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(Array.Empty<Assembly>());
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new TestCommand { Value = "test" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(command));
    }

    /// <summary>
    /// Tests that sending a query request when no handler is registered throws an exception.
    /// </summary>
    [Fact]
    public async Task Send_QueryRequest_WhenHandlerNotRegistered_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(Array.Empty<Assembly>());
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new TestQuery { Value = 42 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(query));
    }

    /// <summary>
    /// Tests that sending a command request when handler returns null completes successfully.
    /// </summary>
    [Fact]
    public async Task Send_CommandRequest_WhenHandlerReturnsNull_CompletesSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(TestNullCommand).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new TestNullCommand();

        // Act & Assert - Should not throw
        await mediator.Send(command);
    }

    /// <summary>
    /// Tests that sending a query request when handler returns null throws an exception.
    /// </summary>
    [Fact]
    public async Task Send_QueryRequest_WhenHandlerReturnsNull_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(TestNullQuery).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new TestNullQuery();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(query));
    }
}

// Test Commands and Queries

// Test Handlers