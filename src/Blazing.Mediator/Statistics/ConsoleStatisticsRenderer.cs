namespace Blazing.Mediator.Statistics;

/// <summary>
/// Console renderer for statistics output that uses Console.WriteLine.
/// </summary>
public sealed class ConsoleStatisticsRenderer : IStatisticsRenderer
{
    /// <summary>
    /// Renders a message to the console.
    /// </summary>
    /// <param name="message">The message to render.</param>
    public void Render(string message)
    {
        Console.WriteLine(message);
    }
}