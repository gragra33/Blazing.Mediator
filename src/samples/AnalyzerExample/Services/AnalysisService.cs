using Blazing.Mediator;
using Blazing.Mediator.Statistics;

namespace AnalyzerExample.Services;

/// <summary>
/// Service that demonstrates all analysis capabilities of Blazing.Mediator across multiple assemblies
/// </summary>
public class AnalysisService(
    IServiceProvider serviceProvider,
    MediatorStatistics statistics,
    IMiddlewarePipelineInspector middlewarePipelineInspector,
    INotificationMiddlewarePipelineInspector notificationPipelineInspector)
{
    public async Task RunAllAnalysisExamples()
    {
        Console.WriteLine("COMPREHENSIVE MULTI-ASSEMBLY ANALYSIS");
        Console.WriteLine("=========================================");
        Console.WriteLine();
        
        // 1. Cross-Assembly Query Analysis
        await RunCrossAssemblyQueryAnalysis();
        
        // 2. Cross-Assembly Command Analysis  
        await RunCrossAssemblyCommandAnalysis();
        
        // 3. Cross-Assembly Notification Analysis
        await RunCrossAssemblyNotificationAnalysis();
        
        // 4. Multi-Project Request Middleware Analysis
        await RunMultiProjectRequestMiddlewareAnalysis();
        
        // 5. Multi-Project Notification Middleware Analysis
        await RunMultiProjectNotificationMiddlewareAnalysis();
        
        // 6. Advanced Type Formatting Across Assemblies
        await RunAdvancedCrossAssemblyFormattingExamples();
    }
    
    private async Task RunCrossAssemblyQueryAnalysis()
    {
        Console.WriteLine("CROSS-ASSEMBLY QUERY ANALYSIS");
        Console.WriteLine("==================================");
        Console.WriteLine();
        
        // Analyze all queries with detailed information across assemblies
        var queries = statistics.AnalyzeQueries(serviceProvider, isDetailed: true);
        
        Console.WriteLine($"[INFO] Found {queries.Count} query types across {queries.GroupBy(q => q.Assembly).Count()} assemblies:");
        Console.WriteLine();
        
        // Group by assembly to show cross-assembly organization
        var queriesByAssembly = queries.GroupBy(q => q.Assembly).OrderBy(g => g.Key);
        
        foreach (var assemblyGroup in queriesByAssembly)
        {
            Console.WriteLine($"Assembly: {assemblyGroup.Key}");
            
            var namespaceGroups = assemblyGroup.GroupBy(q => q.Namespace).OrderBy(g => g.Key);
            foreach (var namespaceGroup in namespaceGroups)
            {
                Console.WriteLine($"  Namespace: {namespaceGroup.Key}");
                
                foreach (var query in namespaceGroup.OrderBy(q => q.ClassName))
                {
                    // Check if handler is missing and set color accordingly
                    bool isMissing = query.HandlerStatus == HandlerStatus.Missing;
                    if (isMissing)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    
                    Console.WriteLine($"    - Query: {query.ClassName}{query.TypeParameters}");
                    Console.WriteLine($"       Response: {query.ResponseType?.Name ?? "void"}");
                    Console.WriteLine($"       Interface: {query.PrimaryInterface}");
                    Console.WriteLine($"       Handler: {query.HandlerStatus} - {query.HandlerDetails}");
                    
                    if (isMissing)
                    {
                        Console.ResetColor();
                    }
                    
                    // Demonstrate new normalized extensions
                    Console.WriteLine($"       [NORMALIZED] Response: {query.NormalizeResponseTypeName() ?? "void"}");
                    Console.WriteLine($"       [NORMALIZED] Interface: {query.NormalizePrimaryInterfaceName()}");
                    Console.WriteLine($"       [NORMALIZED] Handlers: [{string.Join(", ", query.NormalizeHandlerNames())}]");
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }
        
        await Task.Delay(1500); // Pause for readability
    }
    
    private async Task RunCrossAssemblyCommandAnalysis()
    {
        Console.WriteLine("CROSS-ASSEMBLY COMMAND ANALYSIS");
        Console.WriteLine("===================================");
        Console.WriteLine();
        
        var commands = statistics.AnalyzeCommands(serviceProvider, isDetailed: true);
        
        Console.WriteLine($"[INFO] Found {commands.Count} command types across {commands.GroupBy(c => c.Assembly).Count()} assemblies:");
        Console.WriteLine();
        
        var commandsByAssembly = commands.GroupBy(c => c.Assembly).OrderBy(g => g.Key);
        
        foreach (var assemblyGroup in commandsByAssembly)
        {
            Console.WriteLine($"Assembly: {assemblyGroup.Key}");
            
            var namespaceGroups = assemblyGroup.GroupBy(c => c.Namespace).OrderBy(g => g.Key);
            foreach (var namespaceGroup in namespaceGroups)
            {
                Console.WriteLine($"  Namespace: {namespaceGroup.Key}");
                
                foreach (var command in namespaceGroup.OrderBy(c => c.ClassName))
                {
                    // Check if handler is missing and set color accordingly
                    bool isMissing = command.HandlerStatus == HandlerStatus.Missing;
                    if (isMissing)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    
                    Console.WriteLine($"    - Command: {command.ClassName}{command.TypeParameters}");
                    Console.WriteLine($"       Response: {command.ResponseType?.Name ?? "void"}");
                    Console.WriteLine($"       Interface: {command.PrimaryInterface}");
                    Console.WriteLine($"       Handler: {command.HandlerStatus} - {command.HandlerDetails}");
                    
                    if (isMissing)
                    {
                        Console.ResetColor();
                    }
                    
                    // Show interface implementations if available (check if property exists)
                    var commandType = command.GetType();
                    var interfacesProperty = commandType.GetProperty("Interfaces");
                    if (interfacesProperty != null)
                    {
                        var interfaces = interfacesProperty.GetValue(command) as System.Collections.IList;
                        if (interfaces?.Count > 1)
                        {
                            Console.WriteLine($"       Implements: {string.Join(", ", interfaces.Cast<object>())}");
                        }
                    }
                    
                    // Demonstrate normalized extensions
                    Console.WriteLine($"       [NORMALIZED] Response: {command.NormalizeResponseTypeName() ?? "void"}");
                    Console.WriteLine($"       [NORMALIZED] Handlers: [{string.Join(", ", command.NormalizeHandlerNames())}]");
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }
        
        await Task.Delay(1500);
    }
    
    private async Task RunCrossAssemblyNotificationAnalysis()
    {
        Console.WriteLine("CROSS-ASSEMBLY NOTIFICATION ANALYSIS");
        Console.WriteLine("========================================");
        Console.WriteLine();
        
        var notifications = statistics.AnalyzeNotifications(serviceProvider, isDetailed: true);
        
        Console.WriteLine($"[INFO] Found {notifications.Count} notification types across {notifications.GroupBy(n => n.AssemblyName).Count()} assemblies:");
        Console.WriteLine();
        
        var notificationsByAssembly = notifications.GroupBy(n => n.AssemblyName).OrderBy(g => g.Key);
        
        foreach (var assemblyGroup in notificationsByAssembly)
        {
            Console.WriteLine($"Assembly: {assemblyGroup.Key}");
            
            var namespaceGroups = assemblyGroup.GroupBy(n => n.Namespace).OrderBy(g => g.Key);
            foreach (var namespaceGroup in namespaceGroups)
            {
                Console.WriteLine($"  Namespace: {namespaceGroup.Key}");
                
                foreach (var notification in namespaceGroup.OrderBy(n => n.TypeName))
                {
                    // Check if handlers are missing and set color accordingly
                    bool isMissing = notification.HandlerCount == 0 && notification.ActiveSubscriberCount == 0;
                    if (isMissing)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    
                    Console.WriteLine($"    - Notification: {notification.TypeName}{notification.TypeParameters}");
                    Console.WriteLine($"       Interface: {notification.PrimaryInterface}");
                    Console.WriteLine($"       Handlers: {notification.HandlerStatus} - {notification.HandlerDetails}");
                    Console.WriteLine($"       Pattern: {notification.Pattern}");
                    Console.WriteLine($"       Broadcast: {notification.SupportsBroadcast}");
                    
                    if (isMissing)
                    {
                        Console.ResetColor();
                    }
                    
                    // Demonstrate normalized extensions
                    Console.WriteLine($"       [NORMALIZED] Handlers: [{string.Join(", ", notification.NormalizeHandlerNames())}]");
                    Console.WriteLine($"       [NORMALIZED] Interface: {notification.NormalizePrimaryInterfaceName()}");
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }
        
        await Task.Delay(1500);
    }
    
    private async Task RunMultiProjectRequestMiddlewareAnalysis()
    {
        Console.WriteLine("MULTI-PROJECT REQUEST MIDDLEWARE ANALYSIS");
        Console.WriteLine("=============================================");
        Console.WriteLine();
        
        var middlewareAnalysis = middlewarePipelineInspector.AnalyzeMiddleware(serviceProvider);
        
        Console.WriteLine($"[INFO] Found {middlewareAnalysis.Count} request middleware across {middlewareAnalysis.GroupBy(m => m.GetAssemblyName()).Count()} assemblies:");
        Console.WriteLine();
        
        var middlewareByAssembly = middlewareAnalysis.GroupBy(m => m.GetAssemblyName()).OrderBy(g => g.Key);
        
        foreach (var assemblyGroup in middlewareByAssembly)
        {
            Console.WriteLine($"Assembly: {assemblyGroup.Key}");
            
            var namespaceGroups = assemblyGroup.GroupBy(m => m.GetNamespace()).OrderBy(g => g.Key);
            foreach (var namespaceGroup in namespaceGroups)
            {
                Console.WriteLine($"  Namespace: {namespaceGroup.Key}");
                
                foreach (var middleware in namespaceGroup.OrderBy(m => m.Order).ThenBy(m => m.ClassName))
                {
                    Console.WriteLine($"    - Middleware: {middleware.ClassName}");
                    Console.WriteLine($"       Type: {middleware.Type.Name}");
                    Console.WriteLine($"       Order: {middleware.Order}");
                    Console.WriteLine($"       Generic: {middleware.IsGeneric()}, Parameters: {middleware.GetGenericParameterCount()}");
                    Console.WriteLine($"       Configuration: {middleware.GetConfigurationTypeName()}");
                    
                    // Demonstrate all new normalized extensions
                    Console.WriteLine($"       [NORMALIZED] Type: {middleware.NormalizeTypeName()}");
                    Console.WriteLine($"       [NORMALIZED] Class: {middleware.NormalizeClassName()}");
                    Console.WriteLine($"       [NORMALIZED] Parameters: {middleware.NormalizeTypeParameters()}");
                    Console.WriteLine($"       [NORMALIZED] Order: {middleware.NormalizeOrderDisplay()}");
                    Console.WriteLine($"       [NORMALIZED] Summary: {middleware.NormalizeSummary()}");
                    Console.WriteLine($"       [NORMALIZED] Full Summary: {middleware.NormalizeSummary(includeNamespace: true)}");
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }
        
        Console.WriteLine($"[PIPELINE STATS]:");
        Console.WriteLine($"   Total Middleware: {middlewareAnalysis.Count}");
        Console.WriteLine($"   Assemblies: {middlewareAnalysis.GroupBy(m => m.GetAssemblyName()).Count()}");
        Console.WriteLine($"   Namespaces: {middlewareAnalysis.GroupBy(m => m.GetNamespace()).Count()}");
        Console.WriteLine($"   Generic: {middlewareAnalysis.Count(m => m.IsGeneric())}");
        Console.WriteLine($"   With Configuration: {middlewareAnalysis.Count(m => m.HasConfiguration())}");
        Console.WriteLine($"   Average Order: {middlewareAnalysis.Average(m => m.Order):F1}");
        
        Console.WriteLine();
        await Task.Delay(2000);
    }
    
    private async Task RunMultiProjectNotificationMiddlewareAnalysis()
    {
        Console.WriteLine("MULTI-PROJECT NOTIFICATION MIDDLEWARE ANALYSIS");
        Console.WriteLine("==================================================");
        Console.WriteLine();
        
        var middlewareAnalysis = notificationPipelineInspector.AnalyzeMiddleware(serviceProvider);
        
        Console.WriteLine($"[INFO] Found {middlewareAnalysis.Count} notification middleware across {middlewareAnalysis.GroupBy(m => m.GetAssemblyName()).Count()} assemblies:");
        Console.WriteLine();
        
        var middlewareByAssembly = middlewareAnalysis.GroupBy(m => m.GetAssemblyName()).OrderBy(g => g.Key);
        
        foreach (var assemblyGroup in middlewareByAssembly)
        {
            Console.WriteLine($"Assembly: {assemblyGroup.Key}");
            
            var namespaceGroups = assemblyGroup.GroupBy(m => m.GetNamespace()).OrderBy(g => g.Key);
            foreach (var namespaceGroup in namespaceGroups)
            {
                Console.WriteLine($"  Namespace: {namespaceGroup.Key}");
                
                foreach (var middleware in namespaceGroup.OrderBy(m => m.Order).ThenBy(m => m.ClassName))
                {
                    Console.WriteLine($"    - Middleware: {middleware.ClassName}");
                    Console.WriteLine($"       Type: {middleware.Type.Name}");
                    Console.WriteLine($"       Order: {middleware.Order}");
                    Console.WriteLine($"       Generic: {middleware.IsGeneric()}, Parameters: {middleware.GetGenericParameterCount()}");
                    Console.WriteLine($"       Constraints: {middleware.GenericConstraints}");
                    
                    // Demonstrate comprehensive normalized extensions
                    Console.WriteLine($"       [NORMALIZED] Type: {middleware.NormalizeTypeName()}");
                    Console.WriteLine($"       [NORMALIZED] Constraints: {middleware.NormalizeGenericConstraints()}");
                    Console.WriteLine($"       [NORMALIZED] Order: {middleware.NormalizeOrderDisplay()}");
                    Console.WriteLine($"       [NORMALIZED] Summary: {middleware.NormalizeSummary()}");
                    Console.WriteLine($"       [NORMALIZED] Fully Qualified: {middleware.GetFullyQualifiedTypeName()}");
                    Console.WriteLine($"       [NORMALIZED] Type Name Only: {middleware.NormalizeTypeName()}");
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }
        
        await Task.Delay(1500);
    }
    
    private async Task RunAdvancedCrossAssemblyFormattingExamples()
    {
        Console.WriteLine("ADVANCED CROSS-ASSEMBLY TYPE FORMATTING");
        Console.WriteLine("===========================================");
        Console.WriteLine();
        
        var queries = statistics.AnalyzeQueries(serviceProvider, isDetailed: true);
        var commands = statistics.AnalyzeCommands(serviceProvider, isDetailed: true);
        var notifications = statistics.AnalyzeNotifications(serviceProvider, isDetailed: true);
        var requestMiddleware = middlewarePipelineInspector.AnalyzeMiddleware(serviceProvider);
        var notificationMiddleware = notificationPipelineInspector.AnalyzeMiddleware(serviceProvider);
        
        // Count missing handlers
        var missingQueries = queries.Count(q => q.HandlerStatus == HandlerStatus.Missing);
        var missingCommands = commands.Count(c => c.HandlerStatus == HandlerStatus.Missing);
        var missingNotifications = notifications.Count(n => n.HandlerCount == 0 && n.ActiveSubscriberCount == 0);
        var totalMissing = missingQueries + missingCommands + missingNotifications;
        
        Console.WriteLine("[DEMO] Before vs After Formatting Examples:");
        Console.WriteLine();
        
        // Complex query example
        var complexQuery = queries.FirstOrDefault(q => q.TypeParameters.Contains(',') || (q.ResponseType?.IsGenericType == true));
        if (complexQuery != null)
        {
            Console.WriteLine($"Complex Query Example from {complexQuery.Assembly}:");
            Console.WriteLine($"   Raw Response Type: {complexQuery.ResponseType}");
            Console.WriteLine($"   Normalized Response: {complexQuery.NormalizeResponseTypeName()}");
            Console.WriteLine($"   Raw Interface: {complexQuery.PrimaryInterface}");
            Console.WriteLine($"   Normalized Interface: {complexQuery.NormalizePrimaryInterfaceName()}");
            Console.WriteLine($"   Fully Qualified Response: {complexQuery.GetFullyQualifiedResponseTypeName()}");
            Console.WriteLine();
        }
        
        // Complex middleware example
        var complexMiddleware = requestMiddleware.FirstOrDefault(m => m.IsGeneric() && m.GetGenericParameterCount() > 1);
        if (complexMiddleware != null)
        {
            Console.WriteLine($"Complex Middleware Example from {complexMiddleware.GetAssemblyName()}:");
            Console.WriteLine($"   Raw Type: {complexMiddleware.Type}");
            Console.WriteLine($"   Normalized Type: {complexMiddleware.NormalizeTypeName()}");
            Console.WriteLine($"   Raw Parameters: {complexMiddleware.TypeParameters}");
            Console.WriteLine($"   Normalized Parameters: {complexMiddleware.NormalizeTypeParameters()}");
            Console.WriteLine($"   Clean Summary: {complexMiddleware.NormalizeSummary()}");
            Console.WriteLine($"   Full Summary: {complexMiddleware.NormalizeSummary(includeNamespace: true)}");
            Console.WriteLine();
        }
        
        // Assembly distribution
        Console.WriteLine("[CROSS-ASSEMBLY DISTRIBUTION]:");
        Console.WriteLine($"   Queries across {queries.GroupBy(q => q.Assembly).Count()} assemblies");
        Console.WriteLine($"   Commands across {commands.GroupBy(c => c.Assembly).Count()} assemblies");
        Console.WriteLine($"   Notifications across {notifications.GroupBy(n => n.AssemblyName).Count()} assemblies");
        Console.WriteLine($"   Request middleware across {requestMiddleware.GroupBy(m => m.GetAssemblyName()).Count()} assemblies");
        Console.WriteLine($"   Notification middleware across {notificationMiddleware.GroupBy(m => m.GetAssemblyName()).Count()} assemblies");
        
        Console.WriteLine();
        Console.WriteLine("[MISSING HANDLERS SUMMARY]:");
        if (totalMissing > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"   Missing Query Handlers: {missingQueries} queries need handlers");
            Console.WriteLine($"   Missing Command Handlers: {missingCommands} commands need handlers");
            Console.WriteLine($"   Missing Notification Handlers: {missingNotifications} notifications need handlers");
            Console.WriteLine($"   Total Missing: {totalMissing} handlers need to be implemented");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("   All handlers are implemented!");
            Console.ResetColor();
        }
        
        Console.WriteLine();
        Console.WriteLine("[FORMATTING BENEFITS DEMONSTRATED]:");
        Console.WriteLine("   - Clean type names without backticks across all assemblies");
        Console.WriteLine("   - Proper generic syntax for complex types");
        Console.WriteLine("   - Assembly and namespace identification");
        Console.WriteLine("   - Consistent formatting across all analysis types");
        Console.WriteLine("   - Summary formats for quick overviews");
        Console.WriteLine("   - Special order value formatting");
        Console.WriteLine("   - Configuration type analysis");
        Console.WriteLine("   - Missing handler detection with visual indicators");
        
        await Task.Delay(1000);
    }
}