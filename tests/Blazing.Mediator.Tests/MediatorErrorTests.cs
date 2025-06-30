using Microsoft.Extensions.DependencyInjection;
using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Pipeline;
using System.Reflection;

namespace Blazing.Mediator.Tests;

// Advanced test types for comprehensive coverage
public record GenericConstraintCommand<T> : IRequest where T : class, ITestConstraintEntity 
{ 
    public T Data { get; init; } = default!; 
}

public record DerivedCommand : BaseCommand 
{ 
    public string DerivedValue { get; init; } = string.Empty; 
}

public record BaseCommand : IRequest 
{ 
    public string BaseValue { get; init; } = string.Empty; 
}

public record DuplicateHandlerCommand : IRequest;

public interface ITestConstraintEntity 
{ 
    int Id { get; } 
    string Name { get; } 
}

public class TestConstraintEntity : ITestConstraintEntity 
{ 
    public int Id { get; set; } 
    public string Name { get; set; } = string.Empty; 
}

/// <summary>
/// Tests for error scenarios and edge cases in Mediator
/// </summary>
public class MediatorErrorTests
{
    #region Handler-Only Tests (No Middleware)

    [Fact]
    public async Task Send_CommandHandler_WithoutMiddleware_ExecutesDirectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(typeof(TestCommandHandler).Assembly); // No middleware configured
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestCommand command = new TestCommand { Value = "direct" };

        // Act
        await mediator.Send(command);

