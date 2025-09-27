using Blazing.Mediator;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

// Create a simple test to verify SimpleCommand is filtered out
var services = new ServiceCollection();

services.AddMediator(config =>
{
    config.WithStatisticsTracking();
}, Assembly.GetExecutingAssembly());

services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

var serviceProvider = services.BuildServiceProvider();
var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

// Analyze commands
var commands = statistics.AnalyzeCommands(serviceProvider, isDetailed: false);

Console.WriteLine($"Total commands found: {commands.Count}");
foreach (var command in commands)
{
    Console.WriteLine($"  - {command.Assembly}: {command.ClassName} ({command.PrimaryInterface})");
}

// Check if SimpleCommand is in the results
var hasSimpleCommand = commands.Any(c => c.ClassName.Contains("Simple"));
Console.WriteLine($"\nSimpleCommand or similar found: {hasSimpleCommand}");