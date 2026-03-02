using Blazing.Mediator.SourceGenerators.Generators;
using Blazing.Mediator.SourceGenerators.Tests.Helpers;

namespace Blazing.Mediator.SourceGenerators.Tests.Generators;

/// <summary>
/// Tests for <see cref="IncrementalMediatorGenerator"/> handler discovery and dispatch code generation.
/// Validates that request/command/query handlers are correctly discovered and wrapper code emitted into Mediator.g.cs.
/// </summary>
public class HandlerRegistrationGeneratorTests
{
    [Fact]
    public void Generator_WithSimpleQuery_GeneratesExpectedDispatchCode()
    {
        // Arrange
        var inputSource = GeneratorTestHelper.CreateTestSource(
            GeneratorTestHelper.CommonSources.SimpleQuery);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert - Verify generated code is present and contains expected content
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(generatedSources, "Mediator");
        generatedCode.ShouldNotBeNull();
        generatedCode.ShouldContain("GetUserQuery");
        generatedCode.ShouldContain("GetUserQueryHandler");
        generatedCode.ShouldContain("RequestHandlerWrapper");
    }

    [Fact]
    public void Generator_WithSimpleCommand_GeneratesExpectedDispatchCode()
    {
        // Arrange
        var inputSource = GeneratorTestHelper.CreateTestSource(
            GeneratorTestHelper.CommonSources.SimpleCommand);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(generatedSources, "Mediator");
        generatedCode.ShouldNotBeNull();
        generatedCode.ShouldContain("CreateUserCommand");
        generatedCode.ShouldContain("CreateUserCommandHandler");
    }

    [Fact]
    public void Generator_WithMultipleHandlers_GeneratesCompleteDispatchTable()
    {
        // Arrange - Input with multiple handlers
        var inputSource = GeneratorTestHelper.CreateTestSource("""
            // Query 1
            public record GetUserQuery(int UserId) : IRequest<UserDto>;
            public record UserDto(int Id, string Name);
            public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
            {
                public ValueTask<UserDto> Handle(GetUserQuery request, CancellationToken ct)
                    => Task.FromResult(new UserDto(request.UserId, "Test"));
            }

            // Query 2
            public record GetOrderQuery(int OrderId) : IRequest<OrderDto>;
            public record OrderDto(int Id, decimal Total);
            public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto>
            {
                public ValueTask<OrderDto> Handle(GetOrderQuery request, CancellationToken ct)
                    => Task.FromResult(new OrderDto(request.OrderId, 100m));
            }

            // Command 1
            public record CreateUserCommand(string Name) : IRequest<Guid>;
            public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
            {
                public ValueTask<Guid> Handle(CreateUserCommand request, CancellationToken ct)
                    => Task.FromResult(Guid.NewGuid());
            }

            // Command 2 (void)
            public record DeleteUserCommand(int UserId) : IRequest;
            public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
            {
                public ValueTask Handle(DeleteUserCommand request, CancellationToken ct)
                    => Task.CompletedTask;
            }
            """);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert - Verify all request types in switch expression
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(generatedSources, "Mediator");
        generatedCode.ShouldNotBeNull();
        generatedCode.ShouldContain("GetUserQuery");
        generatedCode.ShouldContain("GetOrderQuery");
        generatedCode.ShouldContain("CreateUserCommand");
        generatedCode.ShouldContain("DeleteUserCommand");
    }

    [Fact]
    public void Generator_WithStreamRequest_GeneratesStreamDispatchCode()
    {
        // Arrange
        var inputSource = GeneratorTestHelper.CreateTestSource(
            GeneratorTestHelper.CommonSources.StreamRequest);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(generatedSources, "Mediator");
        generatedCode.ShouldNotBeNull();
        generatedCode.ShouldContain("IAsyncEnumerable");
        generatedCode.ShouldContain("GetDataStreamQuery");
    }

    [Fact]
    public void Generator_WithNoHandlers_GeneratesEmptyOrNoDispatcher()
    {
        // Arrange - No handlers in source
        var inputSource = GeneratorTestHelper.CreateTestSource("""
            // Empty file - no handlers defined
            public class SomeOtherClass
            {
                public void DoSomething() { }
            }
            """);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert - Generator may or may not emit code when no handlers found
        // This is valid behavior - just verify no exceptions thrown
        compilation.ShouldNotBeNull();
    }

    [Fact]
    public void Generator_WithVoidCommand_GeneratesVoidHandlerDispatch()
    {
        // Arrange - Command with void return (IRequest not IRequest<T>)
        var inputSource = GeneratorTestHelper.CreateTestSource("""
            public record LogMessageCommand(string Message) : IRequest;

            public class LogMessageCommandHandler : IRequestHandler<LogMessageCommand>
            {
                public ValueTask Handle(LogMessageCommand request, CancellationToken ct)
                {
                    Console.WriteLine(request.Message);
                    return ValueTask.CompletedTask;
                }
            }
            """);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(generatedSources, "Mediator");
        generatedCode.ShouldNotBeNull();
        generatedCode.ShouldContain("LogMessageCommand");
    }

    [Fact]
    public void Generator_GeneratesWithProperAttributes()
    {
        // Arrange
        var inputSource = GeneratorTestHelper.CreateTestSource(
            GeneratorTestHelper.CommonSources.SimpleQuery);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert - Verify generated code has proper attributes
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(generatedSources, "Mediator");
        generatedCode.ShouldNotBeNull();
        generatedCode.ShouldContain("GeneratedCode");
        generatedCode.ShouldContain("Blazing.Mediator.SourceGenerators");
        generatedCode.ShouldContain("#nullable enable");
    }

    [Fact]
    public void Generator_GeneratesXmlDocumentation()
    {
        // Arrange
        var inputSource = GeneratorTestHelper.CreateTestSource(
            GeneratorTestHelper.CommonSources.SimpleQuery);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert - Verify XML docs present
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(generatedSources, "Mediator");
        generatedCode.ShouldNotBeNull();
        generatedCode.ShouldContain("/// <summary>");
    }

    [Fact]
    public void Generator_WithOpenGenericHandler_DoesNotGenerateCode()
    {
        // Arrange - Open generic handler (not supported)
        var inputSource = GeneratorTestHelper.CreateTestSource("""
            public class GenericHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
                where TRequest : IRequest<TResponse>
            {
                public ValueTask<TResponse> Handle(TRequest request, CancellationToken ct)
                    => Task.FromResult(default(TResponse)!);
            }
            """);

        // Act
        var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<IncrementalMediatorGenerator>(inputSource);

        // Assert - Open generic handlers should be filtered out
        // Generator should not include them in dispatch table
        var generatedCode = GeneratorTestHelper.GetGeneratedSource(generatedSources, "Mediator");
        
        // Either no code generated, or generated code doesn't contain the generic handler
        if (generatedCode != null)
        {
            generatedCode.ShouldNotContain("GenericHandler");
        }
    }
}