        // Assert
        TestCommandHandler.LastExecutedCommand.ShouldBe(command);
        TestCommandHandler.LastExecutedCommand.Value.ShouldBe("direct");
    }

    [Fact]
    public async Task Send_QueryHandler_WithoutMiddleware_ExecutesDirectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(typeof(TestQueryHandler).Assembly); // No middleware configured
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        TestQuery query = new TestQuery { Value = 42 };

        // Act
        string result = await mediator.Send(query);

        // Assert
        result.ShouldBe("Result: 42");
    }

    [Fact]
    public async Task Send_CommandRequest_WhenHandlerThrowsException_PropagatesException()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(typeof(ThrowingCommandHandler).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        ThrowingCommand command = new ThrowingCommand();

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(command));
        exception.Message.ShouldBe("Handler threw an exception");
    }

    [Fact]
    public async Task Send_QueryRequest_WhenHandlerThrowsException_PropagatesException()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(typeof(ThrowingQueryHandler).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        ThrowingQuery query = new ThrowingQuery();

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(query));
        exception.Message.ShouldBe("Query handler threw an exception");
    }

    [Fact]
    public async Task Send_CommandRequest_WhenCancellationRequested_ThrowsOperationCancelledException()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(typeof(CancellationTestCommandHandler).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        CancellationTestCommand command = new CancellationTestCommand();
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            mediator.Send(command, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Send_QueryRequest_WhenCancellationRequested_ThrowsOperationCancelledException()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(typeof(CancellationTestQueryHandler).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        CancellationTestQuery query = new CancellationTestQuery();
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            mediator.Send(query, cancellationTokenSource.Token));
    }

    [Fact]
    public void Mediator_Constructor_WithNullServiceProvider_ThrowsArgumentException()
    {
        // Act & Assert - Test constructor with null service provider
        MiddlewarePipelineBuilder pipelineBuilder = new();
        Assert.Throws<ArgumentNullException>(() => new Mediator(null!, pipelineBuilder));
    }

    [Fact]
    public void Mediator_Constructor_WithNullPipelineBuilder_ThrowsArgumentException()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Test constructor with null pipeline builder
        Assert.Throws<ArgumentNullException>(() => new Mediator(serviceProvider, null!));
    }

    [Fact]
    public async Task Send_CommandRequest_WithComplexTypes_HandlesCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(typeof(ComplexCommandHandler).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        ComplexCommand command = new ComplexCommand
        {
            Data = new ComplexData { Id = 1, Name = "Test", Items = ["A", "B", "C"] }
        };

        // Act
        await mediator.Send(command);

        // Assert
        ComplexCommandHandler.LastExecutedCommand.ShouldBe(command);
    }

    [Fact]
    public async Task Send_QueryRequest_WithComplexTypes_ReturnsCorrectResult()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(typeof(ComplexQueryHandler).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        ComplexQuery query = new ComplexQuery { Filter = "test" };

        // Act
        ComplexResult result = await mediator.Send(query);

        // Assert
        result.ShouldNotBeNull();
        result.FilteredData.ShouldBe("Filtered: test");
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Send_GenericRequest_WithNestedGenerics_HandlesCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(typeof(GenericQueryHandler).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        GenericQuery<List<int>> query = new GenericQuery<List<int>> { Data = [1, 2, 3] };

        // Act
        string result = await mediator.Send(query);

        // Assert
        result.ShouldBe("Count: 3");
    }

    [Fact]
    public async Task Send_CommandRequest_WhenNoHandlerRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator((Assembly[])null!); // No handlers registered
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        UnhandledCommand command = new UnhandledCommand();

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(command));
        exception.Message.ShouldContain("No handler found for request type");
    }

    [Fact]
    public async Task Send_QueryRequest_WhenNoHandlerRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator((Assembly[])null!); // No handlers registered
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        UnhandledQuery query = new UnhandledQuery();

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(query));
        exception.Message.ShouldContain("No handler found for request type");
    }

    [Fact]
    public async Task Send_MultipleHandlers_WithoutMiddleware_ExecuteConcurrently()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(typeof(TestCommandHandler).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            int capturedI = i;
            tasks.Add(Task.Run(async () =>
            {
                var command = new TestCommand { Value = $"concurrent-{capturedI}" };
                await mediator.Send(command);
            }));
        }

        // Act & Assert
        await Task.WhenAll(tasks);
        // Should complete without deadlocks or exceptions
        tasks.Count.ShouldBe(10);
        tasks.ShouldAllBe(t => t.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task Send_ComplexTypes_WithoutMiddleware_HandlesCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(typeof(ComplexCommandHandler).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        ComplexCommand command = new ComplexCommand
        {
            Data = new ComplexData { Id = 42, Name = "NoMiddleware", Items = ["A", "B"] }
        };

        // Act
        await mediator.Send(command);

        // Assert
        ComplexCommandHandler.LastExecutedCommand.ShouldBe(command);
        ComplexCommandHandler.LastExecutedCommand.Data.Name.ShouldBe("NoMiddleware");
    }

    [Fact]
    public void AddMediator_WithNullServiceCollection_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediator((Assembly[])null!));
    }

    [Fact]
    public void AddMediator_WithEmptyAssemblyArray_RegistersSuccessfully()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddMediator((Assembly[])null!);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_FromCallingAssembly_RegistersCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddMediatorFromCallingAssembly();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_FromLoadedAssemblies_RegistersCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddMediatorFromLoadedAssemblies(assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithAssemblyMarkerTypes_RegistersCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(TestCommand), typeof(TestQuery));

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void ServiceCollectionExtensions_RegisterAllHandlerTypes()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(TestCommand).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IRequestHandler<TestCommand>>().ShouldNotBeNull();
        serviceProvider.GetService<IRequestHandler<TestQuery, string>>().ShouldNotBeNull();
        serviceProvider.GetService<IMediator>().ShouldNotBeNull();
    }

    [Fact]
    public void ServiceCollectionExtensions_AvoidsDuplicateRegistrations()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act - Register same assembly twice
        services.AddMediator(typeof(TestCommand).Assembly);
        services.AddMediator(typeof(TestCommand).Assembly);
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert - Should not throw and should work correctly
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    #endregion

    #region Handler + Middleware Tests (Query Only - Commands Don't Support Middleware Currently)

    [Fact]
    public async Task Send_QueryWithMiddleware_ExecutesInCorrectOrder()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstQueryMiddleware>();
            config.AddMiddleware<SecondQueryMiddleware>();
        }, typeof(MiddlewareTestQueryHandler).Assembly);
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        MiddlewareTestQuery query = new MiddlewareTestQuery { Value = "test" };

        // Act
        string result = await mediator.Send(query);

        // Assert
        result.ShouldBe("First: Second: Handler: test");
    }

    [Fact]
    public async Task Send_QueryWithMiddlewareException_StopsExecutionAndPropagatesException()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<ThrowingQueryMiddleware>();
        }, typeof(MiddlewareTestQueryHandler).Assembly);
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        MiddlewareTestQuery query = new MiddlewareTestQuery { Value = "test" };

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(query));
        exception.Message.ShouldBe("Query middleware exception");
    }

    [Fact]
    public async Task Send_QueryWithConditionalMiddleware_OnlyExecutesWhenConditionMet()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<ConditionalQueryMiddleware>();
        }, typeof(ConditionalQueryHandler).Assembly);
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        // Test with condition met
        ConditionalQuery queryWithCondition = new ConditionalQuery { ShouldExecuteMiddleware = true, Value = "condition-met" };
        
        string result1 = await mediator.Send(queryWithCondition);
        result1.ShouldBe("Conditional: Handler: condition-met");

        // Test with condition not met
        ConditionalQuery queryWithoutCondition = new ConditionalQuery { ShouldExecuteMiddleware = false, Value = "condition-not-met" };
        
        string result2 = await mediator.Send(queryWithoutCondition);
        result2.ShouldBe("Handler: condition-not-met");
    }

    [Fact]
    public async Task Send_QueryWithOrderedMiddleware_ExecutesInCorrectOrder()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<HighOrderQueryMiddleware>(); // Order = 100
            config.AddMiddleware<LowOrderQueryMiddleware>();  // Order = 1
            config.AddMiddleware<MidOrderQueryMiddleware>();  // Order = 50
        }, typeof(MiddlewareTestQueryHandler).Assembly);
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        MiddlewareTestQuery query = new MiddlewareTestQuery { Value = "ordered" };

        // Act
        string result = await mediator.Send(query);

        // Assert
        result.ShouldBe("LowOrder: MidOrder: HighOrder: Handler: ordered");
    }

    [Fact]
    public async Task Send_QueryWithCancellationInMiddleware_ThrowsOperationCanceledException()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<CancellationCheckQueryMiddleware>();
        }, typeof(MiddlewareTestQueryHandler).Assembly);
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        MiddlewareTestQuery query = new MiddlewareTestQuery { Value = "test" };
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => mediator.Send(query, cancellationTokenSource.Token));
    }

    [Fact]
    public void PipelineInspector_GetRegisteredMiddleware_ReturnsAllMiddleware()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstQueryMiddleware>();
            config.AddMiddleware<SecondQueryMiddleware>();
        }, typeof(TestQuery).Assembly);
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMiddlewarePipelineInspector inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        IReadOnlyList<Type> registeredMiddleware = inspector.GetRegisteredMiddleware();

        // Assert
        registeredMiddleware.Count.ShouldBe(2);
        registeredMiddleware.ShouldContain(typeof(FirstQueryMiddleware));
        registeredMiddleware.ShouldContain(typeof(SecondQueryMiddleware));
    }

    [Fact]
    public void PipelineInspector_GetMiddlewareConfiguration_ReturnsConfiguration()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstQueryMiddleware>();
        }, typeof(TestQuery).Assembly);
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMiddlewarePipelineInspector inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        IReadOnlyDictionary<Type, object?> configuration = inspector.GetMiddlewareConfiguration();

        // Assert
        configuration.ShouldContainKey(typeof(FirstQueryMiddleware));
    }

    [Fact]
    public async Task Send_QueryWithAsyncMiddleware_ExecutesCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<AsyncQueryMiddleware>();
        }, typeof(MiddlewareTestQueryHandler).Assembly);
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        MiddlewareTestQuery query = new MiddlewareTestQuery { Value = "async-test" };

        // Act
        string result = await mediator.Send(query);

        // Assert
        result.ShouldBe("Async: Handler: async-test");
    }

    #endregion

    #region Advanced Tests

    [Fact]
    public async Task Send_WithGenericConstraints_HandlesCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(typeof(GenericConstraintHandler).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        GenericConstraintCommand<TestConstraintEntity> command = new() { Data = new TestConstraintEntity { Id = 42, Name = "Generic Test" } };

        // Act
        await mediator.Send(command);

        // Assert
        GenericConstraintHandler.LastProcessedEntity.ShouldNotBeNull();
        GenericConstraintHandler.LastProcessedEntity.Id.ShouldBe(42);
        GenericConstraintHandler.LastProcessedEntity.Name.ShouldBe("Generic Test");
    }

    [Fact]
    public async Task Send_WithInheritedHandlers_ExecutesCorrectHandler()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddMediator(typeof(DerivedCommandHandler).Assembly);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        DerivedCommand command = new() { BaseValue = "base-data", DerivedValue = "derived-data" };

        // Act
        await mediator.Send(command);

        // Assert
        DerivedCommandHandler.WasExecuted.ShouldBeTrue();
        DerivedCommandHandler.ProcessedCommand.ShouldBe(command);
        DerivedCommandHandler.ProcessedCommand.BaseValue.ShouldBe("base-data");
        DerivedCommandHandler.ProcessedCommand.DerivedValue.ShouldBe("derived-data");
    }

    [Fact]
    public async Task Send_WithMultipleHandlersRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddScoped<IRequestHandler<DuplicateHandlerCommand>, DuplicateCommandHandler1>();
        services.AddScoped<IRequestHandler<DuplicateHandlerCommand>, DuplicateCommandHandler2>();
        services.AddMediator((Assembly[])null!);
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        DuplicateHandlerCommand command = new();

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(command));
        exception.Message.ShouldContain("Multiple handlers");
    }

    #endregion
}

