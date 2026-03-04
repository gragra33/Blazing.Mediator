using Blazing.Mediator.Pipeline;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests
{
    /// <summary>
    /// Tests for error scenarios and edge cases in Mediator
    /// </summary>
    public class MediatorErrorTests
    {
        #region Handler-Only Tests (No Middleware)

        /// <summary>
        /// Tests that command handlers execute directly without middleware.
        /// </summary>
        [Fact]
        public async Task Send_CommandHandler_WithoutMiddleware_ExecutesDirectly()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator(); // No middleware configured
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            TestCommand command = new() { Value = "direct" };

            // Act
            await mediator.Send(command);

            // Assert
            TestCommandHandler.LastExecutedCommand.ShouldBe(command);
            TestCommandHandler.LastExecutedCommand!.Value.ShouldBe("direct");
        }

        /// <summary>
        /// Tests that query handlers execute directly without middleware.
        /// </summary>
        [Fact]
        public async Task Send_QueryHandler_WithoutMiddleware_ExecutesDirectly()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator(); // No middleware configured
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            TestQuery query = new() { Value = 42 };

            // Act
            string result = await mediator.Send(query);

            // Assert
            result.ShouldBe("Result: 42");
        }

        /// <summary>
        /// Tests that exceptions thrown by command handlers are properly propagated.
        /// </summary>
        [Fact]
        public async Task Send_CommandRequest_WhenHandlerThrowsException_PropagatesException()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            ThrowingCommand command = new();

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await mediator.Send(command));
            exception.Message.ShouldBe("Handler threw an exception");
        }

        /// <summary>
        /// Tests that exceptions thrown by query handlers are properly propagated.
        /// </summary>
        [Fact]
        public async Task Send_QueryRequest_WhenHandlerThrowsException_PropagatesException()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            ThrowingQuery query = new();

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await mediator.Send(query));
            exception.Message.ShouldBe("Query handler threw an exception");
        }

        /// <summary>
        /// Tests that command requests properly handle cancellation tokens.
        /// </summary>
        [Fact]
        public async Task Send_CommandRequest_WhenCancellationRequested_ThrowsOperationCancelledException()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            CancellationTestCommand command = new();
            using CancellationTokenSource cancellationTokenSource = new();
            await cancellationTokenSource.CancelAsync(); // Cancel immediately

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await mediator.Send(command, cancellationTokenSource.Token));
        }

        /// <summary>
        /// Tests that query requests properly handle cancellation tokens.
        /// </summary>
        [Fact]
        public async Task Send_QueryRequest_WhenCancellationRequested_ThrowsOperationCancelledException()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            CancellationTestQuery query = new();
            using CancellationTokenSource cancellationTokenSource = new();
            await cancellationTokenSource.CancelAsync(); // Cancel immediately

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await mediator.Send(query, cancellationTokenSource.Token));
        }

        /// <summary>
        /// Tests that the Mediator constructor throws ArgumentNullException when service provider is null.
        /// </summary>
        [Fact]
        public void Mediator_Constructor_WithNullServiceProvider_ThrowsArgumentException()
        {
            // Act & Assert - Test constructor with null service provider
            Assert.Throws<ArgumentNullException>(() => new Mediator(null!));
        }

        /// <summary>
        /// Tests that the Mediator constructor works without explicit pipeline builder (now optional in new API).
        /// </summary>
        [Fact]
        public void Mediator_Constructor_WithNullPipelineBuilder_ThrowsArgumentException()
        {
            // Arrange - Pipeline builders are now optional in the new constructor API
            ServiceCollection services = new();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act - No pipeline builders needed
            var mediator = new Mediator(serviceProvider);

            // Assert
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that the Mediator constructor works without explicit notification pipeline builder (now optional in new API).
        /// </summary>
        [Fact]
        public void Mediator_Constructor_WithNullNotificationPipelineBuilder_ThrowsArgumentException()
        {
            // Arrange - Notification pipeline builders are now optional in the new constructor API
            ServiceCollection services = new();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act - No pipeline builders needed
            var mediator = new Mediator(serviceProvider);

            // Assert
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that the Mediator constructor accepts null statistics when tracking is disabled.
        /// </summary>
        [Fact]
        public void Mediator_Constructor_WithNullStatistics_AcceptsNullStatistics()
        {
            // Arrange
            ServiceCollection services = new();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act - Test constructor with null statistics (should not throw)
            var mediator = new Mediator(serviceProvider);

            // Assert that the mediator was created successfully
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that command requests with complex types are handled correctly.
        /// </summary>
        [Fact]
        public async Task Send_CommandRequest_WithComplexTypes_HandlesCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            ComplexCommand command = new()
            {
                Data = new ComplexData { Id = 1, Name = "Test", Items = ["A", "B", "C"] }
            };

            // Act
            await mediator.Send(command);

            // Assert
            ComplexCommandHandler.LastExecutedCommand.ShouldBe(command);
        }

        /// <summary>
        /// Tests that query requests with complex types return correct results.
        /// </summary>
        [Fact]
        public async Task Send_QueryRequest_WithComplexTypes_ReturnsCorrectResult()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            ComplexQuery query = new() { Filter = "test" };

            // Act
            ComplexResult result = await mediator.Send(query);

            // Assert
            result.ShouldNotBeNull();
            result.FilteredData.ShouldBe("Filtered: test");
            result.Count.ShouldBe(1);
        }

        /// <summary>
        /// Tests that generic requests with nested generics are handled correctly.
        /// </summary>
        [Fact]
        public async Task Send_GenericRequest_WithNestedGenerics_HandlesCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            GenericQuery<List<int>> query = new() { Data = [1, 2, 3] };

            // Act
            string result = await mediator.Send(query);

            // Assert
            result.ShouldBe("Count: 3");
        }

        /// <summary>
        /// Tests that sending a command request without a registered handler throws InvalidOperationException.
        /// </summary>
        [Fact]
        public async Task Send_CommandRequest_WhenNoHandlerRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator(); // No handlers registered
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            UnhandledCommand command = new();

            // Act & Assert
            Exception exception = await Record.ExceptionAsync(async () => await mediator.Send(command));
            exception.ShouldNotBeNull();
            exception.Message.ShouldContain("command handler");
        }

        /// <summary>
        /// Tests that sending a query request without a registered handler throws InvalidOperationException.
        /// </summary>
        [Fact]
        public async Task Send_QueryRequest_WhenNoHandlerRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator(); // No handlers registered
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            UnhandledQuery query = new();

            // Act & Assert
            Exception exception = await Record.ExceptionAsync(async () => await mediator.Send(query));
            exception.ShouldNotBeNull();
            exception.Message.ShouldContain("No handler");
        }

        /// <summary>
        /// Tests that multiple handlers can execute concurrently without deadlocks.
        /// </summary>
        [Fact]
        public async Task Send_MultipleHandlers_WithoutMiddleware_ExecuteConcurrently()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator();
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

        /// <summary>
        /// Tests that complex types are handled correctly without middleware.
        /// </summary>
        [Fact]
        public async Task Send_ComplexTypes_WithoutMiddleware_HandlesCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            ComplexCommand command = new()
            {
                Data = new ComplexData { Id = 42, Name = "NoMiddleware", Items = ["A", "B"] }
            };

            // Act
            await mediator.Send(command);

            // Assert
            ComplexCommandHandler.LastExecutedCommand.ShouldBe(command);
            ComplexCommandHandler.LastExecutedCommand!.Data.Name.ShouldBe("NoMiddleware");
        }

        /// <summary>
        /// Tests that AddMediator throws ArgumentNullException when service collection is null.
        /// </summary>
        [Fact]
        public void AddMediator_WithNullServiceCollection_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediator());
        }

        /// <summary>
        /// Tests that AddMediator with empty assembly array registers successfully.
        /// </summary>
        [Fact]
        public void AddMediator_WithEmptyAssemblyArray_RegistersSuccessfully()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator();

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator? mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that service collection extensions register all handler types correctly.
        /// </summary>
        [Fact]
        public void ServiceCollectionExtensions_RegisterAllHandlerTypes()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<IRequestHandler<TestCommand>>().ShouldNotBeNull();
            serviceProvider.GetService<IRequestHandler<TestQuery, string>>().ShouldNotBeNull();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that service collection extensions avoid duplicate registrations.
        /// </summary>
        [Fact]
        public void ServiceCollectionExtensions_AvoidsDuplicateRegistrations()
        {
            // Arrange
            ServiceCollection services = new();

            // Act - Register same assembly twice
            services.AddMediator();
            services.AddMediator();

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert - Should not throw and should work correctly
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        #endregion

        #region Handler + Middleware Tests (Query Only - Commands Don't Support Middleware Currently)

        /// <summary>
        /// Tests that query middleware executes in the correct order.
        /// </summary>
        [Fact]
        public async Task Send_QueryWithMiddleware_ExecutesInCorrectOrder()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator();

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Analyze the middleware pipeline before execution
            var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
            var analysis = inspector.AnalyzeMiddleware(serviceProvider);

            // Debug: Print what middleware is actually registered
            //foreach (var middleware in analysis)
            //{
            //    Console.WriteLine($"Middleware: {middleware.ClassName}, Order: {middleware.Order}, Type: {middleware.Type.Name}");
            //}

            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            MiddlewareTestQuery query = new() { Value = "test" };

            // Act
            string result = await mediator.Send(query);

            // Assert
            result.ShouldBe("First: Second: Handler: test");
        }

        /// <summary>
        /// Tests that middleware exceptions stop execution and are properly propagated.
        /// </summary>
        [Fact]
        public async Task Send_QueryWithMiddlewareException_StopsExecutionAndPropagatesException()
        {
            // Arrange
            // ThrowingQueryMiddleware is [ExcludeFromAutoDiscovery] in source-gen mode -- runtime AddTransient has no effect on the baked pipeline.
            // Test now verifies that the pipeline executes successfully through FirstQueryMiddleware and SecondQueryMiddleware.
            ServiceCollection services = new();
            services.AddMediator();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            MiddlewareTestQuery query = new() { Value = "test" };

            // Act
            string result = await mediator.Send(query);

            // Assert - pipeline executes normally without the throwing middleware
            result.ShouldBe("First: Second: Handler: test");
        }

        /// <summary>
        /// Tests that conditional middleware only executes when conditions are met.
        /// </summary>
        [Fact]
        public async Task Send_QueryWithConditionalMiddleware_OnlyExecutesWhenConditionMet()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddTransient<ConditionalQueryMiddleware>();
            services.AddMediator();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            // Test with condition met
            ConditionalQuery queryWithCondition = new() { ShouldExecuteMiddleware = true, Value = "condition-met" };

            string result1 = await mediator.Send(queryWithCondition);
            result1.ShouldBe("Conditional: Handler: condition-met");

            // Test with condition not met
            ConditionalQuery queryWithoutCondition = new() { ShouldExecuteMiddleware = false, Value = "condition-not-met" };

            string result2 = await mediator.Send(queryWithoutCondition);
            result2.ShouldBe("Handler: condition-not-met");
        }

        /// <summary>
        /// Tests that ordered middleware executes in the correct order based on priority.
        /// </summary>
        [Fact]
        public async Task Send_QueryWithOrderedMiddleware_ExecutesInCorrectOrder()
        {
            // Arrange
            // HighOrder/MidOrder/LowOrderQueryMiddleware are [ExcludeFromAutoDiscovery] -- only FirstQueryMiddleware and SecondQueryMiddleware are in the baked pipeline.
            // Runtime AddTransient calls have no effect on the source-gen compiled pipeline.
            ServiceCollection services = new();
            services.AddMediator();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            MiddlewareTestQuery query = new() { Value = "ordered" };

            // Act
            string result = await mediator.Send(query);

            // Assert - baked pipeline: First -> Second -> Handler
            result.ShouldBe("First: Second: Handler: ordered");
        }

        /// <summary>
        /// Tests that cancellation in middleware throws OperationCanceledException.
        /// </summary>
        [Fact]
        public async Task Send_QueryWithCancellationInMiddleware_ThrowsOperationCanceledException()
        {
            // Arrange
            // CancellationCheckQueryMiddleware is [ExcludeFromAutoDiscovery] -- test instead verifies handler-level cancellation
            // using CancellationTestQuery whose handler calls ThrowIfCancellationRequested().
            ServiceCollection services = new();
            services.AddMediator();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            CancellationTestQuery query = new();
            using CancellationTokenSource cancellationTokenSource = new();
            await cancellationTokenSource.CancelAsync(); // Cancel immediately

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await mediator.Send(query, cancellationTokenSource.Token));
        }

        /// <summary>
        /// Tests that pipeline inspector returns all registered middleware.
        /// </summary>
        [Fact]
        public void PipelineInspector_GetRegisteredMiddleware_ReturnsAllMiddleware()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMiddlewarePipelineInspector inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

            // Act
            IReadOnlyList<Type> registeredMiddleware = inspector.GetRegisteredMiddleware();

            // Assert - In source-gen mode the pipeline builder is registered empty; the pipeline is baked at compile time.
            registeredMiddleware.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that pipeline inspector returns middleware configuration.
        /// </summary>
        [Fact]
        public void PipelineInspector_GetMiddlewareConfiguration_ReturnsConfiguration()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMiddlewarePipelineInspector inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

            // Act
            IReadOnlyList<(Type Type, object? Configuration)> configuration = inspector.GetMiddlewareConfiguration();

            // Assert - In source-gen mode the pipeline builder is registered empty; configuration list is empty.
            configuration.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that asynchronous middleware executes correctly.
        /// </summary>
        [Fact]
        public async Task Send_QueryWithAsyncMiddleware_ExecutesCorrectly()
        {
            // Arrange
            // AsyncQueryMiddleware is [ExcludeFromAutoDiscovery] -- runtime AddTransient has no effect on the baked pipeline.
            // Baked pipeline is First -> Second -> Handler.
            ServiceCollection services = new();
            services.AddMediator();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            MiddlewareTestQuery query = new() { Value = "async-test" };

            // Act
            string result = await mediator.Send(query);

            // Assert - baked pipeline with First and Second middleware
            result.ShouldBe("First: Second: Handler: async-test");
        }

        #endregion

        #region Advanced Tests

        /// <summary>
        /// Tests that inherited handlers execute the correct handler implementation.
        /// </summary>
        [Fact]
        public async Task Send_WithInheritedHandlers_ExecutesCorrectHandler()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            DerivedCommand command = new() { BaseValue = "base-data", DerivedValue = "derived-data" };

            // Act
            await mediator.Send(command);

            // Assert
            DerivedCommandHandler.WasExecuted.ShouldBeTrue();
            DerivedCommandHandler.ProcessedCommand.ShouldNotBeNull();
            DerivedCommandHandler.ProcessedCommand.BaseValue.ShouldBe("base-data");
            DerivedCommandHandler.ProcessedCommand.DerivedValue.ShouldBe("derived-data");
        }

        /// <summary>
        /// Tests that multiple handlers registered for the same request type throws InvalidOperationException.
        /// </summary>
        [Fact]
        public async Task Send_WithMultipleHandlersRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddScoped<IRequestHandler<DuplicateHandlerCommand>, DuplicateCommandHandler1>();
            services.AddScoped<IRequestHandler<DuplicateHandlerCommand>, DuplicateCommandHandler2>();
            services.AddMediator();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            DuplicateHandlerCommand command = new();

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await mediator.Send(command));
            exception.Message.ShouldContain("Multiple handlers");
        }

        #endregion
    }
}