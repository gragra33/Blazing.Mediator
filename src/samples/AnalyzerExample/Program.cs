using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AnalyzerExample.Services;
using Blazing.Mediator;
using System.Text;

namespace AnalyzerExample;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Multi-Assembly Analyzer Example - Demonstrates Blazing.Mediator analysis across multiple projects
        // This example showcases how the analyzer extensions work across different assemblies and namespaces

        // Ensure console can handle Unicode properly on all systems
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("BLAZING.MEDIATOR MULTI-ASSEMBLY ANALYZER EXAMPLE");
        Console.WriteLine("====================================================");
        Console.WriteLine();
        Console.WriteLine("This example demonstrates comprehensive analysis capabilities across multiple assemblies:");
        Console.WriteLine("  - AnalyzerExample.Common - Shared middleware and interfaces");
        Console.WriteLine("  - AnalyzerExample.Products - Product domain with queries, commands, and handlers");
        Console.WriteLine("  - AnalyzerExample.Users - User management with complex domain models");
        Console.WriteLine("  - AnalyzerExample.Orders - Order processing with status tracking");
        Console.WriteLine("  - Main Assembly - Coordination and cross-cutting concerns");
        Console.WriteLine();
        Console.WriteLine("Key Analysis Features:");
        Console.WriteLine("  - Cross-assembly type normalization without backticks");
        Console.WriteLine("  - Namespace and assembly identification");
        Console.WriteLine("  - Middleware pipeline analysis across projects");
        Console.WriteLine("  - Handler discovery and registration validation");
        Console.WriteLine("  - Generic type parameter analysis");
        Console.WriteLine("  - Configuration and order display formatting");
        Console.WriteLine();

        var host = CreateHost();

        using var scope = host.Services.CreateScope();
        var analysisService = scope.ServiceProvider.GetRequiredService<AnalysisService>();

        Console.WriteLine("Starting comprehensive multi-assembly analysis...");
        Console.WriteLine();

        await analysisService.RunAllAnalysisExamples();

        Console.WriteLine();
        Console.WriteLine("====================================================================");
        Console.WriteLine("[COMPLETE] Multi-assembly analysis complete! Press any key to exit...");
        Console.ReadKey();
    }

    private static IHost CreateHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices((context, services) =>
            {
                // Register Blazing.Mediator with comprehensive discovery across all assemblies
                services.AddMediator(config =>
                {
                    // Enable detailed statistics tracking for comprehensive analysis
                    config.WithStatisticsTracking();

                    // Enable middleware discovery
                    config.WithMiddlewareDiscovery();

                    // Enable notification middleware discovery
                    config.WithNotificationMiddlewareDiscovery();

                    // Register assemblies for handler and middleware discovery
                    config.AddAssemblies(
                            typeof(Common.Domain.BaseEntity).Assembly, // Common
                            typeof(Products.Domain.Product).Assembly, // Products  
                            typeof(Users.Domain.User).Assembly, // Users
                            typeof(Orders.Domain.Order).Assembly, // Orders
                            typeof(Program).Assembly // Main
                        );
                });
                
                // Register the analysis service
                services.AddScoped<AnalysisService>();
            })
            .Build();
    }
}