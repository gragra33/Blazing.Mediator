using Microsoft.CodeAnalysis;

namespace Blazing.Mediator.SourceGenerators.Diagnostics;

/// <summary>
/// Diagnostic descriptors for all source generator diagnostics.
/// </summary>
public static class DiagnosticDescriptors
{
    private const string Category = "Blazing.Mediator";
    
    /// <summary>
    /// Error: BLAZMED001 - Open generic handler detected.
    /// </summary>
    public static readonly DiagnosticDescriptor OpenGenericHandler = new(
        id: DiagnosticIds.OpenGenericHandlerDetected,
        title: "Open Generic Handler Detected",
        messageFormat: "The handler '{0}' is an open generic type. Source generation does not support open generic handlers.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Open generic handlers must be closed generic types for source generation.");
    
    /// <summary>
    /// Warning: BLAZMED002 - Telemetry configuration missing.
    /// </summary>
    public static readonly DiagnosticDescriptor TelemetryConfigurationMissing = new(
        id: DiagnosticIds.TelemetryConfigurationMissing,
        title: "Telemetry Configuration Missing",
        messageFormat: "Telemetry is enabled but no telemetry sink is registered. Consider adding OpenTelemetry or Application Insights.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Telemetry requires a sink to be useful.");
    
    /// <summary>
    /// Info: BLAZMED003 - Successful execution.
    /// </summary>
    public static readonly DiagnosticDescriptor SuccessfulExecution = new(
        id: DiagnosticIds.SuccessfulExecution,
        title: "Source Generation Successful",
        messageFormat: "Blazing.Mediator source generation completed successfully. Generated {0} handlers, {1} middleware, {2} notification handlers.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Source generation completed without errors.");
    
    /// <summary>
    /// Info: BLAZMED004 - No handlers found.
    /// </summary>
    public static readonly DiagnosticDescriptor NoHandlersFound = new(
        id: DiagnosticIds.NoHandlersFound,
        title: "No Handlers Found",
        messageFormat: "No {0} found in the current compilation. Source generation will be skipped.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "No handlers were discovered for source generation.");
    
    /// <summary>
    /// Warning: BLAZMED013 - Invalid middleware type constraints.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidMiddlewareConstraints = new(
        id: DiagnosticIds.InvalidMiddlewareConstraints,
        title: "Invalid Middleware Type Constraints",
        messageFormat: "Middleware '{0}' has type parameter '{1}' with constraint '{2}' that is not satisfied by type '{3}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The middleware cannot be applied to this request type due to generic type constraint violations.");
    
    /// <summary>
    /// Warning: BLAZMED014 - Missing middleware order property.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingMiddlewareOrder = new(
        id: DiagnosticIds.MissingMiddlewareOrder,
        title: "Missing Middleware Order",
        messageFormat: "Middleware '{0}' does not have an Order property. Default order (0) will be used.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Middleware should have an Order property to control execution sequence.");
    
    /// <summary>
    /// Warning: BLAZMED015 - Missing subscriber registration.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingSubscriberRegistration = new(
        id: DiagnosticIds.MissingSubscriberRegistration,
        title: "Missing Subscriber Registration",
        messageFormat: "Subscriber '{0}' is not registered in DI container",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Subscribers should be registered in the DI container.");
    
    /// <summary>
    /// Error: BLAZMED016 - Subscriber resolution failure.
    /// </summary>
    public static readonly DiagnosticDescriptor SubscriberResolutionFailure = new(
        id: DiagnosticIds.SubscriberResolutionFailure,
        title: "Subscriber Resolution Failure",
        messageFormat: "Cannot resolve subscriber '{0}' from DI container",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Subscriber resolution failed during source generation.");
    
    /// <summary>
    /// Warning: BLAZMED017 - Missing stream middleware.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingStreamMiddleware = new(
        id: DiagnosticIds.MissingStreamMiddleware,
        title: "Missing Stream Middleware",
        messageFormat: "Stream request '{0}' has no middleware registered",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Stream requests should have middleware for proper error handling and logging.");
    
    /// <summary>
    /// Error: BLAZMED018 - Stream middleware resolution failure.
    /// </summary>
    public static readonly DiagnosticDescriptor StreamMiddlewareResolutionFailure = new(
        id: DiagnosticIds.StreamMiddlewareResolutionFailure,
        title: "Stream Middleware Resolution Failure",
        messageFormat: "Cannot resolve stream middleware '{0}' from DI container",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Stream middleware resolution failed during source generation.");
    
    /// <summary>
    /// Warning: BLAZMED019 - AOT trimming issue.
    /// </summary>
    public static readonly DiagnosticDescriptor AOTTrimmingIssue = new(
        id: DiagnosticIds.AOTTrimmingIssue,
        title: "AOT Trimming Issue",
        messageFormat: "Type '{0}' may be trimmed in AOT scenarios. Consider adding DynamicallyAccessedMembers attribute.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Type may be removed by the trimmer in AOT compilation.");
    
    /// <summary>
    /// Warning: BLAZMED020 - AOT validation missing.
    /// </summary>
    public static readonly DiagnosticDescriptor AOTValidationMissing = new(
        id: DiagnosticIds.AOTValidationMissing,
        title: "AOT Validation Missing",
        messageFormat: "AOT compatibility attributes are not applied to '{0}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Consider adding RequiresUnreferencedCode or DynamicallyAccessedMembers attributes for AOT compatibility.");
    
    /// <summary>
    /// Error: BLAZMED021 - Benchmark validation failed.
    /// </summary>
    public static readonly DiagnosticDescriptor BenchmarkValidationFailed = new(
        id: DiagnosticIds.BenchmarkValidationFailed,
        title: "Benchmark Validation Failed",
        messageFormat: "Performance target not met: {0}. Expected: {1}, Actual: {2}.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: false, // Opt-in for benchmark builds
        description: "Generated code did not meet performance targets.");
}
