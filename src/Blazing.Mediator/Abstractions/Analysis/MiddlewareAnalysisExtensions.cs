using Blazing.Mediator.Pipeline;

namespace Blazing.Mediator;

/// <summary>
/// Extension methods for MiddlewareAnalysis to provide normalized type names
/// without backticks and with proper generic type syntax.
/// </summary>
public static class MiddlewareAnalysisExtensions
{
    /// <summary>
    /// Gets a normalized, human-readable type name for the middleware without backticks.
    /// Converts generic type names like "ValidationMiddleware`1[TRequest]" to "ValidationMiddleware&lt;TRequest&gt;".
    /// </summary>
    /// <param name="analysis">The MiddlewareAnalysis instance.</param>
    /// <returns>A normalized middleware type name.</returns>
    public static string NormalizeTypeName(this MiddlewareAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return analysis.Type.FormatTypeName();
    }

    /// <summary>
    /// Gets the fully qualified name of the middleware type with proper generic formatting.
    /// This includes the full namespace and assembly information.
    /// </summary>
    /// <param name="analysis">The MiddlewareAnalysis instance.</param>
    /// <returns>The fully qualified middleware type name.</returns>
    public static string GetFullyQualifiedTypeName(this MiddlewareAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return analysis.Type.FormatTypeNameWithNamespace();
    }

    /// <summary>
    /// Gets a normalized class name without generic backtick suffixes.
    /// Converts "ValidationMiddleware`1" to "ValidationMiddleware".
    /// </summary>
    /// <param name="analysis">The MiddlewareAnalysis instance.</param>
    /// <returns>A clean class name without generic suffixes.</returns>
    public static string NormalizeClassName(this MiddlewareAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return PipelineUtilities.GetCleanTypeName(analysis.Type);
    }

    /// <summary>
    /// Gets normalized type parameters without backticks.
    /// Converts "&lt;TRequest`1, TResponse`1&gt;" to "&lt;TRequest, TResponse&gt;".
    /// </summary>
    /// <param name="analysis">The MiddlewareAnalysis instance.</param>
    /// <returns>A normalized type parameters string.</returns>
    public static string NormalizeTypeParameters(this MiddlewareAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        
        if (!analysis.Type.IsGenericType)
        {
            return string.Empty;
        }

        var genericArgs = analysis.Type.GetGenericArguments();
        var cleanArgNames = genericArgs.Select(t => 
        {
            var name = t.Name;
            // Remove backtick notation
            if (name.Contains('`'))
            {
                var backtickIndex = name.IndexOf('`');
                name = name[..backtickIndex];
            }
            return name;
        });

        return $"<{string.Join(", ", cleanArgNames)}>";
    }

    /// <summary>
    /// Gets normalized generic constraints without backticks.
    /// Converts constraints like "where T : IRequest`1" to "where T : IRequest&lt;TResponse&gt;".
    /// </summary>
    /// <param name="analysis">The MiddlewareAnalysis instance.</param>
    /// <returns>A normalized generic constraints string.</returns>
    public static string NormalizeGenericConstraints(this MiddlewareAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        
        var constraints = PipelineUtilities.GetGenericConstraints(analysis.Type);
        
        // Remove backtick notation from constraints
        if (constraints.Contains('`'))
        {
            constraints = System.Text.RegularExpressions.Regex.Replace(
                constraints, @"`\d+", "");
        }

        return constraints;
    }

    /// <summary>
    /// Gets a normalized order display string.
    /// Converts special order values to readable format (e.g., int.MinValue -> "int.MinValue").
    /// </summary>
    /// <param name="analysis">The MiddlewareAnalysis instance.</param>
    /// <returns>A normalized order display string.</returns>
    public static string NormalizeOrderDisplay(this MiddlewareAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        
        return analysis.Order switch
        {
            int.MinValue => "int.MinValue",
            int.MaxValue => "int.MaxValue",
            0 => "Default",
            _ => analysis.Order.ToString()
        };
    }

    /// <summary>
    /// Gets the assembly name for the middleware type.
    /// </summary>
    /// <param name="analysis">The MiddlewareAnalysis instance.</param>
    /// <returns>The assembly name.</returns>
    public static string GetAssemblyName(this MiddlewareAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return analysis.Type.Assembly.GetName().Name ?? "Unknown";
    }

    /// <summary>
    /// Gets the namespace for the middleware type.
    /// </summary>
    /// <param name="analysis">The MiddlewareAnalysis instance.</param>
    /// <returns>The namespace name.</returns>
    public static string GetNamespace(this MiddlewareAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return analysis.Type.Namespace ?? "Unknown";
    }

    /// <summary>
    /// Checks if the middleware is a generic type.
    /// </summary>
    /// <param name="analysis">The MiddlewareAnalysis instance.</param>
    /// <returns>True if the middleware is generic, false otherwise.</returns>
    public static bool IsGeneric(this MiddlewareAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return analysis.Type.IsGenericType || analysis.Type.IsGenericTypeDefinition;
    }

    /// <summary>
    /// Gets the number of generic parameters for the middleware type.
    /// </summary>
    /// <param name="analysis">The MiddlewareAnalysis instance.</param>
    /// <returns>The number of generic parameters.</returns>
    public static int GetGenericParameterCount(this MiddlewareAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        
        if (analysis.Type.IsGenericType || analysis.Type.IsGenericTypeDefinition)
        {
            return analysis.Type.GetGenericArguments().Length;
        }
        
        return 0;
    }

    /// <summary>
    /// Gets a summary string of the middleware analysis with normalized information.
    /// Useful for display purposes where you want a complete, clean representation.
    /// </summary>
    /// <param name="analysis">The MiddlewareAnalysis instance.</param>
    /// <param name="includeNamespace">Whether to include namespace information in the summary.</param>
    /// <returns>A normalized summary string.</returns>
    public static string NormalizeSummary(this MiddlewareAnalysis analysis, bool includeNamespace = false)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        
        var className = analysis.NormalizeClassName();
        var typeParameters = analysis.NormalizeTypeParameters();
        var orderDisplay = analysis.NormalizeOrderDisplay();
        
        var summary = $"[{orderDisplay}] {className}{typeParameters}";
        
        if (includeNamespace)
        {
            var namespaceName = analysis.GetNamespace();
            var assemblyName = analysis.GetAssemblyName();
            summary += $" ({namespaceName}, {assemblyName})";
        }
        
        return summary;
    }

    /// <summary>
    /// Checks if the middleware has configuration associated with it.
    /// </summary>
    /// <param name="analysis">The MiddlewareAnalysis instance.</param>
    /// <returns>True if configuration exists, false otherwise.</returns>
    public static bool HasConfiguration(this MiddlewareAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return analysis.Configuration != null;
    }

    /// <summary>
    /// Gets the configuration type name if configuration exists.
    /// </summary>
    /// <param name="analysis">The MiddlewareAnalysis instance.</param>
    /// <returns>The configuration type name, or "None" if no configuration exists.</returns>
    public static string GetConfigurationTypeName(this MiddlewareAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        
        if (analysis.Configuration == null)
        {
            return "None";
        }
        
        return analysis.Configuration.GetType().FormatTypeName();
    }
}