using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Common.Interfaces;

public interface IRespectRepository : IRepository<Respect>
{
    Task<bool> HasRecentRespectAsync(Guid senderId, Guid receiverId, TimeSpan cooldownPeriod, CancellationToken cancellationToken = default);
    Task<IEnumerable<Respect>> GetRespectsByReceiverAsync(Guid receiverId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Respect>> GetRespectsBySenderAsync(Guid senderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Respect>> GetGlobalFeedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
