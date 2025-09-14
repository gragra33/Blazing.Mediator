namespace Blazing.Mediator.Statistics;

/// <summary>
/// Console renderer for statistics output that uses Console.WriteLine.
/// </summary>
public class ConsoleStatisticsRenderer : IStatisticsRenderer
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

/// <summary>
/// Text writer renderer for statistics output that writes to a TextWriter.
/// </summary>
public class TextWriterStatisticsRenderer : IStatisticsRenderer
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Initializes a new instance of the TextWriterStatisticsRenderer class.
    /// </summary>
    /// <param name="writer">The TextWriter to write output to.</param>
    public TextWriterStatisticsRenderer(TextWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    /// <summary>
    /// Renders a message to the TextWriter.
    /// </summary>
    /// <param name="message">The message to render.</param>
    public void Render(string message)
    {
        _writer.WriteLine(message);
    }
}