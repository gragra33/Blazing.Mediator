using Blazing.Mediator.SourceGenerators.Generators;
using Blazing.Mediator.SourceGenerators.Tests.Helpers;

namespace Blazing.Mediator.SourceGenerators.Tests.Generators;

/// <summary>
/// Tests for MiddlewarePipelineGenerator.
/// Validates middleware discovery and pre-built pipeline generation.
/// NOTE: MiddlewarePipelineGenerator is currently commented out in the source, so these tests are skipped.
/// </summary>
public class MiddlewarePipelineGeneratorTests
{
    [Fact(Skip = "MiddlewarePipelineGenerator is currently commented out pending implementation")]
    public async Task Generator_WithSimpleMiddleware_GeneratesPipelineCode()
    {
        // Arrange
        var inputSource = GeneratorTestHelper.CreateTestSource(
            GeneratorTestHelper.CommonSources.SimpleQuery + "\n" +
            GeneratorTestHelper.CommonSources.SimpleMiddleware);

        // Act
        // var (compilation, generatedSources) = GeneratorTestHelper.RunGenerator<MiddlewarePipelineGenerator>(inputSource);

        // Assert
        // var generatedCode = GeneratorTestHelper.GetGeneratedSource(generatedSources, "MiddlewarePipelines");
        // generatedCode.ShouldNotBeNull();
        // generatedCode.ShouldContain("LoggingMiddleware");

        await Task.CompletedTask;
    }

    [Fact(Skip = "MiddlewarePipelineGenerator is currently commented out pending implementation")]
    public void Generator_WithMultipleMiddleware_GeneratesOrderedPipeline()
    {
        // Test implementation commented out
    }

    [Fact(Skip = "MiddlewarePipelineGenerator is currently commented out pending implementation")]
    public void Generator_WithTypeConstrainedMiddleware_ValidatesConstraints()
    {
        // Test implementation commented out
    }

    [Fact(Skip = "MiddlewarePipelineGenerator is currently commented out pending implementation")]
    public void Generator_WithStreamMiddleware_GeneratesStreamPipeline()
    {
        // Test implementation commented out
    }

    [Fact(Skip = "MiddlewarePipelineGenerator is currently commented out pending implementation")]
    public void Generator_GeneratesHybridComposition()
    {
        // Test implementation commented out
    }
}
