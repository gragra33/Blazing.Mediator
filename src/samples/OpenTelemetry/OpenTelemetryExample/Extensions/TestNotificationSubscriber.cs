using Blazing.Mediator;
using OpenTelemetryExample.Models;

namespace OpenTelemetryExample.Extensions;

/// <summary>
/// Test notification subscriber for telemetry testing.
/// </summary>
public class TestNotificationSubscriber(ILogger<TestNotificationSubscriber> logger)
    : INotificationSubscriber<TestNotification>
{
    public async Task OnNotification(TestNotification notification, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received test notification: {Message}", notification.Message);
        Console.WriteLine($"[+] Notification subscriber received: {notification.Message}");
        await Task.Delay(Random.Shared.Next(10, 50), cancellationToken); // Simulate work
    }
}