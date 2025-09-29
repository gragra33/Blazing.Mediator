using System.Diagnostics;

namespace Blazing.Mediator.Examples;

/// <summary>
/// Handler for the Pinged notification using Blazing.Mediator.
/// This demonstrates a notification handler that can be one of many handlers for the same notification.
/// Compare with MediatR version: uses Blazing.Mediator.INotificationHandler&lt;T&gt; instead of MediatR.INotificationHandler&lt;T&gt;.
/// </summary>
public class PingedHandler : INotificationHandler<Pinged>
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PingedHandler"/> class.
    /// </summary>
    /// <param name="writer">The text writer for output.</param>
    public PingedHandler(TextWriter writer)
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
        Console.WriteLine($"[TIMING] {DateTime.Now:HH:mm:ss.fff} - PingedHandler starting");
        
        await _writer.WriteLineAsync($"Got pinged async at {notification.Timestamp}");
        
        stopwatch.Stop();
        Console.WriteLine($"[TIMING] {DateTime.Now:HH:mm:ss.fff} - PingedHandler completed in {stopwatch.ElapsedMilliseconds}ms");
    }
}
