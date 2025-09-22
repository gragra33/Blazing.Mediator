namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Third test command for multiple type tests.
/// </summary>
public class ThirdCommand : IRequest<bool>
{
    public bool Flag { get; set; }
}