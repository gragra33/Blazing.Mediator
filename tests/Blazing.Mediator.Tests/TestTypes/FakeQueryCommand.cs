namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Type with confusing name that doesn't implement mediator interfaces.
/// </summary>
public class FakeQueryCommand
{
    public string Data { get; set; } = string.Empty;
}