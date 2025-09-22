namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Complex query with multiple generic parameters.
/// </summary>
public class ComplexGenericQuery<TInput, TOutput> : IQuery<TOutput>
{
    public TInput? Input { get; set; }
}