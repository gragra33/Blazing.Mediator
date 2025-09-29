namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Void request type without clear naming pattern.
/// </summary>
public class AmbiguousVoidRequest : IRequest
{
    public int Value { get; set; }
}