using Microsoft.Extensions.DependencyInjection;
using Blazing.Mediator.Statistics;

namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Comprehensive tests for MiddlewareAnalysisExtensions methods.
/// Tests all extension methods for MiddlewareAnalysis including type formatting,
/// order display formatting, and analysis helper methods.
/// </summary>
public class MiddlewareAnalysisExtensionsTests : IDisposable
{
    private readonly ServiceCollection _services;
    private readonly ServiceProvider _serviceProvider;
    private readonly IMiddlewarePipelineInspector _middlewarePipelineInspector;
    private readonly INotificationMiddlewarePipelineInspector _notificationPipelineInspector;

    public MiddlewareAnalysisExtensionsTests()
    {
        _services = new ServiceCollection();
        _services.AddMediator();
        
        _services.AddLogging();
        _serviceProvider = _services.BuildServiceProvider();
        _middlewarePipelineInspector = _serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        _notificationPipelineInspector = _serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
    }

    #region Basic Extension Method Tests

    [Fact]
    public void NormalizeTypeName_WithGenericMiddleware_ShouldFormatCorrectly()
    {
        // Arrange — source-gen does not expose the baked pipeline to IMiddlewarePipelineInspector; construct directly.
        var genericMiddleware = new MiddlewareAnalysis(
            Type: typeof(TestGenericRequestMiddleware<>),
            Order: 200,
            OrderDisplay: "200",
            ClassName: "TestGenericRequestMiddleware",
            TypeParameters: "<TRequest>",
            GenericConstraints: "where TRequest : IRequest",
            Configuration: null);

        // Act
        var formattedName = genericMiddleware.NormalizeTypeName();

        // Assert
        Assert.NotNull(formattedName);
        Assert.DoesNotContain("`", formattedName);
        if (formattedName.Contains('<'))
        {
            Assert.Contains('>', formattedName);
        }
    }

    [Fact]
    public void NormalizeTypeName_WithNonGenericMiddleware_ShouldReturnCleanName()
    {
        // Arrange — source-gen does not expose the baked pipeline to IMiddlewarePipelineInspector; construct directly.
        var nonGenericMiddleware = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        // Act
        var formattedName = nonGenericMiddleware.NormalizeTypeName();

        // Assert
        Assert.NotNull(formattedName);
        Assert.DoesNotContain("`", formattedName);
        Assert.DoesNotContain("<", formattedName);
        Assert.DoesNotContain(">", formattedName);
    }

    [Fact]
    public void GetFullyQualifiedTypeName_ShouldIncludeNamespace()
    {
        // Arrange — source-gen does not expose the baked pipeline to IMiddlewarePipelineInspector; construct directly.
        var middleware = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        // Act
        var fullyQualified = middleware.GetFullyQualifiedTypeName();

        // Assert
        Assert.NotNull(fullyQualified);
        Assert.Contains(".", fullyQualified);
        Assert.DoesNotContain("`", fullyQualified);
    }

    [Fact]
    public void NormalizeClassName_ShouldRemoveGenericSuffix()
    {
        // Arrange — source-gen does not expose the baked pipeline to IMiddlewarePipelineInspector; construct directly.
        var genericMiddleware = new MiddlewareAnalysis(
            Type: typeof(TestGenericRequestMiddleware<>),
            Order: 200,
            OrderDisplay: "200",
            ClassName: "TestGenericRequestMiddleware",
            TypeParameters: "<TRequest>",
            GenericConstraints: "where TRequest : IRequest",
            Configuration: null);

        // Act
        var className = genericMiddleware.NormalizeClassName();

        // Assert
        Assert.NotNull(className);
        Assert.DoesNotContain("`", className);
        Assert.DoesNotContain("<", className);
        Assert.DoesNotContain(">", className);
    }

    [Fact]
    public void NormalizeTypeParameters_WithGenericMiddleware_ShouldFormatCorrectly()
    {
        // Arrange — source-gen does not expose the baked pipeline to IMiddlewarePipelineInspector; construct directly.
        var genericMiddleware = new MiddlewareAnalysis(
            Type: typeof(TestGenericRequestMiddleware<>),
            Order: 200,
            OrderDisplay: "200",
            ClassName: "TestGenericRequestMiddleware",
            TypeParameters: "<TRequest>",
            GenericConstraints: "where TRequest : IRequest",
            Configuration: null);

        // Act
        var typeParameters = genericMiddleware.NormalizeTypeParameters();

        // Assert
        Assert.NotNull(typeParameters);
        if (!string.IsNullOrEmpty(typeParameters))
        {
            Assert.DoesNotContain("`", typeParameters);
            Assert.StartsWith("<", typeParameters);
            Assert.EndsWith(">", typeParameters);
        }
    }

