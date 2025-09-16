namespace Blazing.Mediator.Examples.Streams;

/// <summary>
/// Handler for the Sing streaming request using Blazing.Mediator.
/// This demonstrates streaming response handlers that return IAsyncEnumerable.
/// Compare with MediatR version: uses Blazing.Mediator.IStreamRequestHandler&lt;T,R&gt; instead of MediatR.IStreamRequestHandler&lt;T,R&gt;.
/// </summary>
public class SingHandler : IStreamRequestHandler<Sing, Song>
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingHandler"/> class.
    /// </summary>
    /// <param name="writer">The text writer for output.</param>
    public SingHandler(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Handles the Sing request and returns a stream of Song responses.
    /// </summary>
    /// <param name="request">The sing request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async enumerable of songs.</returns>
    public async IAsyncEnumerable<Song> Handle(Sing request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync($"--- Handled Sing: {request.Message}, Song");

        var notes = new[] { "do", "re", "mi", "fa", "so", "la", "ti", "do" };

        foreach (var note in notes)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            yield return new Song { Message = $"Singing {note}" };

            // Simulate some async work
            await Task.Delay(10, cancellationToken);
        }
    }
}
