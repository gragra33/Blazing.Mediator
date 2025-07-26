using Blazing.Mediator;

namespace Blazing.Mediator.Examples;

/// <summary>
/// Covariant notification handler that handles all INotification types using Blazing.Mediator.
/// This demonstrates covariant notification handling where a handler can handle base types and derived types.
/// Compare with MediatR version: uses Blazing.Mediator.INotificationSubscriber&lt;T&gt; with covariance support.
/// </summary>
public class CovariantNotificationHandler : INotificationSubscriber<INotification>
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="CovariantNotificationHandler"/> class.
    /// </summary>
    /// <param name="writer">The text writer for output.</param>
    public CovariantNotificationHandler(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Handles any INotification with covariant demonstration.
    /// </summary>
    /// <param name="notification">Any notification implementing INotification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnNotification(INotification notification, CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync("Got notified");
    }
}
