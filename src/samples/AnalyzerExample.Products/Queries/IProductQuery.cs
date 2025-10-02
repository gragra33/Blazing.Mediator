using Blazing.Mediator;

namespace AnalyzerExample.Products.Queries;

/// <summary>
/// Product-specific query interfaces
/// </summary>
public interface IProductQuery<TResponse> : IQuery<TResponse>
{
    bool IncludeDeleted { get; set; }
}