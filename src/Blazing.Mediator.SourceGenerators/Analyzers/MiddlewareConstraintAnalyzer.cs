using System.Linq;
using Microsoft.CodeAnalysis;

namespace Blazing.Mediator.SourceGenerators.Analyzers;

/// <summary>
/// Validates type constraints for middleware at compile-time.
/// Implements clarifications from Section 5.2 of Phase3-plan.md.
/// </summary>
internal sealed class MiddlewareConstraintAnalyzer
{
    private readonly Compilation _compilation;

    public MiddlewareConstraintAnalyzer(Compilation compilation)
    {
        _compilation = compilation;
    }

    /// <summary>
    /// Checks if middleware can be applied to a request/response pair.
    /// </summary>
    /// <param name="middlewareType">The middleware type (possibly open generic).</param>
    /// <param name="requestType">The concrete request type.</param>
    /// <param name="responseType">The concrete response type (null for void commands).</param>
    /// <returns>True if constraints are satisfied; otherwise, false.</returns>
    public bool CanApplyMiddleware(
        INamedTypeSymbol middlewareType,
        ITypeSymbol requestType,
        ITypeSymbol? responseType)
    {
        if (!middlewareType.IsGenericType || middlewareType.TypeParameters.Length == 0)
            return true; // Concrete middleware, no constraints to check

        var typeParameters = middlewareType.TypeParameters;

        // Validate TRequest constraints
        if (typeParameters.Length > 0)
        {
            var requestTypeParam = typeParameters[0];
            if (!SatisfiesConstraints(requestTypeParam, requestType))
                return false;
        }

        // Validate TResponse constraints
        if (typeParameters.Length > 1 && responseType != null)
        {
            var responseTypeParam = typeParameters[1];
            if (!SatisfiesConstraints(responseTypeParam, responseType))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a candidate type satisfies all constraints of a type parameter.
    /// </summary>
    private bool SatisfiesConstraints(
        ITypeParameterSymbol typeParameter,
        ITypeSymbol candidateType)
    {
        // Check reference type constraint (class)
        if (typeParameter.HasReferenceTypeConstraint && candidateType.IsValueType)
            return false;

        // Check value type constraint (struct)
        if (typeParameter.HasValueTypeConstraint && !candidateType.IsValueType)
            return false;

        // Check constructor constraint (new())
        if (typeParameter.HasConstructorConstraint)
        {
            if (candidateType is not INamedTypeSymbol namedType)
                return false;

            var hasParameterlessConstructor = namedType.InstanceConstructors
                .Any(c => c.Parameters.IsEmpty && c.DeclaredAccessibility == Accessibility.Public);

            if (!hasParameterlessConstructor)
                return false;
        }

        // Check type constraints (interfaces, base classes)
        foreach (var constraint in typeParameter.ConstraintTypes)
        {
            if (constraint.TypeKind == TypeKind.Interface)
            {
                // Check if candidate implements the interface
                var implementsInterface = candidateType is INamedTypeSymbol namedCandidate &&
                    namedCandidate.AllInterfaces.Any(i =>
                        SymbolEqualityComparer.Default.Equals(
                            i.OriginalDefinition,
                            constraint.OriginalDefinition));

                if (!implementsInterface)
                    return false;
            }
            else if (constraint.TypeKind == TypeKind.Class)
            {
                // Check if candidate inherits from base class
                if (!InheritsFrom(candidateType, constraint))
                    return false;
            }
            else if (constraint.SpecialType == SpecialType.System_ValueType)
            {
                // struct constraint
                if (!candidateType.IsValueType)
                    return false;
            }
            else if (constraint.SpecialType == SpecialType.System_Enum)
            {
                // enum constraint
                if (candidateType.TypeKind != TypeKind.Enum)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if a type inherits from a base type.
    /// </summary>
    private bool InheritsFrom(ITypeSymbol type, ITypeSymbol baseType)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(
                current.OriginalDefinition,
                baseType.OriginalDefinition))
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Gets a human-readable description of why constraints are not satisfied.
    /// </summary>
    public string GetConstraintViolationReason(
        ITypeParameterSymbol typeParameter,
        ITypeSymbol candidateType)
    {
        if (typeParameter.HasReferenceTypeConstraint && candidateType.IsValueType)
            return $"Type parameter '{typeParameter.Name}' has 'class' constraint, but '{candidateType.Name}' is a value type";

        if (typeParameter.HasValueTypeConstraint && !candidateType.IsValueType)
            return $"Type parameter '{typeParameter.Name}' has 'struct' constraint, but '{candidateType.Name}' is a reference type";

        if (typeParameter.HasConstructorConstraint)
        {
            if (candidateType is INamedTypeSymbol namedType)
            {
                var hasParameterlessConstructor = namedType.InstanceConstructors
                    .Any(c => c.Parameters.IsEmpty && c.DeclaredAccessibility == Accessibility.Public);

                if (!hasParameterlessConstructor)
                    return $"Type parameter '{typeParameter.Name}' has 'new()' constraint, but '{candidateType.Name}' does not have a public parameterless constructor";
            }
        }

        foreach (var constraint in typeParameter.ConstraintTypes)
        {
            if (constraint.TypeKind == TypeKind.Interface)
            {
                var implementsInterface = candidateType is INamedTypeSymbol namedCandidate &&
                    namedCandidate.AllInterfaces.Any(i =>
                        SymbolEqualityComparer.Default.Equals(
                            i.OriginalDefinition,
                            constraint.OriginalDefinition));

                if (!implementsInterface)
                    return $"Type parameter '{typeParameter.Name}' requires interface '{constraint.Name}', but '{candidateType.Name}' does not implement it";
            }
            else if (constraint.TypeKind == TypeKind.Class)
            {
                if (!InheritsFrom(candidateType, constraint))
                    return $"Type parameter '{typeParameter.Name}' requires base class '{constraint.Name}', but '{candidateType.Name}' does not inherit from it";
            }
        }

        return "Unknown constraint violation";
    }
}
