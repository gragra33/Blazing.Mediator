namespace Blazing.Mediator.Statistics;

/// <summary>
/// Interface for rendering mediator statistics output.
/// </summary>
public interface IStatisticsRenderer
{
    /// <summary>
    /// Renders a statistics message.
    /// </summary>
    /// <param name="message">The message to render.</param>
    void Render(string message);
}
