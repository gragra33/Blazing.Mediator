using System.Diagnostics;

namespace Blazing.Mediator.Statistics;

/// <summary>
/// Interface for rendering mediator statistics output.
/// </summary>
public interface IStatisticsRenderer
{
    /// <summary>
    /// Renders a statistics message.
    /// </summary>
    /// <param name="message">The message to render.</param>
    void Render(string message);
}

/// <summary>
/// Collects and reports statistics about mediator usage, including query and command analysis.
/// </summary>
public class MediatorStatistics
{
    private readonly ConcurrentDictionary<string, int> _queryCounts = new();
    private readonly ConcurrentDictionary<string, int> _commandCounts = new();
    private readonly ConcurrentDictionary<string, int> _notificationCounts = new();
    private readonly IStatisticsRenderer _renderer;

    /// <summary>
    /// Initializes a new instance of the MediatorStatistics class.
    /// </summary>
    /// <param name="renderer">The renderer to use for statistics output.</param>
    public MediatorStatistics(IStatisticsRenderer renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    /// <summary>
    /// Increments the count for a specific query type.
    /// </summary>
    /// <param name="queryType">The name of the query type.</param>
    public void IncrementQuery(string queryType) 
    {
        if (!string.IsNullOrEmpty(queryType))
        {
            _queryCounts.AddOrUpdate(queryType, 1, (_, count) => count + 1);
        }
    }
    
    /// <summary>
    /// Increments the count for a specific command type.
    /// </summary>
    /// <param name="commandType">The name of the command type.</param>
    public void IncrementCommand(string commandType) 
    {
        if (!string.IsNullOrEmpty(commandType))
        {
            _commandCounts.AddOrUpdate(commandType, 1, (_, count) => count + 1);
        }
    }
    
    /// <summary>
    /// Increments the count for a specific notification type.
    /// </summary>
    /// <param name="notificationType">The name of the notification type.</param>
    public void IncrementNotification(string notificationType) 
    {
        if (!string.IsNullOrEmpty(notificationType))
        {
            _notificationCounts.AddOrUpdate(notificationType, 1, (_, count) => count + 1);
        }
    }

    /// <summary>
    /// Reports the current statistics using the configured renderer.
    /// </summary>
    public void ReportStatistics()
    {
        _renderer.Render("Mediator Statistics:");
        _renderer.Render($"Queries: {_queryCounts.Count}");
        _renderer.Render($"Commands: {_commandCounts.Count}");
        _renderer.Render($"Notifications: {_notificationCounts.Count}");
    }

    /// <summary>
    /// Analyzes all registered queries in the application and returns detailed information grouped by assembly and namespace.
    /// </summary>
    /// <param name="serviceProvider">Service provider to scan for registered query types.</param>
    /// <returns>Read-only list of query analysis information grouped by assembly with namespace.</returns>
    public IReadOnlyList<QueryCommandAnalysis> AnalyzeQueries(IServiceProvider serviceProvider)
    {
        // Look for IQuery<T> implementations first
        var queryTypes = FindTypesImplementingInterface(typeof(IQuery<>));
        
        // Also include IRequest<T> types that look like queries (contain "Query" in name)
        var requestWithResponseTypes = FindTypesImplementingInterface(typeof(IRequest<>))
            .Where(t => t.Name.Contains("Query", StringComparison.OrdinalIgnoreCase));
        
        var allQueryTypes = queryTypes.Concat(requestWithResponseTypes).Distinct().ToList();
        return CreateAnalysisResults(allQueryTypes, serviceProvider);
    }

    /// <summary>
    /// Analyzes all registered commands in the application and returns detailed information grouped by assembly and namespace.
    /// </summary>
    /// <param name="serviceProvider">Service provider to scan for registered command types.</param>
    /// <returns>Read-only list of command analysis information grouped by assembly with namespace.</returns>
    public IReadOnlyList<QueryCommandAnalysis> AnalyzeCommands(IServiceProvider serviceProvider)
    {
        // Look for ICommand and ICommand<T> implementations first
        var commandTypes = FindTypesImplementingInterface(typeof(ICommand))
            .Concat(FindTypesImplementingInterface(typeof(ICommand<>)))
            .Distinct()
            .ToList();
        
        // Also include IRequest types (void commands) that look like commands
        var voidRequestTypes = FindTypesImplementingInterface(typeof(IRequest))
            .Where(t => !t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))) // Exclude IRequest<T>
            .Where(t => t.Name.Contains("Command", StringComparison.OrdinalIgnoreCase));
            
        // Include IRequest<T> types that look like commands (contain "Command" in name)
        var requestWithResponseTypes = FindTypesImplementingInterface(typeof(IRequest<>))
            .Where(t => t.Name.Contains("Command", StringComparison.OrdinalIgnoreCase));
        
