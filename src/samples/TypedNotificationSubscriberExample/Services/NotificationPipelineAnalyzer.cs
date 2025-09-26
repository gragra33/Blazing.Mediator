namespace TypedNotificationSubscriberExample.Services;

/// <summary>
/// Helper class for analyzing notification middleware pipeline.
/// Provides detailed information about registered middleware and their execution order.
/// </summary>
public static class NotificationPipelineAnalyzer
{
    /// <summary>
    /// Analyzes the notification middleware pipeline and returns detailed information.
    /// </summary>
    public static List<NotificationMiddlewareInfo> AnalyzeMiddleware(
        INotificationMiddlewarePipelineInspector pipelineInspector,
        IServiceProvider serviceProvider)
    {
        var middlewareInfo = pipelineInspector.GetDetailedMiddlewareInfo(serviceProvider);

        return middlewareInfo
            .Select(m => ExtractNotificationMiddlewareInfo(m.Type, m.Order))
            .OrderBy(m => m.Order)
            .ToList();
    }

    /// <summary>
    /// Extracts detailed information from a notification middleware type.
    /// </summary>
    private static NotificationMiddlewareInfo ExtractNotificationMiddlewareInfo(Type middlewareType, int order)
    {
        var (className, typeParameters) = ExtractClassNameAndTypeParameters(middlewareType);
        var orderDisplay = FormatOrderValue(order);
        var constraints = ExtractGenericConstraints(middlewareType);

        return new NotificationMiddlewareInfo(
            order,
            orderDisplay,
            className,
            typeParameters,
            constraints
        );
    }

    /// <summary>
    /// Extracts class name and type parameters from a notification middleware type.
    /// </summary>
    private static (string ClassName, string TypeParameters) ExtractClassNameAndTypeParameters(Type type)
    {
        var typeName = type.Name;

        if (type.IsGenericType)
        {
            // Remove the generic suffix (e.g., `1, `2)
            var backtickIndex = typeName.IndexOf('`');
            if (backtickIndex >= 0)
            {
                typeName = typeName[..backtickIndex];
            }

            // Build type parameters string
            var genericArgs = type.GetGenericArguments();
            var typeParams = string.Join(", ", genericArgs.Select(arg => arg.Name));
            return (typeName, $"<{typeParams}>");
        }

        return (typeName, string.Empty);
    }

    /// <summary>
    /// Formats order values for display (handles special values like int.MinValue).
    /// </summary>
    private static string FormatOrderValue(int order)
    {
        return order switch
        {
            int.MinValue => "int.MinValue",
            int.MaxValue => "int.MaxValue",
            _ => order.ToString()
        };
    }

    /// <summary>
    /// Extracts generic constraints from middleware type.
    /// </summary>
    private static string ExtractGenericConstraints(Type type)
    {
        if (!type.IsGenericType)
            return string.Empty;

        var constraints = new List<string>();
        var genericArgs = type.GetGenericArguments();

        foreach (var arg in genericArgs)
        {
            var argConstraints = new List<string>();

            // Get generic parameter constraints
            var genericParam = arg.IsGenericParameter ? arg : null;
            if (genericParam != null)
            {
                var constraintTypes = genericParam.GetGenericParameterConstraints();
                argConstraints.AddRange(constraintTypes
                    .Where(constraintType => constraintType.Name.StartsWith("INotification") ||
                                             constraintType.Name.EndsWith("Notification"))
                    .Select(constraintType => constraintType.Name));
            }

            if (argConstraints.Any())
            {
                constraints.Add($"{arg.Name} : {string.Join(", ", argConstraints)}");
            }
        }

        return constraints.Any() ? string.Join("; ", constraints) : string.Empty;
    }
}