using Blazing.Mediator.Pipeline;

namespace Blazing.Mediator;

/// <summary>
/// Extension methods for QueryCommandAnalysis to provide normalized type names
/// without backticks and with proper generic type syntax.
/// </summary>
public static class QueryCommandAnalysisExtensions
{
    /// <summary>
    /// Gets a normalized, human-readable name for the response type without backticks.
    /// Converts generic type names like "List`1[User]" to "List<User>".
    /// </summary>
    /// <param name="analysis">The QueryCommandAnalysis instance.</param>
    /// <returns>A normalized response type name, or null if there is no response type.</returns>
    public static string? NormalizeResponseTypeName(this QueryCommandAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return analysis.ResponseType?.FormatTypeName();
    }

    /// <summary>
    /// Gets a normalized, human-readable name for the primary interface without backticks.
    /// Converts interface names like "IQuery<List`1>" to "IQuery<List<User>>".
    /// </summary>
    /// <param name="analysis">The QueryCommandAnalysis instance.</param>
    /// <returns>A normalized primary interface name.</returns>
    public static string NormalizePrimaryInterfaceName(this QueryCommandAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        
        // If PrimaryInterface contains type parameters but they're not properly formatted,
        // we need to reconstruct it from the Type information
        var primaryInterface = analysis.PrimaryInterface;
        
        // Check if this looks like a malformed generic interface name
        if (primaryInterface.Contains('<') && primaryInterface.Contains('`'))
        {
            // Extract the interface name without generic parameters
            var interfaceName = primaryInterface.Substring(0, primaryInterface.IndexOf('<'));
            
            // Find the corresponding interface on the actual type to get proper formatting
            var interfaces = analysis.Type.GetInterfaces();
            
            foreach (var iface in interfaces)
            {
                var formattedInterface = iface.FormatTypeName();
                if (formattedInterface.StartsWith(interfaceName + "<"))
                {
                    return formattedInterface;
                }
            }
        }
        
        return primaryInterface;
    }

    /// <summary>
    /// Gets normalized, human-readable names for all handler types without backticks.
    /// Converts handler names like "GetUsersQueryHandler`1" to "GetUsersQueryHandler<T>".
    /// </summary>
    /// <param name="analysis">The QueryCommandAnalysis instance.</param>
    /// <returns>A list of normalized handler type names.</returns>
    public static IReadOnlyList<string> NormalizeHandlerNames(this QueryCommandAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return analysis.Handlers.Select(h => h.FormatTypeName()).ToList();
    }

    /// <summary>
    /// Gets a normalized handler details string with properly formatted type names.
    /// This is useful for display purposes where you want clean type names.
    /// </summary>
    /// <param name="analysis">The QueryCommandAnalysis instance.</param>
    /// <returns>A normalized handler details string.</returns>
    public static string NormalizeHandlerDetails(this QueryCommandAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        
        return analysis.HandlerStatus switch
        {
            HandlerStatus.Missing => "No handler registered",
            HandlerStatus.Single => analysis.Handlers.Count > 0 ? analysis.Handlers[0].FormatTypeName() : "Handler found",
            HandlerStatus.Multiple => $"{analysis.Handlers.Count} handlers: {string.Join(", ", analysis.NormalizeHandlerNames())}",
            _ => analysis.HandlerDetails
        };
    }

    /// <summary>
    /// Gets the fully qualified name of the response type with proper generic formatting.
    /// This includes the full namespace and assembly information.
    /// </summary>
    /// <param name="analysis">The QueryCommandAnalysis instance.</param>
    /// <returns>The fully qualified response type name, or null if there is no response type.</returns>
    public static string? GetFullyQualifiedResponseTypeName(this QueryCommandAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return analysis.ResponseType?.FormatTypeNameWithNamespace();
    }

    /// <summary>
    /// Gets the fully qualified name of the primary interface with proper generic formatting.
    /// This attempts to reconstruct the proper interface name from the type's actual interfaces.
    /// </summary>
    /// <param name="analysis">The QueryCommandAnalysis instance.</param>
    /// <returns>The fully qualified primary interface name.</returns>
    public static string GetFullyQualifiedPrimaryInterfaceName(this QueryCommandAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        
        var primaryInterface = analysis.PrimaryInterface;
        
        // If this looks like a clean interface name already, try to find the full version
        if (!primaryInterface.Contains('`'))
        {
            var interfaces = analysis.Type.GetInterfaces();
            
            // Look for an interface that matches the primary interface pattern
            foreach (var iface in interfaces)
            {
                var formattedInterface = iface.FormatTypeNameWithNamespace();
                var simpleFormatted = iface.FormatTypeName();
                
                if (simpleFormatted == primaryInterface || formattedInterface.EndsWith("." + primaryInterface))
                {
                    return formattedInterface;
                }
            }
        }
        
        // Fallback to the normalized primary interface name
        return analysis.NormalizePrimaryInterfaceName();
    }

    /// <summary>
    /// Gets normalized, fully qualified names for all handler types.
    /// </summary>
    /// <param name="analysis">The QueryCommandAnalysis instance.</param>
    /// <returns>A list of fully qualified handler type names.</returns>
    public static IReadOnlyList<string> GetFullyQualifiedHandlerNames(this QueryCommandAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return analysis.Handlers.Select(h => h.FormatTypeNameWithNamespace()).ToList();
    }
}

/// <summary>
/// Extension methods for NotificationAnalysis to provide normalized type names
/// without backticks and with proper generic type syntax.
/// </summary>
public static class NotificationAnalysisExtensions
{
    /// <summary>
    /// Gets normalized, human-readable names for all handler types without backticks.
    /// Converts handler names like "OrderCreatedNotificationHandler`1" to "OrderCreatedNotificationHandler<T>".
    /// </summary>
    /// <param name="analysis">The NotificationAnalysis instance.</param>
    /// <returns>A list of normalized handler type names.</returns>
    public static IReadOnlyList<string> NormalizeHandlerNames(this NotificationAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return analysis.Handlers.Select(h => h.FormatTypeName()).ToList();
    }

