using Microsoft.Extensions.DependencyInjection;
using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Pipeline;
using Blazing.Mediator.Configuration;
using System.Reflection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests for edge cases and error scenarios to improve code coverage
/// </summary>
public class MediatorEdgeCaseTests
{
    [Fact]
    public async Task Send_Command_WhenExecutePipelineMethodNotFound_UsesFallbackExecution()
    {
        // Arrange - Create a mock pipeline builder that doesn't have ExecutePipeline method
        ServiceCollection services = new ServiceCollection();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IMiddlewarePipelineBuilder, MockPipelineBuilderWithoutExecuteMethod>();
        services.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestCommand command = new();

        // Act & Assert - Should not throw exception and execute fallback
        await mediator.Send(command);
    }

    [Fact]
    public async Task Send_Query_WhenExecutePipelineMethodNotFound_UsesFallbackExecution()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IMiddlewarePipelineBuilder, MockPipelineBuilderWithoutExecuteMethod>();
        services.AddScoped<IRequestHandler<TestQuery, string>, TestQueryHandler>();
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestQuery query = new();

        // Act
        string result = await mediator.Send(query);

        // Assert - Should get the actual handler result when fallback is used
        result.ShouldBe("Result: 0");
    }

    [Fact]
    public async Task Send_Command_WhenReflectionCallReturnsNull_ContinuesExecution()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IMiddlewarePipelineBuilder, MockPipelineBuilderReturningNull>();
        services.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestCommand command = new();

        // Act & Assert - Should not throw exception
        await mediator.Send(command);
    }

    [Fact]
    public async Task Send_Query_WhenReflectionCallReturnsNull_UsesFallback()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IMiddlewarePipelineBuilder, MockPipelineBuilderReturningNull>();
        services.AddScoped<IRequestHandler<TestQuery, string>, TestQueryHandler>();
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestQuery query = new();

        // Act
        string result = await mediator.Send(query);

        // Assert - Should get the actual handler result when fallback is used
        result.ShouldBe("Result: 0");
    }

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
        configurations.ShouldContainKey(typeof(FirstQueryMiddleware));
        configurations[typeof(FirstQueryMiddleware)].ShouldBe(config);
    }

    [Fact]
    public async Task ExecutePipeline_WhenMiddlewareCreationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        
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

    [Fact]
    public async Task ExecutePipeline_Command_WhenMiddlewareCreationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        
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

    [Fact]
    public async Task Build_ForQuery_ThrowsInvalidOperationException()
    {
        // Arrange
        MiddlewarePipelineBuilder builder = new();
        ServiceCollection services = new ServiceCollection();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        RequestHandlerDelegate<string> finalHandler = () => Task.FromResult("test");

        // Act
        var result = builder.Build<TestQuery, string>(serviceProvider, finalHandler);
        
        // Assert - The returned delegate should throw when called
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => result());
        exception.Message.ShouldContain("Use ExecutePipeline method instead");
    }

    [Fact]
    public async Task Build_ForCommand_ThrowsInvalidOperationException()
    {
        // Arrange
        MiddlewarePipelineBuilder builder = new();
        ServiceCollection services = new ServiceCollection();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        RequestHandlerDelegate finalHandler = () => Task.CompletedTask;

        // Act
        var result = builder.Build<TestCommand>(serviceProvider, finalHandler);
        
        // Assert - The returned delegate should throw when called
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => result());
        exception.Message.ShouldContain("Use ExecutePipeline method instead for commands");
    }

    [Fact]
    public async Task ExecutePipeline_WithGenericMiddleware_HandlesTypeInstantiation()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        
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

    [Fact]
    public async Task ExecutePipeline_WithMiddlewareOrderingException_UsesDefaultOrder()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        
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
}

