using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Orders.Domain;

namespace AnalyzerExample.Orders.Queries;

public class GetUserOrdersQuery : IOrderQuery<PagedResult<OrderSummaryDto>>, IPaginatedQuery<PagedResult<OrderSummaryDto>>
{
    public int UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public List<OrderStatus> StatusFilters { get; set; } = new();
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool IncludeCancelled { get; set; } = false;
}