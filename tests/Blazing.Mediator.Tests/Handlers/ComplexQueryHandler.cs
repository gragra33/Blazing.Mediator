namespace Blazing.Mediator.Tests;

/// <summary>
/// Handler for complex queries that processes filtering and returns complex results.
/// Used to test the mediator functionality with complex data structures and filtering logic.
/// </summary>
public class ComplexQueryHandler : IRequestHandler<ComplexQuery, ComplexResult>
{
    /// <summary>
    /// Handles the complex query by applying filtering logic and returning a complex result.
    /// </summary>
    /// <param name="request">The complex query containing filter criteria.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task containing a ComplexResult with filtered data and count information.</returns>
    public Task<ComplexResult> Handle(ComplexQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ComplexResult
        {
            FilteredData = $"Filtered: {request.Filter}",
            Count = 1
        });
    }
}