namespace Blazing.Mediator.Abstractions;

/// <summary>
/// Represents detailed analysis information about a middleware component in the pipeline.
/// Provides structured information for debugging, monitoring, and documentation purposes.
/// </summary>
/// <param name="Type">The middleware type.</param>
/// <param name="Order">The numeric execution order of the middleware.</param>
/// <param name="OrderDisplay">The display string for the order (e.g., "int.MinValue", "100").</param>
/// <param name="ClassName">The name of the middleware class without generic suffixes.</param>
/// <param name="TypeParameters">The generic type parameters in angle brackets (e.g., "&lt;TRequest, TResponse&gt;").</param>
/// <param name="GenericConstraints">The generic constraints for each type parameter (e.g., "where TRequest : ICommand&lt;TResponse&gt;").</param>
/// <param name="Configuration">Optional configuration object for the middleware.</param>
public sealed record MiddlewareAnalysis(
    Type Type,
    int Order,
    string OrderDisplay,
    string ClassName,
    string TypeParameters,
    string GenericConstraints,
    object? Configuration);
