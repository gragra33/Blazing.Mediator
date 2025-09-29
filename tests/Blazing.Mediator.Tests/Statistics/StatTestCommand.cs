namespace Blazing.Mediator.Tests.Statistics;

public record StatTestCommand : ICommand
{
    public string Value { get; init; } = string.Empty;
}