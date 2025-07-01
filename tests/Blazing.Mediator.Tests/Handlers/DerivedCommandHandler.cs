namespace Blazing.Mediator.Tests;

/// <summary>
/// Derived command handler for testing inheritance scenarios.
/// </summary>
public class DerivedCommandHandler : IRequestHandler<DerivedCommand>
{
    /// <summary>
    /// Gets a value indicating whether the handler was executed.
    /// </summary>
    public static bool WasExecuted { get; set; }
    
    /// <summary>
    /// Gets the last processed derived command.
    /// </summary>
    public static DerivedCommand? ProcessedCommand { get; set; }

    /// <summary>
    /// Handles the derived command.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task Handle(DerivedCommand request, CancellationToken cancellationToken = default)
    {
        WasExecuted = true;
        ProcessedCommand = request;
        return Task.CompletedTask;
    }
}