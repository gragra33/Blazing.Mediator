using System.Diagnostics;

namespace Blazing.Mediator.Examples;

/// <summary>
/// Another handler for the Pinged notification using Blazing.Mediator.
/// This demonstrates multiple handlers for the same notification.
/// Compare with MediatR version: uses Blazing.Mediator.INotificationHandler&lt;T&gt; instead of MediatR.INotificationHandler&lt;T&gt;.
/// </summary>
public class PingedAlsoHandler : INotificationHandler<Pinged>
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PingedAlsoHandler"/> class.
    /// </summary>
    /// <param name="writer">The text writer for output.</param>
    public PingedAlsoHandler(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Handles the Pinged notification.
    /// </summary>
    /// <param name="notification">The pinged notification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(Pinged notification, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"[TIMING] {DateTime.Now:HH:mm:ss.fff} - PingedAlsoHandler starting");
        
        await _writer.WriteLineAsync("Got pinged also async");
        
        stopwatch.Stop();
        Console.WriteLine($"[TIMING] {DateTime.Now:HH:mm:ss.fff} - PingedAlsoHandler completed in {stopwatch.ElapsedMilliseconds}ms");
    }
}
