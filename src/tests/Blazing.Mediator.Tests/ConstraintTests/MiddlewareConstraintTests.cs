using Microsoft.Extensions.DependencyInjection;
using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Tests.TestMiddleware;
using System.Reflection;

namespace Blazing.Mediator.Tests.ConstraintTests;

/// <summary>
/// Comprehensive tests for generic constraint support in both request and notification middleware.
/// Tests constraint detection, analysis, and enforcement in the middleware pipeline.
/// </summary>
public class MiddlewareConstraintTests
{
    private readonly Assembly _testAssembly = typeof(MiddlewareConstraintTests).Assembly;

    /// <summary>
    /// Test that constraint analysis correctly identifies and formats class constraints.
    /// </summary>
    [Fact]
    public void RequestMiddleware_ClassConstraint_ExtractsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<ClassConstraintMiddleware<,>>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider, isDetailed: true);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBe(1);

        var middleware = analysis.First();
        middleware.ClassName.ShouldBe("ClassConstraintMiddleware");
        middleware.TypeParameters.ShouldBe("<TRequest, TResponse>");
        middleware.GenericConstraints.ShouldContain("where TRequest : class");
        middleware.GenericConstraints.ShouldContain("IRequest<TResponse>");
    }

    /// <summary>
    /// Test that constraint analysis correctly identifies interface constraints.
    /// </summary>
    [Fact]
    public void RequestMiddleware_InterfaceConstraint_ExtractsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<InterfaceConstraintMiddleware<,>>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider, isDetailed: true);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBe(1);

        var middleware = analysis.First();
        middleware.ClassName.ShouldBe("InterfaceConstraintMiddleware");
        middleware.GenericConstraints.ShouldContain("where TRequest : ICommand<TResponse>");
    }

    /// <summary>
    /// Test that constraint analysis correctly identifies new() constraints.
    /// </summary>
    [Fact]
    public void RequestMiddleware_NewConstraint_ExtractsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<NewConstraintMiddleware<,>>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider, isDetailed: true);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBe(1);

        var middleware = analysis.First();
        middleware.ClassName.ShouldBe("NewConstraintMiddleware");
        middleware.GenericConstraints.ShouldContain("new()");
    }

    /// <summary>
    /// Test that constraint analysis correctly identifies multiple constraints.
    /// </summary>
    [Fact]
    public void RequestMiddleware_MultipleConstraints_ExtractsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<MultipleConstraintMiddleware<,>>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider, isDetailed: true);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBe(1);

        var middleware = analysis.First();
        middleware.ClassName.ShouldBe("MultipleConstraintMiddleware");
        middleware.GenericConstraints.ShouldContain("where TRequest : class");
        middleware.GenericConstraints.ShouldContain("IQuery<TResponse>");
        middleware.GenericConstraints.ShouldContain("new()");
    }

    /// <summary>
    /// Test that constraint analysis works for single parameter middleware.
    /// </summary>
    [Fact]
    public void RequestMiddleware_SingleParameter_ExtractsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<SingleParameterConstraintMiddleware<>>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider, isDetailed: true);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBe(1);

        var middleware = analysis.First();
        middleware.ClassName.ShouldBe("SingleParameterConstraintMiddleware");
        middleware.TypeParameters.ShouldBe("<TRequest>");
        middleware.GenericConstraints.ShouldContain("where TRequest : ICommand");
    }

    /// <summary>
    /// Test that notification middleware constraint analysis works correctly.
    /// </summary>
    [Fact]
    public void NotificationMiddleware_Constraints_ExtractsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddNotificationMiddleware<NotificationConstraintMiddleware<>>();
            config.AddNotificationMiddleware<DomainEventNotificationMiddleware>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBe(2);

        var constraintMiddleware = analysis.FirstOrDefault(a => a.ClassName == "NotificationConstraintMiddleware");
        constraintMiddleware.ShouldNotBeNull();
        constraintMiddleware.TypeParameters.ShouldBe("<TNotification>");
        constraintMiddleware.GenericConstraints.ShouldContain("where TNotification : class");
        constraintMiddleware.GenericConstraints.ShouldContain("INotification");

        var domainEventMiddleware = analysis.FirstOrDefault(a => a.ClassName == "DomainEventNotificationMiddleware");
        domainEventMiddleware.ShouldNotBeNull();
        domainEventMiddleware.TypeParameters.ShouldBeEmpty();
        domainEventMiddleware.GenericConstraints.ShouldBeEmpty();
    }

    /// <summary>
    /// Test that constraint analysis respects detailed vs compact mode.
    /// </summary>
    [Fact]
    public void ConstraintAnalysis_DetailedVsCompact_RespectsMode()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<ClassConstraintMiddleware<,>>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var detailedAnalysis = inspector.AnalyzeMiddleware(serviceProvider, isDetailed: true);
        var compactAnalysis = inspector.AnalyzeMiddleware(serviceProvider, isDetailed: false);

        // Assert
        // Detailed mode should include constraints
        detailedAnalysis.First().GenericConstraints.ShouldNotBeEmpty();
        detailedAnalysis.First().TypeParameters.ShouldNotBeEmpty();

        // Compact mode should not include constraints or type parameters
        compactAnalysis.First().GenericConstraints.ShouldBeEmpty();
        compactAnalysis.First().TypeParameters.ShouldBeEmpty();
    }

    /// <summary>
    /// Test that constraint validation works during middleware execution.
    /// </summary>
    [Fact]
    public async Task ConstraintValidation_IncompatibleType_SkipsMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<InterfaceConstraintMiddleware<,>>(); // Requires ICommand<T>
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        // Register a query handler (not a command)
        services.AddScoped<IRequestHandler<TestQuery, string>, TestQueryHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act - This should work because the middleware constraint validation should skip
        // the InterfaceConstraintMiddleware for TestQuery (which implements IRequest<string>, not ICommand<string>)
        var result = await mediator.Send(new TestQuery { Value = 42 });

        // Assert
        result.ShouldBe("Handled: 42");
    }

    /// <summary>
    /// Test that multiple middleware with different constraints work together.
    /// </summary>
    [Fact]
    public void MultipleConstraintMiddleware_DifferentConstraints_AnalyzesAllCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<ClassConstraintMiddleware<,>>();
            config.AddMiddleware<InterfaceConstraintMiddleware<,>>();
            config.AddMiddleware<NewConstraintMiddleware<,>>();
            config.AddMiddleware<MultipleConstraintMiddleware<,>>();
            config.AddMiddleware<SingleParameterConstraintMiddleware<>>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider, isDetailed: true);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBe(5);

        // Verify each middleware has its constraints properly extracted
        var classConstraint = analysis.FirstOrDefault(a => a.ClassName == "ClassConstraintMiddleware");
        classConstraint.ShouldNotBeNull();
        classConstraint.GenericConstraints.ShouldContain("class");

        var interfaceConstraint = analysis.FirstOrDefault(a => a.ClassName == "InterfaceConstraintMiddleware");
        interfaceConstraint.ShouldNotBeNull();
        interfaceConstraint.GenericConstraints.ShouldContain("ICommand<TResponse>");

        var newConstraint = analysis.FirstOrDefault(a => a.ClassName == "NewConstraintMiddleware");
        newConstraint.ShouldNotBeNull();
        newConstraint.GenericConstraints.ShouldContain("new()");

        var multipleConstraint = analysis.FirstOrDefault(a => a.ClassName == "MultipleConstraintMiddleware");
        multipleConstraint.ShouldNotBeNull();
        multipleConstraint.GenericConstraints.ShouldContain("class");
        multipleConstraint.GenericConstraints.ShouldContain("IQuery<TResponse>");
        multipleConstraint.GenericConstraints.ShouldContain("new()");

        var singleParameter = analysis.FirstOrDefault(a => a.ClassName == "SingleParameterConstraintMiddleware");
        singleParameter.ShouldNotBeNull();
        singleParameter.GenericConstraints.ShouldContain("ICommand");
    }

    /// <summary>
    /// Test that constraint analysis output format is consistent.
    /// </summary>
    [Fact]
    public void ConstraintAnalysis_OutputFormat_IsConsistent()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<MultipleConstraintMiddleware<,>>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider, isDetailed: true);

        // Assert
        var middleware = analysis.First();
        
        // Verify constraint format follows expected pattern: "where ParameterName : constraint1, constraint2"
        middleware.GenericConstraints.ShouldStartWith("where TRequest :");
        middleware.GenericConstraints.ShouldContain("class");
        middleware.GenericConstraints.ShouldContain("IQuery<TResponse>");
        middleware.GenericConstraints.ShouldContain("new()");
        
        // Verify constraints are comma-separated
        var constraintPart = middleware.GenericConstraints.Substring("where TRequest : ".Length);
        var constraints = constraintPart.Split(',').Select(c => c.Trim()).ToArray();
        constraints.ShouldContain("class");
        constraints.ShouldContain("IQuery<TResponse>");
        constraints.ShouldContain("new()");
    }
}

/// <summary>
/// Test classes for constraint validation testing.
/// </summary>
public class TestQuery : IRequest<string>
{
    public int Value { get; set; }
}

public class TestQueryHandler : IRequestHandler<TestQuery, string>
{
    public Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Handled: {request.Value}");
    }
}