    [Fact]
    public void NormalizeTypeParameters_WithNonGenericMiddleware_ShouldReturnEmpty()
    {
        // Arrange — source-gen does not expose the baked pipeline to IMiddlewarePipelineInspector; construct directly.
        var nonGenericMiddleware = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        // Act
        var typeParameters = nonGenericMiddleware.NormalizeTypeParameters();

        // Assert
        Assert.NotNull(typeParameters);
        Assert.Empty(typeParameters);
    }

    #endregion

    #region Order Display Tests

    [Fact]
    public void NormalizeOrderDisplay_WithSpecialValues_ShouldFormatCorrectly()
    {
        // Test with manually created MiddlewareAnalysis objects for special order values
        var intMinValueAnalysis = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: int.MinValue,
            OrderDisplay: "int.MinValue",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        var intMaxValueAnalysis = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: int.MaxValue,
            OrderDisplay: "int.MaxValue",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        var defaultOrderAnalysis = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: 0,
            OrderDisplay: "0",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        var normalOrderAnalysis = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        // Act & Assert
        Assert.Equal("int.MinValue", intMinValueAnalysis.NormalizeOrderDisplay());
        Assert.Equal("int.MaxValue", intMaxValueAnalysis.NormalizeOrderDisplay());
        Assert.Equal("Default", defaultOrderAnalysis.NormalizeOrderDisplay());
        Assert.Equal("100", normalOrderAnalysis.NormalizeOrderDisplay());
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void GetAssemblyName_ShouldReturnCorrectAssembly()
    {
        // Arrange — source-gen does not expose the baked pipeline to IMiddlewarePipelineInspector; construct directly.
        var middleware = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        // Act
        var assemblyName = middleware.GetAssemblyName();

        // Assert
        Assert.NotNull(assemblyName);
        Assert.NotEqual("Unknown", assemblyName);
        // Should be the test assembly
        Assert.Contains("Tests", assemblyName);
    }

    [Fact]
    public void GetNamespace_ShouldReturnCorrectNamespace()
    {
        // Arrange — source-gen does not expose the baked pipeline to IMiddlewarePipelineInspector; construct directly.
        var middleware = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        // Act
        var namespaceName = middleware.GetNamespace();

        // Assert
        Assert.NotNull(namespaceName);
        Assert.NotEqual("Unknown", namespaceName);
        Assert.Contains("Blazing.Mediator.Tests", namespaceName);
    }

    [Fact]
    public void IsGeneric_ShouldIdentifyGenericMiddleware()
    {
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var genericMiddleware = middlewareAnalysis.FirstOrDefault(m => m.Type.IsGenericType || m.Type.IsGenericTypeDefinition);
        var nonGenericMiddleware = middlewareAnalysis.FirstOrDefault(m => !m.Type.IsGenericType && !m.Type.IsGenericTypeDefinition);

        // Act & Assert
        if (genericMiddleware != null)
        {
            Assert.True(genericMiddleware.IsGeneric());
        }
        
        if (nonGenericMiddleware != null)
        {
            Assert.False(nonGenericMiddleware.IsGeneric());
        }
    }

    [Fact]
    public void GetGenericParameterCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var singleParamMiddleware = middlewareAnalysis.FirstOrDefault(m => 
            m.Type.IsGenericTypeDefinition && m.Type.GetGenericArguments().Length == 1);
        var twoParamMiddleware = middlewareAnalysis.FirstOrDefault(m => 
            m.Type.IsGenericTypeDefinition && m.Type.GetGenericArguments().Length == 2);
        var nonGenericMiddleware = middlewareAnalysis.FirstOrDefault(m => !m.Type.IsGenericType && !m.Type.IsGenericTypeDefinition);

        // Act & Assert
        if (singleParamMiddleware != null)
        {
            Assert.Equal(1, singleParamMiddleware.GetGenericParameterCount());
        }
        
        if (twoParamMiddleware != null)
        {
            Assert.Equal(2, twoParamMiddleware.GetGenericParameterCount());
        }
        
        if (nonGenericMiddleware != null)
        {
            Assert.Equal(0, nonGenericMiddleware.GetGenericParameterCount());
        }
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void HasConfiguration_WithConfiguredMiddleware_ShouldReturnTrue()
    {
        // Create a middleware analysis with configuration
        var configuredAnalysis = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: new { Setting = "TestValue" });

        // Act & Assert
        Assert.True(configuredAnalysis.HasConfiguration());
    }

