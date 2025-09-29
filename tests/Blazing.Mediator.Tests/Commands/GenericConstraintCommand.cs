namespace Blazing.Mediator.Tests.Commands;

/// <summary>
/// Generic command for testing C# generic constraints functionality.
/// This tests standard C# generic constraints, not the removed constraint validation feature.
/// </summary>
public class GenericConstraintCommand<TEntity> : IRequest
    where TEntity : ITestConstraintEntity
{
    public TEntity Data { get; set; } = default!;
}