// Test types for comprehensive coverage
public record UnhandledCommand : IRequest;
public record UnhandledQuery : IRequest<string>;

public record MiddlewareTestQuery : IRequest<string> 
{ 
    public string Value { get; init; } = string.Empty; 
}

public record ConditionalQuery : IRequest<string>
{ 
    public bool ShouldExecuteMiddleware { get; init; }
    public string Value { get; init; } = string.Empty;
}

// Query middleware implementations (only queries support middleware in current architecture)
public class FirstQueryMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        string result = await next();
        return $"First: {result}";
    }
}

public class SecondQueryMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        string result = await next();
        return $"Second: {result}";
    }
}

public class ThrowingQueryMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    public Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Query middleware exception");
    }
}

public class ConditionalQueryMiddleware : IConditionalMiddleware<ConditionalQuery, string>
{
    public bool ShouldExecute(ConditionalQuery request)
    {
        return request.ShouldExecuteMiddleware;
    }

    public async Task<string> HandleAsync(ConditionalQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        string result = await next();
        return $"Conditional: {result}";
    }
}

public class LowOrderQueryMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    public int Order => 1;
    
    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        string result = await next();
        return $"LowOrder: {result}";
    }
}

public class MidOrderQueryMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    public int Order => 50;
    
    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        string result = await next();
        return $"MidOrder: {result}";
    }
}

