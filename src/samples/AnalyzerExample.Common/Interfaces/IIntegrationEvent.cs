using Blazing.Mediator;

namespace AnalyzerExample.Common.Interfaces;

public interface IIntegrationEvent : INotification
{
    Guid EventId { get; }
    DateTime Timestamp { get; }
    string Source { get; }
}