    [Fact]
    public void HasConfiguration_WithoutConfiguration_ShouldReturnFalse()
    {
        // Arrange — source-gen does not expose the baked pipeline to IMiddlewarePipelineInspector; construct directly.
        var middleware = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        // Act & Assert
        Assert.False(middleware.HasConfiguration()); // Should be false as we didn't configure any
    }

    [Fact]
    public void GetConfigurationTypeName_WithConfiguration_ShouldReturnTypeName()
    {
        // Create a middleware analysis with configuration
        var configuredAnalysis = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: new TestConfiguration { Setting = "TestValue" });

        // Act
        var configTypeName = configuredAnalysis.GetConfigurationTypeName();

        // Assert
        Assert.Equal("TestConfiguration", configTypeName);
    }

    [Fact]
    public void GetConfigurationTypeName_WithoutConfiguration_ShouldReturnNone()
    {
        // Arrange — source-gen does not expose the baked pipeline to IMiddlewarePipelineInspector; construct directly.
        var middleware = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        // Act
        var configTypeName = middleware.GetConfigurationTypeName();

        // Assert
        Assert.Equal("None", configTypeName);
    }

    #endregion

    #region Summary Tests

    [Fact]
    public void NormalizeSummary_WithoutNamespace_ShouldReturnBasicSummary()
    {
        // Arrange — source-gen does not expose the baked pipeline to IMiddlewarePipelineInspector; construct directly.
        var middleware = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        // Act
        var summary = middleware.NormalizeSummary();

        // Assert
        Assert.NotNull(summary);
        Assert.Contains("[", summary);
        Assert.Contains("]", summary);
        Assert.DoesNotContain("`", summary);
        Assert.DoesNotContain("Blazing.Mediator.Tests", summary); // Should not contain namespace
    }

    [Fact]
    public void NormalizeSummary_WithNamespace_ShouldIncludeNamespaceInfo()
    {
        // Arrange — source-gen does not expose the baked pipeline to IMiddlewarePipelineInspector; construct directly.
        var middleware = new MiddlewareAnalysis(
            Type: typeof(TestRequestMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestRequestMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        // Act
        var summary = middleware.NormalizeSummary(includeNamespace: true);

        // Assert
        Assert.NotNull(summary);
        Assert.Contains("[", summary);
        Assert.Contains("]", summary);
        Assert.Contains("(", summary);
        Assert.Contains(")", summary);
        Assert.DoesNotContain("`", summary);
        Assert.Contains("Blazing.Mediator.Tests", summary); // Should contain namespace
    }

    #endregion

    #region Notification Middleware Tests

    [Fact]
    public void NotificationMiddleware_NormalizeGenericConstraints_ShouldFormatCorrectly()
    {
        // Arrange — source-gen does not expose the baked pipeline to INotificationMiddlewarePipelineInspector; construct directly.
        var middleware = new MiddlewareAnalysis(
            Type: typeof(TestNotificationMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestNotificationMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        // Act
        var constraints = middleware.NormalizeGenericConstraints();

        // Assert
        Assert.NotNull(constraints);
        Assert.DoesNotContain("`", constraints);
    }

    [Fact]
    public void NotificationMiddleware_ExtensionMethods_ShouldWorkCorrectly()
    {
        // Arrange — source-gen does not expose the baked pipeline to INotificationMiddlewarePipelineInspector; construct directly.
        var middleware = new MiddlewareAnalysis(
            Type: typeof(TestNotificationMiddleware),
            Order: 100,
            OrderDisplay: "100",
            ClassName: "TestNotificationMiddleware",
            TypeParameters: "",
            GenericConstraints: "",
            Configuration: null);

        // Act & Assert
        Assert.NotNull(middleware);
        
        var formattedTypeName = middleware.NormalizeTypeName();
        Assert.NotNull(formattedTypeName);
        Assert.DoesNotContain("`", formattedTypeName);
        
        var className = middleware.NormalizeClassName();
        Assert.NotNull(className);
        Assert.DoesNotContain("`", className);
        
        var isGeneric = middleware.IsGeneric();
        Assert.True(isGeneric || !isGeneric); // Should not throw
        
        var summary = middleware.NormalizeSummary();
        Assert.NotNull(summary);
        Assert.DoesNotContain("`", summary);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Extensions_WithNullAnalysis_ShouldThrowArgumentNullException()
    {
        // Arrange
        MiddlewareAnalysis? nullAnalysis = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.NormalizeTypeName());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.NormalizeClassName());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.NormalizeTypeParameters());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.NormalizeOrderDisplay());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetAssemblyName());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetNamespace());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.IsGeneric());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetGenericParameterCount());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.NormalizeSummary());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.HasConfiguration());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetConfigurationTypeName());
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Extensions_WithComplexGenericMiddleware_ShouldFormatCorrectly()
    {
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var complexMiddleware = middlewareAnalysis.FirstOrDefault(m => 
            m.Type.IsGenericTypeDefinition && m.Type.GetGenericArguments().Length == 2);

        // Act & Assert
        if (complexMiddleware != null)
        {
            var formattedTypeName = complexMiddleware.NormalizeTypeName();
            Assert.NotNull(formattedTypeName);
            Assert.DoesNotContain("`", formattedTypeName);
            
            var typeParameters = complexMiddleware.NormalizeTypeParameters();
            Assert.NotNull(typeParameters);
            if (!string.IsNullOrEmpty(typeParameters))
            {
                Assert.StartsWith("<", typeParameters);
                Assert.EndsWith(">", typeParameters);
                Assert.Contains(",", typeParameters); // Should have multiple parameters
            }
            
            var summary = complexMiddleware.NormalizeSummary();
            Assert.NotNull(summary);
            Assert.DoesNotContain("`", summary);
        }
    }

    [Fact]
    public void Extensions_WithAllMiddlewareTypes_ShouldFormatConsistently()
    {
        // Arrange
        var requestMiddleware = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var notificationMiddleware = _notificationPipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var allMiddleware = requestMiddleware.Concat(notificationMiddleware);

        // Act & Assert
        foreach (var middleware in allMiddleware)
        {
            // All extension methods should work without throwing
            var formattedTypeName = middleware.NormalizeTypeName();
            var className = middleware.NormalizeClassName();
            var typeParameters = middleware.NormalizeTypeParameters();
            var orderDisplay = middleware.NormalizeOrderDisplay();
            var assemblyName = middleware.GetAssemblyName();
            var namespaceName = middleware.GetNamespace();
            var isGeneric = middleware.IsGeneric();
            var paramCount = middleware.GetGenericParameterCount();
            var summary = middleware.NormalizeSummary();
            var hasConfig = middleware.HasConfiguration();
            var configTypeName = middleware.GetConfigurationTypeName();
            
            // Basic validation
            Assert.NotNull(formattedTypeName);
            Assert.NotNull(className);
            Assert.NotNull(typeParameters);
            Assert.NotNull(orderDisplay);
            Assert.NotNull(assemblyName);
            Assert.NotNull(namespaceName);
            Assert.NotNull(summary);
            Assert.NotNull(configTypeName);
            
            // No backticks in formatted output
            Assert.DoesNotContain("`", formattedTypeName);
            Assert.DoesNotContain("`", className);
            Assert.DoesNotContain("`", typeParameters);
            Assert.DoesNotContain("`", summary);
            
            // Generic parameter count should be consistent with IsGeneric
            if (isGeneric)
            {
                Assert.True(paramCount >= 0);
            }
            else
            {
                Assert.Equal(0, paramCount);
            }
        }
    }

    #endregion

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

