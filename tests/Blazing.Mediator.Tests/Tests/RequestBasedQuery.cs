namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Request-based query for testing IRequest&lt;T&gt; detection.
/// </summary>
public class RequestBasedQuery : IRequest<int>
{
    public string? Filter { get; set; }
}