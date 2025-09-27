using System.Diagnostics;

namespace Blazing.Mediator.Examples;

/// <summary>
/// Handler for the Ping request using Blazing.Mediator.
/// This demonstrates a basic request handler that returns a response.
/// Compare with MediatR version: uses Blazing.Mediator.IRequestHandler&lt;T,R&gt; instead of MediatR.IRequestHandler&lt;T,R&gt;.
/// </summary>
public class PingHandler : IRequestHandler<Ping, Pong>
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PingHandler"/> class.
    /// </summary>
    /// <param name="writer">The text writer for output.</param>
    public PingHandler(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Handles the Ping request and returns a Pong response.
    /// </summary>
    /// <param name="request">The ping request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A pong response.</returns>
    public async Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"[TIMING] {DateTime.Now:HH:mm:ss.fff} - PingHandler starting");
        
        await _writer.WriteLineAsync($"--- Handled Ping: {request.Message}");
        
        stopwatch.Stop();
        Console.WriteLine($"[TIMING] {DateTime.Now:HH:mm:ss.fff} - PingHandler completed in {stopwatch.ElapsedMilliseconds}ms");
        
        return new Pong { Message = request.Message + " Pong" };
    }
}
