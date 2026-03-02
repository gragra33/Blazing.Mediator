namespace Blazing.Mediator.SourceGenerators.Diagnostics;

/// <summary>
/// Centralized diagnostic IDs for all source generator diagnostics.
/// </summary>
public static class DiagnosticIds
{
    // Errors (prevent compilation)
    public const string OpenGenericHandlerDetected = "BLAZMED001";
    public const string SubscriberResolutionFailure = "BLAZMED016";
    public const string StreamMiddlewareResolutionFailure = "BLAZMED018";
    public const string BenchmarkValidationFailed = "BLAZMED021";
    
    // Warnings (suggest fixes)
    public const string TelemetryConfigurationMissing = "BLAZMED002";
    public const string InvalidMiddlewareConstraints = "BLAZMED013";
    public const string MissingMiddlewareOrder = "BLAZMED014";
    public const string MissingSubscriberRegistration = "BLAZMED015";
    public const string MissingStreamMiddleware = "BLAZMED017";
    public const string AOTTrimmingIssue = "BLAZMED019";
    public const string AOTValidationMissing = "BLAZMED020";
    
    // Info (informational)
    public const string SuccessfulExecution = "BLAZMED003";
    public const string NoHandlersFound = "BLAZMED004";
}
