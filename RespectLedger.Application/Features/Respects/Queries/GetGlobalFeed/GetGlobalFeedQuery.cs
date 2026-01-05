using Ardalis.Result;
using MediatR;
using RespectLedger.Application.Features.Respects.DTOs;

namespace RespectLedger.Application.Features.Respects.Queries.GetGlobalFeed;

public record GetGlobalFeedQuery(int PageNumber = 1, int PageSize = 20) : IRequest<Result<PagedResult<RespectDto>>>;

public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
