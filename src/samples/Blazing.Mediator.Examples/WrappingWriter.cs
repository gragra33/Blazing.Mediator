using System.Text;

namespace Blazing.Mediator.Examples;

/// <summary>
/// A TextWriter wrapper that captures content for later analysis.
/// This is identical to the MediatR version - utility classes don't need conversion.
/// </summary>
public class WrappingWriter : TextWriter
{
    private readonly TextWriter _innerWriter;
    private readonly StringBuilder _stringWriter = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WrappingWriter"/> class.
    /// </summary>
    /// <param name="innerWriter">The inner text writer.</param>
    public WrappingWriter(TextWriter innerWriter)
    {
        _innerWriter = innerWriter;
    }

    /// <summary>
    /// Writes a character to the output.
    /// </summary>
    /// <param name="value">The character to write.</param>
    public override void Write(char value)
    {
        _stringWriter.Append(value);
        _innerWriter.Write(value);
    }

    /// <summary>
    /// Writes a line to the output asynchronously.
    /// </summary>
    /// <param name="value">The string to write.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override Task WriteLineAsync(string? value)
    {
        _stringWriter.AppendLine(value);
        return _innerWriter.WriteLineAsync(value);
    }

    /// <summary>
    /// Gets the encoding of the writer.
    /// </summary>
    public override Encoding Encoding => _innerWriter.Encoding;

    /// <summary>
    /// Gets the captured contents of all writes.
    /// </summary>
    public string Contents => _stringWriter.ToString();
}
