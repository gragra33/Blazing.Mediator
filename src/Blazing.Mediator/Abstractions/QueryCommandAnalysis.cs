namespace Blazing.Mediator.Abstractions;

/// <summary>
/// Represents the status of handlers for a query or command.
/// </summary>
public enum HandlerStatus
{
    /// <summary>
    /// No handler is registered for this request type.
    /// </summary>
    Missing,
    
    /// <summary>
    /// Exactly one handler is registered (ideal state).
    /// </summary>
    Single,
    
    /// <summary>
    /// Multiple handlers are registered (potential issue).
    /// </summary>
    Multiple
}

/// <summary>
/// Represents analysis information for a query or command type, including handler details.
/// Contains metadata about the type, its response type, assembly information, and handler status.
/// </summary>
/// <param name="Type">The actual Type being analyzed.</param>
/// <param name="ClassName">The name of the class without generic parameters.</param>
/// <param name="TypeParameters">String representation of generic type parameters (e.g., "&lt;T, U&gt;").</param>
/// <param name="Assembly">The name of the assembly containing this type.</param>
/// <param name="Namespace">The namespace of the type.</param>
/// <param name="ResponseType">The response type for queries/commands that return values, null for void commands.</param>
/// <param name="HandlerStatus">The status of handlers for this request type.</param>
/// <param name="HandlerDetails">Detailed information about the handlers.</param>
/// <param name="Handlers">List of handler types registered for this request.</param>
public record QueryCommandAnalysis(
    Type Type,
    string ClassName,
    string TypeParameters,
    string Assembly,
    string Namespace,
    Type? ResponseType,
    HandlerStatus HandlerStatus,
    string HandlerDetails,
    IReadOnlyList<Type> Handlers
);