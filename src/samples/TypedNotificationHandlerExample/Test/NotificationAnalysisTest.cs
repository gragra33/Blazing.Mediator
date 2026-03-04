using Blazing.Mediator;
using Blazing.Mediator.Configuration;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace TypedNotificationHandlerExample.Test;

// Simple test notification
public class TestNotification : INotification
{
    public string Message { get; set; } = string.Empty;
}

// Simple test handler
public class TestNotificationHandler : INotificationHandler<TestNotification>
{
    public ValueTask Handle(TestNotification notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Handling: {notification.Message}");
        return ValueTask.CompletedTask;
    }
}

public static class NotificationAnalysisTest
{
    public static async Task RunTest()
    {
        Console.WriteLine("=== NOTIFICATION ANALYSIS TEST ===");
        
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                var mediatorConfig = new MediatorConfiguration();
                mediatorConfig.WithStatisticsTracking(options =>
                {
                    options.EnableNotificationMetrics = true;
                    options.EnableDetailedAnalysis = true;
                });
                services.AddMediator(mediatorConfig);
            })
            .Build();

        using var scope = host.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();
        
        // Test compact mode (isDetailed: false)
        var compactResults = statistics.AnalyzeNotifications(serviceProvider, isDetailed: false);
        var compactTestNotification = compactResults.FirstOrDefault(r => r.ClassName == "TestNotification");
        
        Console.WriteLine($"COMPACT MODE: TestNotification has {compactTestNotification?.Handlers.Count ?? 0} handlers");
        Console.WriteLine($"COMPACT STATUS: {compactTestNotification?.HandlerStatus}");
        
        // Test detailed mode (isDetailed: true)
        var detailedResults = statistics.AnalyzeNotifications(serviceProvider, isDetailed: true);
        var detailedTestNotification = detailedResults.FirstOrDefault(r => r.ClassName == "TestNotification");
        
        Console.WriteLine($"DETAILED MODE: TestNotification has {detailedTestNotification?.Handlers.Count ?? 0} handlers");
        Console.WriteLine($"DETAILED STATUS: {detailedTestNotification?.HandlerStatus}");
        
        // Both should show the same handler count
        var compactCount = compactTestNotification?.Handlers.Count ?? 0;
        var detailedCount = detailedTestNotification?.Handlers.Count ?? 0;
        
        if (compactCount == detailedCount && compactCount > 0)
        {
            Console.WriteLine("! SUCCESS: Both modes show the same handler count!");
        }
        else
        {
            Console.WriteLine($"X FAILED: Compact={compactCount}, Detailed={detailedCount}");
        }
        
        Console.WriteLine("=== TEST COMPLETE ===");
    }
}