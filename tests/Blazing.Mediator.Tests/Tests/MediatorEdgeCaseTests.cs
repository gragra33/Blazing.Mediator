using Blazing.Mediator.Configuration;
using Blazing.Mediator.Pipeline;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests for edge cases and error scenarios in the Mediator functionality.
/// Covers various failure modes, null handling, and boundary conditions.
/// </summary>
public class MediatorEdgeCaseTests
{
    /// <summary>
    /// Tests that Send command executes successfully via the source-gen dispatcher
    /// without any IMiddlewarePipelineBuilder registration (which is no longer needed
    /// in source-gen mode where the pipeline is pre-baked at compile time).
    /// </summary>
    [Fact]
    public async Task Send_Command_WithSourceGenDispatcher_ExecutesSuccessfully()
    {
        // Arrange — source-gen discovers all handlers in the test assembly via AddMediator()
        ServiceCollection services = new();
        services.AddMediator();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        TestCommand command = new();

        // Act & Assert — should execute without exception via source-gen dispatcher
        await mediator.Send(command);
    }

    /// <summary>
    /// Tests that Send query returns the expected result via the source-gen dispatcher
    /// without any IMiddlewarePipelineBuilder registration.
    /// </summary>
    [Fact]
    public async Task Send_Query_WithSourceGenDispatcher_ReturnsExpectedResult()
    {
        // Arrange — source-gen discovers all handlers in the test assembly via AddMediator()
        ServiceCollection services = new();
        services.AddMediator();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        TestQuery query = new();

        // Act
        string result = await mediator.Send(query);

        // Assert — handler returns "Result: 0" for the default TestQuery
        result.ShouldBe("Result: 0");
    }

    /// <summary>
    /// Tests that Send command works when IMiddlewarePipelineBuilder is registered but
    /// unused by the source-gen dispatcher path (IMiddlewarePipelineBuilder is optional).
    /// </summary>
    [Fact]
    public async Task Send_Command_WithOptionalMiddlewarePipelineBuilder_ExecutesSuccessfully()
    {
        // Arrange — source-gen discovers all handlers; IMiddlewarePipelineBuilder is optional
        ServiceCollection services = new();
        services.AddMediator();
        // Optionally register IMiddlewarePipelineBuilder — should not affect source-gen dispatch
        services.AddScoped<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        TestCommand command = new();

        // Act & Assert
        await mediator.Send(command);
    }

    /// <summary>
    /// Tests that Send query returns the expected result when an optional IMiddlewarePipelineBuilder
    /// is registered but bypassed by the source-gen dispatcher.
    /// </summary>
    [Fact]
    public async Task Send_Query_WithOptionalMiddlewarePipelineBuilder_ReturnsExpectedResult()
    {
        // Arrange — source-gen discovers all handlers; IMiddlewarePipelineBuilder is optional
        ServiceCollection services = new();
        services.AddMediator();
        services.AddScoped<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        TestQuery query = new();

        // Act
        string result = await mediator.Send(query);

        // Assert
        result.ShouldBe("Result: 0");
    }

    /// <summary>
    /// Tests that MiddlewarePipelineBuilder AddMiddleware with configuration stores the configuration correctly.
    /// </summary>
    [Fact]
    public void MiddlewarePipelineBuilder_AddMiddleware_WithConfiguration_StoresConfiguration()
    {
        // Arrange
        MiddlewarePipelineBuilder builder = new();
        var config = new { Setting = "test" };

        // Act
        builder.AddMiddleware<FirstQueryMiddleware>(config);

        // Assert
        var configurations = builder.GetMiddlewareConfiguration();
        configurations.ShouldContain(config => config.Type == typeof(FirstQueryMiddleware));
        var middlewareConfig = configurations.First(config => config.Type == typeof(FirstQueryMiddleware));
        middlewareConfig.Configuration.ShouldBe(config);
    }

    /// <summary>
    /// Tests that ExecutePipeline throws InvalidOperationException when middleware creation fails.
    /// </summary>
    [Fact]
    public async Task ExecutePipeline_WhenMiddlewareCreationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddScoped<IRequestHandler<MiddlewareTestQuery, string>, MiddlewareTestQueryHandler>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Use MiddlewarePipelineBuilder directly (PipelineBuilder on MediatorConfiguration is null
        // in source-gen mode — pipeline is pre-baked at compile time).
        var pipelineBuilder = new MiddlewarePipelineBuilder();
        pipelineBuilder.AddMiddleware<MiddlewareWithNoParameterlessConstructor>();

        var request = new MiddlewareTestQuery();
        RequestHandlerDelegate<string> finalHandler = () => ValueTask.FromResult("Handler: test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await pipelineBuilder.ExecutePipeline(request, serviceProvider, finalHandler, CancellationToken.None));
    }

    /// <summary>
    /// Tests that ExecutePipeline for commands throws InvalidOperationException when middleware creation fails.
    /// </summary>
    [Fact]
    public async Task ExecutePipeline_Command_WhenMiddlewareCreationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Use MiddlewarePipelineBuilder directly (PipelineBuilder on MediatorConfiguration is null
        // in source-gen mode — pipeline is pre-baked at compile time).
        var pipelineBuilder = new MiddlewarePipelineBuilder();
        pipelineBuilder.AddMiddleware<CommandMiddlewareWithNoParameterlessConstructor>();

        var request = new TestCommand();
        RequestHandlerDelegate finalHandler = () => ValueTask.CompletedTask;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await pipelineBuilder.ExecutePipeline(request, serviceProvider, finalHandler, CancellationToken.None));
    }

    /// <summary>
    /// Tests that Build method for query throws InvalidOperationException.
    /// </summary>
    [Fact]
    public async Task Build_ForQuery_ThrowsInvalidOperationException()
    {
        // Arrange
        MiddlewarePipelineBuilder builder = new();
        ServiceCollection services = new();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        RequestHandlerDelegate<string> finalHandler = () => ValueTask.FromResult("test");

        // Act
        var result = builder.Build<TestQuery, string>(serviceProvider, finalHandler);

        // Assert - The returned delegate should throw when called
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await result());
        exception.Message.ShouldContain("Use ExecutePipeline method instead");
    }

    /// <summary>
    /// Tests that Build method for command throws InvalidOperationException.
    /// </summary>
    [Fact]
    public async Task Build_ForCommand_ThrowsInvalidOperationException()
    {
        // Arrange
        MiddlewarePipelineBuilder builder = new();
        ServiceCollection services = new();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        RequestHandlerDelegate finalHandler = () => ValueTask.CompletedTask;

        // Act
        var result = builder.Build<TestCommand>(serviceProvider, finalHandler);

        // Assert - The returned delegate should throw when called
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await result());
        exception.Message.ShouldContain("Use ExecutePipeline method instead for commands");
    }

    /// <summary>
    /// Simple console renderer for testing purposes
    /// </summary>
    public class TestStatisticsRenderer : IStatisticsRenderer
    {
        public void Render(string message)
        {
            Console.WriteLine(message);
        }
    }
}