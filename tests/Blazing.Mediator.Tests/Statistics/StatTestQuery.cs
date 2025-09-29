namespace Blazing.Mediator.Tests.Statistics;

public record StatTestQuery : IQuery<string>
{
    public string Value { get; init; } = string.Empty;
}