        var allCommandTypes = commandTypes.Concat(voidRequestTypes).Concat(requestWithResponseTypes).Distinct().ToList();
        return CreateAnalysisResults(allCommandTypes, serviceProvider);
    }

    /// <summary>
    /// Finds all types in loaded assemblies that implement the specified interface.
    /// </summary>
    private static List<Type> FindTypesImplementingInterface(Type interfaceType)
    {
        var types = new List<Type>();
        
        // Get all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .ToList();

        foreach (var assembly in assemblies)
        {
            try
            {
                var assemblyTypes = assembly.GetTypes()
                    .Where(t => t is { IsAbstract: false, IsInterface: false, IsClass: true })
                    .ToList();

                foreach (var type in assemblyTypes)
                {
                    if (ImplementsInterface(type, interfaceType))
                    {
                        types.Add(type);
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // Skip assemblies that can't be loaded
            }
        }

        return types;
    }

    /// <summary>
    /// Checks if a type implements the specified interface (including generic interfaces).
    /// </summary>
    private static bool ImplementsInterface(Type type, Type interfaceType)
    {
        if (interfaceType.IsGenericTypeDefinition)
        {
            return type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }
        
        return interfaceType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Finds registered handlers for a specific request type.
    /// </summary>
    private static List<Type> FindHandlersForRequestType(Type requestType, IServiceProvider serviceProvider)
    {
        var handlers = new List<Type>();
        
        try
        {
            // Determine the handler interface type based on request type
            Type? handlerInterfaceType = null;
            
            // Check for IQuery<T> -> IRequestHandler<IQuery<T>, T>
            var queryInterface = requestType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
            
            if (queryInterface != null)
            {
                var responseType = queryInterface.GetGenericArguments()[0];
                handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            }
            
            // Check for ICommand -> IRequestHandler<ICommand>
            if (handlerInterfaceType == null && typeof(ICommand).IsAssignableFrom(requestType))
            {
                handlerInterfaceType = typeof(IRequestHandler<>).MakeGenericType(requestType);
            }
            
            // Check for ICommand<T> -> IRequestHandler<ICommand<T>, T>
            if (handlerInterfaceType == null)
            {
                var commandInterface = requestType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
                
                if (commandInterface != null)
                {
                    var responseType = commandInterface.GetGenericArguments()[0];
                    handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
                }
            }
            
            // Check for IRequest -> IRequestHandler<IRequest>
            if (handlerInterfaceType == null && typeof(IRequest).IsAssignableFrom(requestType) && !requestType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            {
                handlerInterfaceType = typeof(IRequestHandler<>).MakeGenericType(requestType);
            }
            
            // Check for IRequest<T> -> IRequestHandler<IRequest<T>, T>
            if (handlerInterfaceType == null)
            {
                var requestInterface = requestType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
                
                if (requestInterface != null)
                {
                    var responseType = requestInterface.GetGenericArguments()[0];
                    handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
                }
            }
            
            if (handlerInterfaceType != null)
            {
                // Try to get all registered services for this handler interface
                var handlerServices = serviceProvider.GetServices(handlerInterfaceType);
                handlers.AddRange(handlerServices.Select(h => h?.GetType()).Distinct()!);
            }
        }
        catch (Exception ex)
        {
            // For debugging: log the exception details (in production this would be logged)
            // ReSharper disable once InvocationIsSkipped
            Debug.WriteLine($"Error finding handlers for {requestType.Name}: {ex.Message}");
        }
        
        return handlers;
    }

    /// <summary>
    /// Creates analysis results from a collection of types, grouped by assembly and namespace.
    /// </summary>
    private static IReadOnlyList<QueryCommandAnalysis> CreateAnalysisResults(IEnumerable<Type> types, IServiceProvider serviceProvider)
    {
        var analysisResults = new List<QueryCommandAnalysis>();

        foreach (var type in types.OrderBy(t => t.Assembly.GetName().Name).ThenBy(t => t.Namespace ?? "Unknown").ThenBy(t => t.Name))
        {
            var className = type.Name;
            var typeParameters = string.Empty;
            Type? responseType = null;

            // Handle generic types
            if (type.IsGenericType)
            {
                // Remove generic suffix from class name
                var backtickIndex = className.IndexOf('`');
                if (backtickIndex > 0)
                {
                    className = className[..backtickIndex];
                }

                // Get type parameters
                var genericArgs = type.GetGenericArguments();
                typeParameters = $"<{string.Join(", ", genericArgs.Select(t => t.Name))}>";
            }

            // Get response type from interfaces
            var queryInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && 
                               (i.GetGenericTypeDefinition() == typeof(IQuery<>) || 
                                i.GetGenericTypeDefinition() == typeof(ICommand<>) ||
                                i.GetGenericTypeDefinition() == typeof(IRequest<>)));

            if (queryInterface != null)
            {
                responseType = queryInterface.GetGenericArguments()[0];
            }

            // Find handlers for this request type
            var handlers = FindHandlersForRequestType(type, serviceProvider);
            
            // Determine handler status
            HandlerStatus handlerStatus;
            string handlerDetails;
            
            switch (handlers.Count)
            {
                case 0:
                    handlerStatus = HandlerStatus.Missing;
                    handlerDetails = "No handler registered";
                    break;
                case 1:
                    handlerStatus = HandlerStatus.Single;
                    handlerDetails = handlers[0].Name;
                    break;
                default:
                    handlerStatus = HandlerStatus.Multiple;
                    handlerDetails = $"{handlers.Count} handlers: {string.Join(", ", handlers.Select(h => h.Name))}";
                    break;
            }

            analysisResults.Add(new QueryCommandAnalysis(
                Type: type,
                ClassName: className,
                TypeParameters: typeParameters,
                Assembly: type.Assembly.GetName().Name ?? "Unknown",
                Namespace: type.Namespace ?? "Unknown",
                ResponseType: responseType,
                HandlerStatus: handlerStatus,
                HandlerDetails: handlerDetails,
                Handlers: handlers
            ));
        }

        return analysisResults;
    }
}