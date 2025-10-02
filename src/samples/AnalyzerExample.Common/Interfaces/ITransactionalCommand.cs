using Blazing.Mediator;

namespace AnalyzerExample.Common.Interfaces;

public interface ITransactionalCommand : ICommand
{
    bool RequiresTransaction { get; }
}