public class HighOrderQueryMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    public int Order => 100;
    
    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        string result = await next();
        return $"HighOrder: {result}";
    }
}

public class CancellationCheckQueryMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await next();
    }
}

public class AsyncQueryMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        string result = await next();
        return $"Async: {result}";
    }
}

// Handler implementations
public class MiddlewareTestQueryHandler : IRequestHandler<MiddlewareTestQuery, string>
{
    public Task<string> Handle(MiddlewareTestQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Handler: {request.Value}");
    }
}

public class ConditionalQueryHandler : IRequestHandler<ConditionalQuery, string>
{
    public Task<string> Handle(ConditionalQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Handler: {request.Value}");
    }
}

public class GenericConstraintHandler : IRequestHandler<GenericConstraintCommand<TestConstraintEntity>>
{
    public static TestConstraintEntity LastProcessedEntity { get; set; } = default!;

    public Task Handle(GenericConstraintCommand<TestConstraintEntity> request, CancellationToken cancellationToken = default)
    {
        LastProcessedEntity = request.Data;
        return Task.CompletedTask;
    }
}

public class DerivedCommandHandler : IRequestHandler<DerivedCommand>
{
    public static bool WasExecuted;
    public static DerivedCommand ProcessedCommand = null!;

    public Task Handle(DerivedCommand request, CancellationToken cancellationToken = default)
    {
        WasExecuted = true;
        ProcessedCommand = request;
        return Task.CompletedTask;
    }
}

public class DuplicateCommandHandler1 : IRequestHandler<DuplicateHandlerCommand>
{
    public Task Handle(DuplicateHandlerCommand request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public class DuplicateCommandHandler2 : IRequestHandler<DuplicateHandlerCommand>
{
    public Task Handle(DuplicateHandlerCommand request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}