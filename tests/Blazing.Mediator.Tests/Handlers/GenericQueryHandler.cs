namespace Blazing.Mediator.Tests;

/// <summary>
/// Handler for generic queries containing a list of integers.
/// Used to test generic query handling in the mediator system.
/// </summary>
public class GenericQueryHandler : IRequestHandler<GenericQuery<List<int>>, string>
{
    /// <summary>
    /// Handles the generic query and returns a count of items in the data list.
    /// </summary>
    /// <param name="request">The generic query containing a list of integers.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task containing a string with the count of items in the list.</returns>
    public Task<string> Handle(GenericQuery<List<int>> request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Count: {request.Data?.Count ?? 0}");
    }
}