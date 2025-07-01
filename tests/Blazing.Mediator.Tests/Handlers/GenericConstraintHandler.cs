namespace Blazing.Mediator.Tests;

/// <summary>
/// Generic constraint handler for testing type constraints.
/// </summary>
public class GenericConstraintHandler : IRequestHandler<GenericConstraintCommand<TestConstraintEntity>>
{
    /// <summary>
    /// Gets the last processed entity.
    /// </summary>
    public static TestConstraintEntity? LastProcessedEntity { get; set; }

    /// <summary>
    /// Handles the generic constraint command.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task Handle(GenericConstraintCommand<TestConstraintEntity> request, CancellationToken cancellationToken = default)
    {
        LastProcessedEntity = request.Data;
        return Task.CompletedTask;
    }
}