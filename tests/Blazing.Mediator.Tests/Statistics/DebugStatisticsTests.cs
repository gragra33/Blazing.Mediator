using Blazing.Mediator.Configuration;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Debug test to see actual statistics output
/// </summary>
public class DebugStatisticsTests
{
    public record TestQuery(string Message) : IRequest<string>;
    public record TestCommand(string Action) : IRequest;
    public record TestNotification(string Event) : INotification;

    public class TestQueryHandler : IRequestHandler<TestQuery, string>
    {
        public ValueTask<string> Handle(TestQuery request, CancellationToken cancellationToken)
            => ValueTask.FromResult($"Response: {request.Message}");
    }

    public class TestCommandHandler : IRequestHandler<TestCommand>
    {
        public ValueTask Handle(TestCommand request, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;
    }

    public class TestNotificationHandler : INotificationHandler<TestNotification>
    {
        public ValueTask Handle(TestNotification notification, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;
    }

    [Fact]
    public async Task Debug_SeeActualStatisticsOutput()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new DebugStatisticsRenderer();
        services.AddSingleton<IStatisticsRenderer>(renderer);
        var cfg = new MediatorConfiguration();
        cfg.WithStatisticsTracking(options =>
        {
            options.EnableRequestMetrics = true;
            options.EnableNotificationMetrics = true;
        }).WithNotificationHandlerDiscovery();
        services.AddMediator(cfg);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

        // Act
        await mediator.Send(new TestQuery("Query1"));
        await mediator.Send(new TestQuery("Query2"));
        await mediator.Send(new TestCommand("Command1"));
        await mediator.Publish(new TestNotification("Event1"));
        await mediator.Publish(new TestNotification("Event2"));

        statistics.ReportStatistics();

        // Assert - just output what we actually get
        foreach (var message in renderer.Messages)
        {
            Console.WriteLine($"ACTUAL OUTPUT: '{message}'");
        }

        // Basic check that we got some output
        renderer.Messages.ShouldNotBeEmpty();
    }

    private class DebugStatisticsRenderer : IStatisticsRenderer
    {
        public List<string> Messages { get; } = new();

        public void Render(string message)
        {
            Messages.Add(message);
            Console.WriteLine($"RENDERER: {message}");
        }
    }
}