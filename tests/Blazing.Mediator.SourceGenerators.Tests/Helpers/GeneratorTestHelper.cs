using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection;

namespace Blazing.Mediator.SourceGenerators.Tests.Helpers;

/// <summary>
/// Helper class for testing source generators.
/// Provides common functionality for creating and running generator tests.
/// </summary>
public static class GeneratorTestHelper
{
    /// <summary>
    /// Runs an incremental source generator against input source code and returns the generated sources.
    /// </summary>
    public static (Compilation compilation, ImmutableArray<GeneratedSourceResult> generatedSources) RunGenerator<TGenerator>(string source)
        where TGenerator : IIncrementalGenerator, new()
    {
        var (compilation, syntaxTree) = CreateCompilation(source);

        // Create and run the incremental generator
        var generator = new TGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        
        return (outputCompilation, runResult.GeneratedTrees
            .Select(t => new GeneratedSourceResult { SourceText = t.GetText().ToString(), HintName = t.FilePath })
            .ToImmutableArray());
    }

    /// <summary>
    /// Runs a legacy source generator against input source code and returns the generated sources.
    /// </summary>
    public static (Compilation compilation, ImmutableArray<GeneratedSourceResult> generatedSources) RunLegacyGenerator<TGenerator>(string source)
        where TGenerator : ISourceGenerator, new()
    {
        var (compilation, syntaxTree) = CreateCompilation(source);

        // Create and run the legacy generator
        var generator = new TGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        
        return (outputCompilation, runResult.GeneratedTrees
            .Select(t => new GeneratedSourceResult { SourceText = t.GetText().ToString(), HintName = t.FilePath })
            .ToImmutableArray());
    }

    /// <summary>
    /// Creates a compilation from source code.
    /// </summary>
    private static (CSharpCompilation compilation, SyntaxTree syntaxTree) CreateCompilation(string source)
    {
        // Create a syntax tree from the source
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Create a compilation with the required references
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IMediator).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IRequest<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(INotification).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.IAsyncEnumerable<>).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
            MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return (compilation, syntaxTree);
    }

    /// <summary>
    /// Creates a minimal test source code file with required usings.
    /// </summary>
    /// <param name="typeDefinitions">The type definitions to include.</param>
    /// <returns>A complete source code string.</returns>
    public static string CreateTestSource(string typeDefinitions)
    {
        return $@"using Blazing.Mediator;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

{typeDefinitions}";
    }

    /// <summary>
    /// Extracts generated source by hint name pattern from results.
    /// </summary>
    /// <param name="generatedSources">The generated sources from the test run.</param>
    /// <param name="hintNamePattern">Pattern to match hint name.</param>
    /// <returns>The generated source code or null if not found.</returns>
    public static string? GetGeneratedSource(
        ImmutableArray<GeneratedSourceResult> generatedSources,
        string hintNamePattern)
    {
        foreach (var source in generatedSources)
        {
            if (source.HintName.Contains(hintNamePattern, StringComparison.OrdinalIgnoreCase))
            {
                return source.SourceText;
            }
        }

        return null;
    }

    /// <summary>
    /// Common handler source code for testing.
    /// </summary>
    public static class CommonSources
    {
        public const string SimpleQuery = @"public record GetUserQuery(int UserId) : IRequest<UserDto>;
public record UserDto(int Id, string Name);

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public ValueTask<UserDto> Handle(GetUserQuery request, CancellationToken ct)
        => Task.FromResult(new UserDto(request.UserId, ""Test User""));
}";
        public const string SimpleCommand = @"public record CreateUserCommand(string Name) : IRequest<Guid>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    public ValueTask<Guid> Handle(CreateUserCommand request, CancellationToken ct)
        => Task.FromResult(Guid.NewGuid());
}";
        public const string StreamRequest = @"public record GetDataStreamQuery : IStreamRequest<DataItem>;
public record DataItem(int Id, string Value);

public class GetDataStreamQueryHandler : IStreamRequestHandler<GetDataStreamQuery, DataItem>
{
    public async IAsyncEnumerable<DataItem> Handle(GetDataStreamQuery request, CancellationToken ct)
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new DataItem(i, $""Item {i}"");
        }
    }
}";
        public const string SimpleNotification = @"public record UserCreatedNotification(int UserId, string Name) : INotification;

public class UserCreatedEmailHandler : INotificationHandler<UserCreatedNotification>
{
    public ValueTask Handle(UserCreatedNotification notification, CancellationToken ct)
        => Task.CompletedTask;
}

public class UserCreatedCacheHandler : INotificationHandler<UserCreatedNotification>
{
    public ValueTask Handle(UserCreatedNotification notification, CancellationToken ct)
        => Task.CompletedTask;
}";
        public const string SimpleMiddleware = @"public class LoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 100;

    public Task<TResponse> Invoke(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        Console.WriteLine($""Before: {typeof(TRequest).Name}"");
        var result = next();
        Console.WriteLine($""After: {typeof(TRequest).Name}"");
        return result;
    }
}";
    }
}

/// <summary>
/// Represents a generated source result.
/// </summary>
public record GeneratedSourceResult
{
    public required string SourceText { get; init; }
    public required string HintName { get; init; }
}
