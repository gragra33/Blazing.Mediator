using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Orders.Domain;

namespace AnalyzerExample.Orders.Queries;

public class SearchOrdersQuery : IOrderQuery<PagedResult<OrderSummaryDto>>, IPaginatedQuery<PagedResult<OrderSummaryDto>>
{
    public string SearchTerm { get; set; } = string.Empty; // Order number, user email, etc.
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public List<OrderStatus> StatusFilters { get; set; } = new();
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool IncludeCancelled { get; set; } = false;
}