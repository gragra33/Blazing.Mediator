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
        _services.AddMediator(config =>
        {
            // Add various middleware types for testing
            config.AddMiddleware<TestRequestMiddleware>();
            config.AddMiddleware(typeof(TestGenericRequestMiddleware<>));
            config.AddMiddleware(typeof(TestTwoParameterMiddleware<,>));
            config.AddNotificationMiddleware<TestNotificationMiddleware>();
            config.AddNotificationMiddleware(typeof(TestGenericNotificationMiddleware<>));
        }, typeof(MiddlewareAnalysisExtensionsTests).Assembly);
        
        _services.AddLogging();
        _serviceProvider = _services.BuildServiceProvider();
        _middlewarePipelineInspector = _serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        _notificationPipelineInspector = _serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
    }

    #region Basic Extension Method Tests

    [Fact]
    public void GetFormattedTypeName_WithGenericMiddleware_ShouldFormatCorrectly()
    {
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var genericMiddleware = middlewareAnalysis.FirstOrDefault(m => m.Type.IsGenericType || m.Type.IsGenericTypeDefinition);

        // Act
        var formattedName = genericMiddleware?.GetFormattedTypeName();

        // Assert
        Assert.NotNull(formattedName);
        Assert.DoesNotContain("`", formattedName);
        if (formattedName.Contains('<'))
        {
            Assert.Contains('>', formattedName);
        }
    }

    [Fact]
    public void GetFormattedTypeName_WithNonGenericMiddleware_ShouldReturnCleanName()
    {
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var nonGenericMiddleware = middlewareAnalysis.FirstOrDefault(m => !m.Type.IsGenericType && !m.Type.IsGenericTypeDefinition);

        // Act
        var formattedName = nonGenericMiddleware?.GetFormattedTypeName();

        // Assert
        Assert.NotNull(formattedName);
        Assert.DoesNotContain("`", formattedName);
        Assert.DoesNotContain("<", formattedName);
        Assert.DoesNotContain(">", formattedName);
    }

    [Fact]
    public void GetFullyQualifiedTypeName_ShouldIncludeNamespace()
    {
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var middleware = middlewareAnalysis.FirstOrDefault();

        // Act
        var fullyQualified = middleware?.GetFullyQualifiedTypeName();

        // Assert
        Assert.NotNull(fullyQualified);
        Assert.Contains(".", fullyQualified);
        Assert.DoesNotContain("`", fullyQualified);
    }

    [Fact]
    public void GetFormattedClassName_ShouldRemoveGenericSuffix()
    {
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var genericMiddleware = middlewareAnalysis.FirstOrDefault(m => m.Type.IsGenericType || m.Type.IsGenericTypeDefinition);

        // Act
        var className = genericMiddleware?.GetFormattedClassName();

        // Assert
        Assert.NotNull(className);
        Assert.DoesNotContain("`", className);
        Assert.DoesNotContain("<", className);
        Assert.DoesNotContain(">", className);
    }

    [Fact]
    public void GetFormattedTypeParameters_WithGenericMiddleware_ShouldFormatCorrectly()
    {
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var genericMiddleware = middlewareAnalysis.FirstOrDefault(m => m.Type.IsGenericType || m.Type.IsGenericTypeDefinition);

        // Act
        var typeParameters = genericMiddleware?.GetFormattedTypeParameters();

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
    public void GetFormattedTypeParameters_WithNonGenericMiddleware_ShouldReturnEmpty()
    {
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var nonGenericMiddleware = middlewareAnalysis.FirstOrDefault(m => !m.Type.IsGenericType && !m.Type.IsGenericTypeDefinition);

        // Act
        var typeParameters = nonGenericMiddleware?.GetFormattedTypeParameters();

        // Assert
        Assert.NotNull(typeParameters);
        Assert.Empty(typeParameters);
    }

    #endregion

    #region Order Display Tests

    [Fact]
    public void GetFormattedOrderDisplay_WithSpecialValues_ShouldFormatCorrectly()
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
        Assert.Equal("int.MinValue", intMinValueAnalysis.GetFormattedOrderDisplay());
        Assert.Equal("int.MaxValue", intMaxValueAnalysis.GetFormattedOrderDisplay());
        Assert.Equal("Default", defaultOrderAnalysis.GetFormattedOrderDisplay());
        Assert.Equal("100", normalOrderAnalysis.GetFormattedOrderDisplay());
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void GetAssemblyName_ShouldReturnCorrectAssembly()
    {
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var middleware = middlewareAnalysis.FirstOrDefault();

        // Act
        var assemblyName = middleware?.GetAssemblyName();

        // Assert
        Assert.NotNull(assemblyName);
        Assert.NotEqual("Unknown", assemblyName);
        // Should be the test assembly
        Assert.Contains("Tests", assemblyName);
    }

    [Fact]
    public void GetNamespace_ShouldReturnCorrectNamespace()
    {
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var middleware = middlewareAnalysis.FirstOrDefault();

        // Act
        var namespaceName = middleware?.GetNamespace();

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
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var middleware = middlewareAnalysis.FirstOrDefault();

        // Act & Assert
        Assert.NotNull(middleware);
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
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var middleware = middlewareAnalysis.FirstOrDefault();

        // Act
        var configTypeName = middleware?.GetConfigurationTypeName();

        // Assert
        Assert.Equal("None", configTypeName);
    }

    #endregion

    #region Summary Tests

    [Fact]
    public void GetFormattedSummary_WithoutNamespace_ShouldReturnBasicSummary()
    {
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var middleware = middlewareAnalysis.FirstOrDefault();

        // Act
        var summary = middleware?.GetFormattedSummary();

        // Assert
        Assert.NotNull(summary);
        Assert.Contains("[", summary);
        Assert.Contains("]", summary);
        Assert.DoesNotContain("`", summary);
        Assert.DoesNotContain("Blazing.Mediator.Tests", summary); // Should not contain namespace
    }

    [Fact]
    public void GetFormattedSummary_WithNamespace_ShouldIncludeNamespaceInfo()
    {
        // Arrange
        var middlewareAnalysis = _middlewarePipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var middleware = middlewareAnalysis.FirstOrDefault();

        // Act
        var summary = middleware?.GetFormattedSummary(includeNamespace: true);

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
    public void NotificationMiddleware_GetFormattedGenericConstraints_ShouldFormatCorrectly()
    {
        // Arrange
        var middlewareAnalysis = _notificationPipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var middleware = middlewareAnalysis.FirstOrDefault();

        // Act
        var constraints = middleware?.GetFormattedGenericConstraints();

        // Assert
        Assert.NotNull(constraints);
        Assert.DoesNotContain("`", constraints);
    }

    [Fact]
    public void NotificationMiddleware_ExtensionMethods_ShouldWorkCorrectly()
    {
        // Arrange
        var middlewareAnalysis = _notificationPipelineInspector.AnalyzeMiddleware(_serviceProvider);
        var middleware = middlewareAnalysis.FirstOrDefault();

        // Act & Assert
        Assert.NotNull(middleware);
        
        var formattedTypeName = middleware.GetFormattedTypeName();
        Assert.NotNull(formattedTypeName);
        Assert.DoesNotContain("`", formattedTypeName);
        
        var className = middleware.GetFormattedClassName();
        Assert.NotNull(className);
        Assert.DoesNotContain("`", className);
        
        var isGeneric = middleware.IsGeneric();
        Assert.True(isGeneric || !isGeneric); // Should not throw
        
        var summary = middleware.GetFormattedSummary();
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
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetFormattedTypeName());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetFormattedClassName());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetFormattedTypeParameters());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetFormattedOrderDisplay());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetAssemblyName());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetNamespace());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.IsGeneric());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetGenericParameterCount());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetFormattedSummary());
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
            var formattedTypeName = complexMiddleware.GetFormattedTypeName();
            Assert.NotNull(formattedTypeName);
            Assert.DoesNotContain("`", formattedTypeName);
            
            var typeParameters = complexMiddleware.GetFormattedTypeParameters();
            Assert.NotNull(typeParameters);
            if (!string.IsNullOrEmpty(typeParameters))
            {
                Assert.StartsWith("<", typeParameters);
                Assert.EndsWith(">", typeParameters);
                Assert.Contains(",", typeParameters); // Should have multiple parameters
            }
            
            var summary = complexMiddleware.GetFormattedSummary();
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
            var formattedTypeName = middleware.GetFormattedTypeName();
            var className = middleware.GetFormattedClassName();
            var typeParameters = middleware.GetFormattedTypeParameters();
            var orderDisplay = middleware.GetFormattedOrderDisplay();
            var assemblyName = middleware.GetAssemblyName();
            var namespaceName = middleware.GetNamespace();
            var isGeneric = middleware.IsGeneric();
            var paramCount = middleware.GetGenericParameterCount();
            var summary = middleware.GetFormattedSummary();
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
    
    public async Task HandleAsync(TestRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
    }
}

public class TestGenericRequestMiddleware<TRequest> : IRequestMiddleware<TRequest> where TRequest : IRequest
{
    public static int Order => 200;
    
    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
    }
}

public class TestTwoParameterMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    public static int Order => 300;
    
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}

public class TestNotificationMiddleware : INotificationMiddleware
{
    public static int Order => 100;
    
    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

public class TestGenericNotificationMiddleware<TNotification> : INotificationMiddleware<TNotification> 
    where TNotification : INotification
{
    public static int Order => 200;
    
    public async Task InvokeAsync(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        await next(notification, cancellationToken);
    }
    
    public async Task InvokeAsync<TNotificationGeneric>(TNotificationGeneric notification, NotificationDelegate<TNotificationGeneric> next, CancellationToken cancellationToken) where TNotificationGeneric : INotification
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
    public Task Handle(TestRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class TestConfiguration
{
    public string Setting { get; set; } = string.Empty;
}

#endregion