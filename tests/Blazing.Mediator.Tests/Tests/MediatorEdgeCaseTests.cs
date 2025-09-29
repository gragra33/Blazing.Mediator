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
    /// Tests that Send command uses fallback execution when ExecutePipeline method is not found via reflection.
    /// </summary>
    [Fact]
    public async Task Send_Command_WhenExecutePipelineMethodNotFound_UsesFallbackExecution()
    {
        // Arrange - Create a mock pipeline builder that doesn't have ExecutePipeline method
        ServiceCollection services = new();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IMiddlewarePipelineBuilder, MockPipelineBuilderWithoutExecuteMethod>();
        services.AddScoped<INotificationPipelineBuilder, NotificationPipelineBuilder>();
        services.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();
        services.AddSingleton<IStatisticsRenderer, TestStatisticsRenderer>();
        services.AddSingleton<MediatorStatistics>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestCommand command = new();

        // Act & Assert - Should not throw exception and execute fallback
        await mediator.Send(command);
    }

    /// <summary>
    /// Tests that Send query uses fallback execution when ExecutePipeline method is not found via reflection.
    /// </summary>
    [Fact]
    public async Task Send_Query_WhenExecutePipelineMethodNotFound_UsesFallbackExecution()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IMiddlewarePipelineBuilder, MockPipelineBuilderWithoutExecuteMethod>();
        services.AddScoped<INotificationPipelineBuilder, NotificationPipelineBuilder>();
        services.AddScoped<IRequestHandler<TestQuery, string>, TestQueryHandler>();
        services.AddSingleton<IStatisticsRenderer, TestStatisticsRenderer>();
        services.AddSingleton<MediatorStatistics>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestQuery query = new();

        // Act
        string result = await mediator.Send(query);

        // Assert - Should get the actual handler result when fallback is used
        result.ShouldBe("Result: 0");
    }

    /// <summary>
    /// Tests that Send command continues execution when reflection call returns null.
    /// </summary>
    [Fact]
    public async Task Send_Command_WhenReflectionCallReturnsNull_ContinuesExecution()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IMiddlewarePipelineBuilder, MockPipelineBuilderReturningNull>();
        services.AddScoped<INotificationPipelineBuilder, NotificationPipelineBuilder>();
        services.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();
        services.AddSingleton<IStatisticsRenderer, TestStatisticsRenderer>();
        services.AddSingleton<MediatorStatistics>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestCommand command = new();

        // Act & Assert - Should not throw exception
        await mediator.Send(command);
    }

    /// <summary>
    /// Tests that Send query uses fallback when reflection call returns null.
    /// </summary>
    [Fact]
    public async Task Send_Query_WhenReflectionCallReturnsNull_UsesFallback()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IMiddlewarePipelineBuilder, MockPipelineBuilderReturningNull>();
        services.AddScoped<INotificationPipelineBuilder, NotificationPipelineBuilder>();
        services.AddScoped<IRequestHandler<TestQuery, string>, TestQueryHandler>();
        services.AddSingleton<IStatisticsRenderer, TestStatisticsRenderer>();
        services.AddSingleton<MediatorStatistics>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestQuery query = new();

        // Act
        string result = await mediator.Send(query);

        // Assert - Should get the actual handler result when fallback is used
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

        var config = new MediatorConfiguration();
        config.PipelineBuilder.AddMiddleware<MiddlewareWithNoParameterlessConstructor>();

        services.AddSingleton(config);
        services.AddScoped<IMiddlewarePipelineBuilder>(provider =>
            provider.GetRequiredService<MediatorConfiguration>().PipelineBuilder);
        services.AddScoped<IRequestHandler<MiddlewareTestQuery, string>, MiddlewareTestQueryHandler>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var pipelineBuilder = serviceProvider.GetRequiredService<IMiddlewarePipelineBuilder>();

        var request = new MiddlewareTestQuery();
        RequestHandlerDelegate<string> finalHandler = () => Task.FromResult("Handler: test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pipelineBuilder.ExecutePipeline(request, serviceProvider, finalHandler, CancellationToken.None));
    }

    /// <summary>
    /// Tests that ExecutePipeline for commands throws InvalidOperationException when middleware creation fails.
    /// </summary>
    [Fact]
    public async Task ExecutePipeline_Command_WhenMiddlewareCreationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        ServiceCollection services = new();

        var config = new MediatorConfiguration();
        config.PipelineBuilder.AddMiddleware<CommandMiddlewareWithNoParameterlessConstructor>();

        services.AddSingleton(config);
        services.AddScoped<IMiddlewarePipelineBuilder>(provider =>
            provider.GetRequiredService<MediatorConfiguration>().PipelineBuilder);
        services.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var pipelineBuilder = serviceProvider.GetRequiredService<IMiddlewarePipelineBuilder>();

        var request = new TestCommand();
        RequestHandlerDelegate finalHandler = () => Task.CompletedTask;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pipelineBuilder.ExecutePipeline(request, serviceProvider, finalHandler, CancellationToken.None));
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
        RequestHandlerDelegate<string> finalHandler = () => Task.FromResult("test");

        // Act
        var result = builder.Build<TestQuery, string>(serviceProvider, finalHandler);

        // Assert - The returned delegate should throw when called
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => result());
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
        RequestHandlerDelegate finalHandler = () => Task.CompletedTask;

        // Act
        var result = builder.Build<TestCommand>(serviceProvider, finalHandler);

        // Assert - The returned delegate should throw when called
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => result());
        exception.Message.ShouldContain("Use ExecutePipeline method instead for commands");
    }

    /// <summary>
    /// Tests that ExecutePipeline with generic middleware handles type instantiation correctly.
    /// </summary>
    [Fact]
    public async Task ExecutePipeline_WithGenericMiddleware_HandlesTypeInstantiation()
    {
        // Arrange
        ServiceCollection services = new();

        var config = new MediatorConfiguration(services);
        config.PipelineBuilder.AddMiddleware(typeof(GenericMiddleware<,>));

        services.AddSingleton(config);
        services.AddScoped<IMiddlewarePipelineBuilder>(provider =>
            provider.GetRequiredService<MediatorConfiguration>().PipelineBuilder);
        services.AddScoped<IRequestHandler<MiddlewareTestQuery, string>, MiddlewareTestQueryHandler>();
        services.AddScoped(typeof(GenericMiddleware<,>));

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var pipelineBuilder = serviceProvider.GetRequiredService<IMiddlewarePipelineBuilder>();

        var request = new MiddlewareTestQuery();
        RequestHandlerDelegate<string> finalHandler = () => Task.FromResult("Handler: test");

        // Act
        string result = await pipelineBuilder.ExecutePipeline(request, serviceProvider, finalHandler, CancellationToken.None);

        // Assert
        result.ShouldBe("Generic: Handler: test");
    }

    /// <summary>
    /// Tests that ExecutePipeline with middleware ordering exception uses default order.
    /// </summary>
    [Fact]
    public async Task ExecutePipeline_WithMiddlewareOrderingException_UsesDefaultOrder()
    {
        // Arrange
        ServiceCollection services = new();

        var config = new MediatorConfiguration(services);
        config.PipelineBuilder.AddMiddleware<MiddlewareWithExceptionInOrder>();

        services.AddSingleton(config);
        services.AddScoped<IMiddlewarePipelineBuilder>(provider =>
            provider.GetRequiredService<MediatorConfiguration>().PipelineBuilder);
        services.AddScoped<IRequestHandler<MiddlewareTestQuery, string>, MiddlewareTestQueryHandler>();
        services.AddScoped<MiddlewareWithExceptionInOrder>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var pipelineBuilder = serviceProvider.GetRequiredService<IMiddlewarePipelineBuilder>();

        var request = new MiddlewareTestQuery();
        RequestHandlerDelegate<string> finalHandler = () => Task.FromResult("Handler: test");

        // Act
        string result = await pipelineBuilder.ExecutePipeline(request, serviceProvider, finalHandler, CancellationToken.None);

        // Assert
        result.ShouldBe("Exception Order: Handler: test");
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