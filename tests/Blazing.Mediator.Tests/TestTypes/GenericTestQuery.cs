namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Generic query test type.
/// </summary>
public class GenericTestQuery<T> : IQuery<T>
{
    public string Filter { get; set; } = string.Empty;
}