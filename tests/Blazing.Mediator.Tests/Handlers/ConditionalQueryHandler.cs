using Blazing.Mediator;

/// <summary>
/// Conditional query handler for testing conditional middleware.
/// </summary>
public class ConditionalQueryHandler : IRequestHandler<ConditionalQuery, string>
{
    /// <summary>
    /// Handles the conditional query.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A constant "ConditionalHandler" response.</returns>
    public ValueTask<string> Handle(ConditionalQuery request, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult($"Handler: {request.Value}");
    }
}