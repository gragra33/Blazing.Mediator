namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Generic command with constraints for testing.
/// </summary>
public class GenericConstraintCommand<T> : IRequest where T : class
{
    public T? Entity { get; set; }
}