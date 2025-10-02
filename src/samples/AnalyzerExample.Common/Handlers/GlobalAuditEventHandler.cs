using AnalyzerExample.Common.Interfaces;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Common.Handlers;

/// <summary>
/// Global audit handler that logs all domain events that implement IDomainEvent
/// This demonstrates handling events through shared interfaces without direct references
/// </summary>
public class GlobalAuditEventHandler : INotificationHandler<IDomainEvent>
{
    private readonly ILogger<GlobalAuditEventHandler> _logger;

    public GlobalAuditEventHandler(ILogger<GlobalAuditEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(IDomainEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AUDIT: Domain event {EventType} occurred at {Timestamp}", 
            notification.GetType().Name, DateTime.UtcNow);

        await SimulateAuditLogging(notification, cancellationToken);
    }

    private async Task SimulateAuditLogging(IDomainEvent notification, CancellationToken cancellationToken)
    {
        // Simulate writing to audit database
        _logger.LogDebug("Writing audit log for {EventType}", notification.GetType().Name);
        await Task.Delay(10, cancellationToken);
    }
}