using System.Diagnostics;

namespace Blazing.Mediator.Examples;

/// <summary>
/// Constrained notification handler for Ponged notifications using Blazing.Mediator.
/// This demonstrates constraint-based notification handling where handlers are constrained to specific types.
/// Compare with MediatR version: uses Blazing.Mediator.INotificationHandler&lt;T&gt; with constraints.
/// </summary>
public class PongedConstrainedHandler : INotificationHandler<Ponged>
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PongedConstrainedHandler"/> class.
    /// </summary>
    /// <param name="writer">The text writer for output.</param>
    public PongedConstrainedHandler(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Handles the Ponged notification with constraint demonstration.
    /// </summary>
    /// <param name="notification">The ponged notification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(Ponged notification, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"[TIMING] {DateTime.Now:HH:mm:ss.fff} - PongedConstrainedHandler starting");
        
        await _writer.WriteLineAsync("Got pinged constrained async");
        
        stopwatch.Stop();
        Console.WriteLine($"[TIMING] {DateTime.Now:HH:mm:ss.fff} - PongedConstrainedHandler completed in {stopwatch.ElapsedMilliseconds}ms");
    }
}
