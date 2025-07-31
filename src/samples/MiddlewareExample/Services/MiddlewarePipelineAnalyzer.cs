namespace MiddlewareExample.Services;

/// <summary>
/// Helper class for processing and analyzing middleware pipeline information.
/// Provides utilities for extracting order values, generic type parameters, and sorting middleware by execution order.
/// Uses the extended IMiddlewarePipelineInspector interface to access order information.
/// </summary>
public static class MiddlewarePipelineAnalyzer
{
    /// <summary>
    /// Represents information about a middleware component in the pipeline.
    /// </summary>
    /// <param name="Order">The numeric execution order of the middleware.</param>
    /// <param name="OrderDisplay">The display string for the order (e.g., "int.MinValue", "100").</param>
    /// <param name="ClassName">The name of the middleware class without generic suffixes.</param>
    /// <param name="TypeParameters">The generic type parameters in angle brackets (e.g., "&lt;TRequest&gt;").</param>
    public record MiddlewareInfo(int Order, string OrderDisplay, string ClassName, string TypeParameters);

    /// <summary>
    /// Processes registered middleware types and yields detailed information about each middleware component.
    /// Uses the extended IMiddlewarePipelineInspector interface to get actual order values from DI.
    /// </summary>
    /// <param name="pipelineInspector">The pipeline inspector that provides middleware information.</param>
    /// <param name="serviceProvider">The service provider to resolve concrete middleware instances and get actual order values.</param>
    /// <returns>An enumerable of middleware information sorted by execution order (ascending).</returns>
    public static IEnumerable<MiddlewareInfo> AnalyzeMiddleware(IMiddlewarePipelineInspector pipelineInspector, IServiceProvider serviceProvider)
    {
        // Use the GetDetailedMiddlewareInfo method with service provider to get actual order values from DI instances
        var detailedInfo = pipelineInspector.GetDetailedMiddlewareInfo(serviceProvider);
        
        return detailedInfo
            .Select(info => ExtractMiddlewareInfo(info.Type, info.Order))
            .OrderBy(x => x.Order);
    }

    /// <summary>
    /// Extracts detailed information from a middleware type and its known order value.
    /// </summary>
    /// <param name="middlewareType">The middleware type to analyze.</param>
    /// <param name="actualOrder">The actual order value from Blazing.Mediator's registration.</param>
    /// <returns>A MiddlewareInfo record containing the extracted information.</returns>
    private static MiddlewareInfo ExtractMiddlewareInfo(Type middlewareType, int actualOrder)
    {
        var (className, typeParameters) = ExtractClassNameAndTypeParameters(middlewareType);
        var orderDisplay = FormatOrderValue(actualOrder);
        
        return new MiddlewareInfo(actualOrder, orderDisplay, className, typeParameters);
    }

    /// <summary>
    /// Extracts the class name and generic type parameters from a middleware type.
    /// Handles both generic middleware classes and concrete implementations.
    /// </summary>
    /// <param name="middlewareType">The middleware type to analyze.</param>
    /// <returns>A tuple containing the clean class name and formatted type parameters.</returns>
    private static (string ClassName, string TypeParameters) ExtractClassNameAndTypeParameters(Type middlewareType)
    {
        string className = middlewareType.Name;
        string typeParameters = "";

        if (middlewareType.IsGenericType)
        {
            // For generic classes like ErrorHandlingMiddleware<TRequest>
            var genericArgs = middlewareType.GetGenericArguments();
            if (genericArgs.Length > 0)
            {
                var argNames = genericArgs.Select(arg => arg.Name).ToArray();
                typeParameters = $"<{string.Join(", ", argNames)}>";
            }
            
            // Remove the generic type suffix (e.g., `1) from the class name
            var backtickIndex = className.IndexOf('`');
            if (backtickIndex > 0)
            {
                className = className.Substring(0, backtickIndex);
            }
        }
        else
        {
            // For non-generic classes, check their interface implementations
            // to get the actual concrete types like ProductQueryCacheMiddleware : IRequestMiddleware<GetProductQuery, string>
            var interfaces = middlewareType.GetInterfaces();
            var requestMiddlewareInterface = interfaces.FirstOrDefault(i => 
                i.IsGenericType && 
                (i.GetGenericTypeDefinition().Name.Contains("IRequestMiddleware")));
            
            if (requestMiddlewareInterface != null)
            {
                // Get the actual concrete type arguments from the interface implementation
                var interfaceArgs = requestMiddlewareInterface.GetGenericArguments();
                if (interfaceArgs.Length > 0)
                {
                    var argNames = interfaceArgs.Select(arg => arg.Name).ToArray();
                    typeParameters = $"<{string.Join(", ", argNames)}>";
                }
            }
        }

        return (className, typeParameters);
    }

    /// <summary>
    /// Formats the numeric order value into a readable display string.
    /// Converts special values like int.MinValue and int.MaxValue to their string representations.
    /// </summary>
    /// <param name="order">The numeric order value.</param>
    /// <returns>A formatted string representation of the order.</returns>
    private static string FormatOrderValue(int order)
    {
        return order switch
        {
            int.MinValue => "int.MinValue",
            int.MaxValue => "int.MaxValue",
            _ => order.ToString()
        };
    }
}
