namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Command test type implementing IRequest&lt;T&gt; with Command in name.
/// </summary>
public class RequestReturningTestCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
}