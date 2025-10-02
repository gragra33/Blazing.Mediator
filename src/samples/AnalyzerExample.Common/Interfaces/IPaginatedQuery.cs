using Blazing.Mediator;

namespace AnalyzerExample.Common.Interfaces;

public interface IPaginatedQuery<out TResponse> : IQuery<TResponse>
{
    int Page { get; set; }
    int PageSize { get; set; }
}