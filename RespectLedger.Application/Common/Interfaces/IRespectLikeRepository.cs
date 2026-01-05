using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Common.Interfaces;

public interface IRespectLikeRepository : IRepository<RespectLike>
{
    Task<bool> HasUserLikedAsync(int respectId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetLikeCountAsync(int respectId, CancellationToken cancellationToken = default);
    Task<RespectLike?> GetUserLikeAsync(int respectId, Guid userId, CancellationToken cancellationToken = default);
}
