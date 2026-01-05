using Microsoft.EntityFrameworkCore;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Domain.Entities;
using RespectLedger.Infrastructure.Data;

namespace RespectLedger.Infrastructure.Data.Repositories;

public class RespectLikeRepository : Repository<RespectLike>, IRespectLikeRepository
{
    public RespectLikeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> HasUserLikedAsync(int respectId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(rl => rl.RespectId == respectId && rl.UserId == userId, cancellationToken);
    }

    public async Task<int> GetLikeCountAsync(int respectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .CountAsync(rl => rl.RespectId == respectId, cancellationToken);
    }

    public async Task<RespectLike?> GetUserLikeAsync(int respectId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(rl => rl.RespectId == respectId && rl.UserId == userId, cancellationToken);
    }
}
