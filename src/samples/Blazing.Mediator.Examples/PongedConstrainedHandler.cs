namespace Blazing.Mediator.Examples;

/// <summary>
/// Constrained notification handler for Ponged notifications using Blazing.Mediator.
/// This demonstrates constraint-based notification handling where handlers are constrained to specific types.
/// Compare with MediatR version: uses Blazing.Mediator.INotificationSubscriber&lt;T&gt; with constraints.
/// </summary>
public class PongedConstrainedHandler : INotificationSubscriber<Ponged>
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
    public async Task OnNotification(Ponged notification, CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync("Got pinged constrained async");
    }
}
