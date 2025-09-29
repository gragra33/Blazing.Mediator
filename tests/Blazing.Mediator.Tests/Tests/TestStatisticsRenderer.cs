using Blazing.Mediator.Statistics;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Test statistics renderer for capturing output.
/// </summary>
public class TestStatisticsRenderer : IStatisticsRenderer
{
    public List<string> Messages { get; } = [];

    public void Render(string message)
    {
        Messages.Add(message);
    }
}