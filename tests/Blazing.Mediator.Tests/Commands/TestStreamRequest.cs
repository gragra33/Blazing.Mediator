namespace Blazing.Mediator.Tests.Commands;

/// <summary>
/// Test streaming request for unit testing
/// </summary>
public class TestStreamRequest : IStreamRequest<string>
{
    public string? SearchTerm { get; set; }
}
