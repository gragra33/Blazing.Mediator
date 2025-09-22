namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Query test type implementing IRequest&lt;T&gt; with Query in name.
/// </summary>
public class RequestBasedTestQuery : IRequest<List<string>>
{
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
}