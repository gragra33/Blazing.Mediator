namespace Blazing.Mediator.Tests;

/// <summary>
/// Second duplicate command handler for testing multiple handler scenarios.
/// </summary>
public class DuplicateCommandHandler2 : IRequestHandler<DuplicateHandlerCommand>
{
    /// <summary>
    /// Handles the duplicate handler command.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task Handle(DuplicateHandlerCommand request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}