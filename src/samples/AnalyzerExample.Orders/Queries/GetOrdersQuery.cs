using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Orders.Domain;

namespace AnalyzerExample.Orders.Queries;

public class GetOrdersQuery : IOrderQuery<PagedResult<OrderSummaryDto>>, IPaginatedQuery<PagedResult<OrderSummaryDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? UserId { get; set; }
    public OrderStatus? StatusFilter { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public OrderSortBy SortBy { get; set; } = OrderSortBy.CreatedDate;
    public bool SortDescending { get; set; } = true;
    public bool IncludeCancelled { get; set; } = false;
}