#region Test Middleware Classes

public class TestRequestMiddleware : IRequestMiddleware<TestRequest>
{
    public static int Order => 100;
    
    public async ValueTask HandleAsync(TestRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
    }
}

public class TestGenericRequestMiddleware<TRequest> : IRequestMiddleware<TRequest> where TRequest : IRequest
{
    public static int Order => 200;
    
    public async ValueTask HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
    }
}

public class TestTwoParameterMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    public static int Order => 300;
    
    public async ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}

[ExcludeFromAutoDiscovery]
public class TestNotificationMiddleware : INotificationMiddleware
{
    public static int Order => 100;
    
    public async ValueTask InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

[ExcludeFromAutoDiscovery]
public class TestGenericNotificationMiddleware<TNotification> : INotificationMiddleware<TNotification> 
    where TNotification : INotification
{
    public static int Order => 200;
    
    public async ValueTask InvokeAsync(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        await next(notification, cancellationToken);
    }
    
    public async ValueTask InvokeAsync<TNotificationGeneric>(TNotificationGeneric notification, NotificationDelegate<TNotificationGeneric> next, CancellationToken cancellationToken) where TNotificationGeneric : INotification
    {
        if (notification is TNotification typedNotification)
        {
            var typedNext = new NotificationDelegate<TNotification>((n, ct) => next((TNotificationGeneric)(object)n, ct));
            await InvokeAsync(typedNotification, typedNext, cancellationToken);
        }
        else
        {
            await next(notification, cancellationToken);
        }
    }
}

public class TestRequest : IRequest
{
    public string Message { get; set; } = string.Empty;
}

public class TestRequestHandler : IRequestHandler<TestRequest>
{
    public ValueTask Handle(TestRequest request, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}

public class TestConfiguration
{
    public string Setting { get; set; } = string.Empty;
}

#endregion