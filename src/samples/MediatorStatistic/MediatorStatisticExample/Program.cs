using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Blazing.Mediator;
using Blazing.Mediator.Configuration;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        var config = new MediatorConfiguration()
            .WithStatisticsTracking()
            .WithMiddlewareDiscovery()
            .WithNotificationMiddlewareDiscovery();

        services.AddMediator(config);
    })
    .Build();

Console.WriteLine("===========================================");
Console.WriteLine("Blazing.Mediator Statistics Snapshot Demo");
Console.WriteLine("===========================================");
Console.WriteLine();
Console.WriteLine("This sample demonstrates type-constrained statistics snapshot middleware.");
Console.WriteLine("StatisticsSnapshotMiddleware<TRequest, TResponse> and");
Console.WriteLine("StatisticsSnapshotMiddleware<TRequest> both have Order = 10,");
Console.WriteLine("and only execute for requests implementing IStatisticsTrackedRequest.");
Console.WriteLine();
Console.WriteLine("See MediatorStatisticExample.Tests for verification of correct middleware order.");
