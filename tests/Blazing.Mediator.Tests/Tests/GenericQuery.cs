namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Generic test query for generic type tests.
/// </summary>
public class GenericQuery<T> : IRequest<T>
{
    public T? Value { get; set; }
}