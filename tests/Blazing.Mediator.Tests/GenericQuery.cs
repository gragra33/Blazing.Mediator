namespace Blazing.Mediator.Tests;

public class GenericQuery<T> : IRequest<string>
{
    public T Data { get; set; } = default!;
}