namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Another test query for grouping tests.
/// </summary>
public class AnotherQuery : IRequest<int>
{
    public int Value { get; set; }
}