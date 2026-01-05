using Microsoft.EntityFrameworkCore;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Domain.Entities;
using RespectLedger.Infrastructure.Data;

namespace RespectLedger.Infrastructure.Data.Repositories;

public class RespectRepository : Repository<Respect>, IRespectRepository
{
    public RespectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> HasRecentRespectAsync(Guid senderId, Guid receiverId, TimeSpan cooldownPeriod, CancellationToken cancellationToken = default)
    {
        DateTime cutoffTime = DateTime.UtcNow.Subtract(cooldownPeriod);
        
        return await _dbSet
            .AnyAsync(r => r.SenderId == senderId 
                && r.ReceiverId == receiverId 
                && r.CreatedAt >= cutoffTime, 
                cancellationToken);
    }

    public async Task<IEnumerable<Respect>> GetRespectsByReceiverAsync(Guid receiverId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.ReceiverId == receiverId)
            .OrderByDescending(r => r.CreatedAt)
            .Include(r => r.Sender)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Respect>> GetRespectsBySenderAsync(Guid senderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.SenderId == senderId)
            .OrderByDescending(r => r.CreatedAt)
            .Include(r => r.Receiver)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Respect>> GetGlobalFeedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .Include(r => r.Season)
            .ToListAsync(cancellationToken);
    }
}
