namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Request type without clear Query/Command naming pattern.
/// </summary>
public class AmbiguousRequest : IRequest<string>
{
    public string Data { get; set; } = string.Empty;
}