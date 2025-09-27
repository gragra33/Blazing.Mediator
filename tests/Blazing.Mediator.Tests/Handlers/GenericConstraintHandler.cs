using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Tests.Commands;

namespace Blazing.Mediator.Tests.Handlers;

/// <summary>
/// Generic handler for testing C# generic constraints functionality.
/// This tests standard C# generic constraints, not the removed constraint validation feature.
/// </summary>
public class GenericConstraintHandler<TEntity> : IRequestHandler<GenericConstraintCommand<TEntity>>
    where TEntity : ITestConstraintEntity
{
    public static object? LastProcessedEntity { get; private set; }

    public Task Handle(GenericConstraintCommand<TEntity> request, CancellationToken cancellationToken)
    {
        LastProcessedEntity = request.Data;
        return Task.CompletedTask;
    }
}