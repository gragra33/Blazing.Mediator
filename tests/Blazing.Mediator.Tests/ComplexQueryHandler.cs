namespace Blazing.Mediator.Tests;

public class ComplexQueryHandler : IRequestHandler<ComplexQuery, ComplexResult>
{
    public Task<ComplexResult> Handle(ComplexQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ComplexResult
        {
            FilteredData = $"Filtered: {request.Filter}",
            Count = 1
        });
    }
}