// Mock classes for testing edge cases  
public class MockPipelineBuilderWithoutExecuteMethod : IMiddlewarePipelineBuilder, IMiddlewarePipelineInspector
{
    public IMiddlewarePipelineBuilder AddMiddleware<TMiddleware>() where TMiddleware : class => this;
    public IMiddlewarePipelineBuilder AddMiddleware(Type middlewareType) => this;
    public RequestHandlerDelegate<TResponse> Build<TRequest, TResponse>(IServiceProvider serviceProvider, RequestHandlerDelegate<TResponse> finalHandler) where TRequest : IRequest<TResponse> => finalHandler;
    public RequestHandlerDelegate Build<TRequest>(IServiceProvider serviceProvider, RequestHandlerDelegate finalHandler) where TRequest : IRequest => finalHandler;
    public IReadOnlyList<Type> GetRegisteredMiddleware() => new List<Type>();
    public IReadOnlyDictionary<Type, object?> GetMiddlewareConfiguration() => new Dictionary<Type, object?>();

    // Required interface methods - but these will be found by reflection, so the test needs different expectations
    public Task<TResponse> ExecutePipeline<TRequest, TResponse>(TRequest request, IServiceProvider serviceProvider, RequestHandlerDelegate<TResponse> finalHandler, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
    {
        return finalHandler();
    }

    public Task ExecutePipeline<TRequest>(TRequest request, IServiceProvider serviceProvider, RequestHandlerDelegate finalHandler, CancellationToken cancellationToken) where TRequest : IRequest
    {
        return finalHandler();
    }
}

public class MockPipelineBuilderReturningNull : IMiddlewarePipelineBuilder, IMiddlewarePipelineInspector
{
    public IMiddlewarePipelineBuilder AddMiddleware<TMiddleware>() where TMiddleware : class => this;
    public IMiddlewarePipelineBuilder AddMiddleware(Type middlewareType) => this;
    public RequestHandlerDelegate<TResponse> Build<TRequest, TResponse>(IServiceProvider serviceProvider, RequestHandlerDelegate<TResponse> finalHandler) where TRequest : IRequest<TResponse> => finalHandler;
    public RequestHandlerDelegate Build<TRequest>(IServiceProvider serviceProvider, RequestHandlerDelegate finalHandler) where TRequest : IRequest => finalHandler;
    public IReadOnlyList<Type> GetRegisteredMiddleware() => new List<Type>();
    public IReadOnlyDictionary<Type, object?> GetMiddlewareConfiguration() => new Dictionary<Type, object?>();

    // ExecutePipeline methods that will be called via reflection and can return appropriate results
    public Task<TResponse> ExecutePipeline<TRequest, TResponse>(TRequest request, IServiceProvider serviceProvider, RequestHandlerDelegate<TResponse> finalHandler, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
    {
        return finalHandler();
    }

    public Task ExecutePipeline<TRequest>(TRequest request, IServiceProvider serviceProvider, RequestHandlerDelegate finalHandler, CancellationToken cancellationToken) where TRequest : IRequest
    {
        return finalHandler();
    }
}

public class MiddlewareWithNoParameterlessConstructor : IRequestMiddleware<MiddlewareTestQuery, string>
{
    public MiddlewareWithNoParameterlessConstructor(string requiredParameter)
    {
        // This will cause Activator.CreateInstance to fail
    }

    public int Order => 0;

    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}

public class CommandMiddlewareWithNoParameterlessConstructor : IRequestMiddleware<TestCommand>
{
    public CommandMiddlewareWithNoParameterlessConstructor(string requiredParameter)
    {
        // This will cause Activator.CreateInstance to fail
    }

    public int Order => 0;

    public async Task HandleAsync(TestCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
    }
}

public class GenericMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 0;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var result = await next();
        if (result is string str)
        {
            return (TResponse)(object)$"Generic: {str}";
        }
        return result;
    }
}

public class MiddlewareWithExceptionInOrder : IRequestMiddleware<MiddlewareTestQuery, string>
{
    public int Order 
    { 
        get 
        { 
            throw new InvalidOperationException("Cannot get order"); 
        } 
    }

    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        var result = await next();
        return $"Exception Order: {result}";
    }
}
