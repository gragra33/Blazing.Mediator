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
        ServiceCollection services = new();
        services.AddMediator(typeof(TestCommand).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestCommand command = new() { Value = "test" };

        // Act
        await mediator.Send(command);

        // Assert
        TestCommandHandler.LastExecutedCommand.ShouldBe(command);
    }

    /// <summary>
    /// Tests that sending a query request returns the expected result.
    /// </summary>
    [Fact]
    public async Task Send_QueryRequest_ReturnsExpectedResult()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddMediator(typeof(TestQuery).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestQuery query = new() { Value = 42 };

        // Act
        string result = await mediator.Send(query);

        // Assert
        result.ShouldBe("Result: 42");
    }

    /// <summary>
    /// Tests that sending a command request with a cancellation token passes the token to the handler.
    /// </summary>
    [Fact]
    public async Task Send_CommandRequest_WithCancellationToken_PassesToHandler()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddMediator(typeof(TestCancellableCommand).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestCancellableCommand command = new();
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await mediator.Send(command, cancellationToken);

        // Assert
        TestCancellableCommandHandler.LastCancellationToken.ShouldBe(cancellationToken);
    }

    /// <summary>
    /// Tests that sending a query request with a cancellation token passes the token to the handler.
    /// </summary>
    [Fact]
    public async Task Send_QueryRequest_WithCancellationToken_PassesToHandler()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddMediator(typeof(TestCancellableQuery).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestCancellableQuery query = new();
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        string result = await mediator.Send(query, cancellationToken);

        // Assert
        TestCancellableQueryHandler.LastCancellationToken.ShouldBe(cancellationToken);
        result.ShouldBe("Cancellable result");
    }

    /// <summary>
    /// Tests that sending a command request when no handler is registered throws an exception.
    /// </summary>
    [Fact]
    public async Task Send_CommandRequest_WhenHandlerNotRegistered_ThrowsException()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddMediator(Array.Empty<Assembly>());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestCommand command = new() { Value = "test" };

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
        ServiceCollection services = new();
        services.AddMediator(Array.Empty<Assembly>());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestQuery query = new() { Value = 42 };

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
        ServiceCollection services = new();
        services.AddMediator(typeof(TestNullCommand).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestNullCommand command = new();

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
        ServiceCollection services = new();
        services.AddMediator(typeof(TestNullQuery).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestNullQuery query = new();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(query));
    }
}

// Test Commands and Queries

// Test Handlers