    /// <summary>
    /// Gets a normalized handler details string with properly formatted type names.
    /// This is useful for display purposes where you want clean type names.
    /// </summary>
    /// <param name="analysis">The NotificationAnalysis instance.</param>
    /// <returns>A normalized handler details string.</returns>
    public static string NormalizeHandlerDetails(this NotificationAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        
        return analysis.HandlerStatus switch
        {
            HandlerStatus.Missing => "No handler registered",
            HandlerStatus.Single => analysis.Handlers.Count > 0 ? analysis.Handlers[0].FormatTypeName() : "Handler found",
            HandlerStatus.Multiple => $"{analysis.Handlers.Count} handlers: {string.Join(", ", analysis.NormalizeHandlerNames())}",
            _ => analysis.HandlerDetails
        };
    }

    /// <summary>
    /// Gets normalized, fully qualified names for all handler types.
    /// </summary>
    /// <param name="analysis">The NotificationAnalysis instance.</param>
    /// <returns>A list of fully qualified handler type names.</returns>
    public static IReadOnlyList<string> GetFullyQualifiedHandlerNames(this NotificationAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return analysis.Handlers.Select(h => h.FormatTypeNameWithNamespace()).ToList();
    }

    /// <summary>
    /// Gets a normalized, human-readable name for the primary interface without backticks.
    /// </summary>
    /// <param name="analysis">The NotificationAnalysis instance.</param>
    /// <returns>A normalized primary interface name.</returns>
    public static string NormalizePrimaryInterfaceName(this NotificationAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        // For notifications, the primary interface is typically just "INotification"
        // which shouldn't have backticks, but we'll be consistent
        return analysis.PrimaryInterface;
    }

    /// <summary>
    /// Gets the fully qualified name of the primary interface.
    /// For notifications, this attempts to find the actual INotification interface.
    /// </summary>
    /// <param name="analysis">The NotificationAnalysis instance.</param>
    /// <returns>The fully qualified primary interface name.</returns>
    public static string GetFullyQualifiedPrimaryInterfaceName(this NotificationAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        
        var interfaces = analysis.Type.GetInterfaces();
        
        // Look for INotification interface
        var notificationInterface = interfaces.FirstOrDefault(i => 
            i == typeof(INotification) || 
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotification)));
            
        if (notificationInterface != null)
        {
            return notificationInterface.FormatTypeNameWithNamespace();
        }
        
        // Fallback to the primary interface name
        return analysis.PrimaryInterface;
    }

    /// <summary>
    /// Gets normalized subscriber type names from the analysis.
    /// This parses the subscriber details to extract clean type names.
    /// </summary>
    /// <param name="analysis">The NotificationAnalysis instance.</param>
    /// <returns>A list of normalized subscriber type names.</returns>
    public static IReadOnlyList<string> NormalizeSubscriberNames(this NotificationAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        
        if (analysis.SubscriberStatus != SubscriberStatus.Present || string.IsNullOrEmpty(analysis.SubscriberDetails))
        {
            return Array.Empty<string>();
        }

        // Extract subscriber types from the SubscriberTypes property if available
        if (analysis.SubscriberTypes.Any())
        {
            return analysis.SubscriberTypes;
        }

        // Fallback: try to parse from subscriber details
        var details = analysis.SubscriberDetails;
        
        // Look for patterns like "(Type1, Type2)" or "Type1, Type2"
        var parenStart = details.IndexOf('(');
        var parenEnd = details.IndexOf(')', parenStart + 1);
        
        if (parenStart >= 0 && parenEnd > parenStart)
        {
            var typesPart = details.Substring(parenStart + 1, parenEnd - parenStart - 1);
            return typesPart.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(t => t.Trim())
                           .Where(t => !string.IsNullOrEmpty(t))
                           .ToArray();
        }

        return Array.Empty<string>();
    }
}

/// <summary>
/// Internal extension methods for Type formatting, using the existing PipelineUtilities where possible.
/// </summary>
internal static class TypeFormattingExtensions
{
    /// <summary>
    /// Formats a Type to a human-readable string without backticks and with proper generic syntax.
    /// Leverages the existing PipelineUtilities.FormatTypeName method.
    /// </summary>
    /// <param name="type">The Type to format.</param>
    /// <returns>A formatted type name.</returns>
    internal static string FormatTypeName(this Type type)
    {
        return PipelineUtilities.FormatTypeName(type);
    }

    /// <summary>
    /// Formats a Type to a human-readable string with namespace and without backticks.
    /// Converts "System.Collections.Generic.List`1[MyNamespace.User]" to "System.Collections.Generic.List<MyNamespace.User>".
    /// </summary>
    /// <param name="type">The Type to format.</param>
    /// <returns>A formatted type name with namespace.</returns>
    internal static string FormatTypeNameWithNamespace(this Type type)
    {
        if (!type.IsGenericType)
        {
            return type.FullName ?? type.Name;
        }

        // Get the base name with namespace, without the generic backtick suffix
        var baseName = type.FullName ?? type.Name;
        var backtickIndex = baseName.IndexOf('`');
        if (backtickIndex > 0)
        {
            baseName = baseName[..backtickIndex];
        }

        // Format generic arguments recursively
        var genericArgs = type.GetGenericArguments();
        var formattedArgs = genericArgs.Select(FormatTypeNameWithNamespace);
        
        return $"{baseName}<{string.Join(", ", formattedArgs)}>";
    }
}