namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Generic command test type.
/// </summary>
public class GenericTestCommand<T> : ICommand<T>
{
    public T? Data { get; set; }
}