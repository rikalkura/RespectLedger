using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Common.Interfaces;

public interface IManaService
{
    Task EnsureManaResetAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> HasSufficientManaAsync(User user, int requiredAmount, CancellationToken cancellationToken = default);
}
