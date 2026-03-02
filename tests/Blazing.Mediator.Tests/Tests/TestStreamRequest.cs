namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests-folder streaming request (temporary name to avoid source generator collision until Phase H folder reorganisation).
/// </summary>
public class TestsTestStreamRequest : IStreamRequest<string>
{
    public string? Value { get; set; }
}