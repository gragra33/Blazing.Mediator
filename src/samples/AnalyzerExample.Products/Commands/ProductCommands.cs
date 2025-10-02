using AnalyzerExample.Products.Domain;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Commands;

public interface IProductCommand<TResponse> : ICommand<TResponse>
{
    int ProductId { get; set; }
}