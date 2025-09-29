namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Command with type constraints.
/// </summary>
public class ConstrainedTestCommand<T> : ICommand<bool> where T : class, new()
{
    public T? Entity { get; set; }
}