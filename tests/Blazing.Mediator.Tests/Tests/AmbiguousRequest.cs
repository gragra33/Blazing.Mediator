namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Ambiguous request that doesn't follow Query/Command naming convention.
/// </summary>
public class AmbiguousRequest : IRequest<string>
{
    public string? Value { get; set; }
}