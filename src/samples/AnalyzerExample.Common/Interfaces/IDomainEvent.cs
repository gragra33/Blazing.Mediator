using Blazing.Mediator;

namespace AnalyzerExample.Common.Interfaces;

public interface IDomainEvent : INotification
{
    DateTime OccurredAt { get; }
    string EventType { get; }
    int Version { get; }
}