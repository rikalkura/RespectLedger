using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Infrastructure.Services;

public class ManaService : IManaService
{
    private readonly IUnitOfWork _unitOfWork;

    public ManaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task EnsureManaResetAsync(User user, CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;
        DateTime? lastReset = user.LastManaReset;

        // Check if we need to reset (new day)
        if (lastReset == null || lastReset.Value.Date < now.Date)
        {
            user.ResetMana();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public Task<bool> HasSufficientManaAsync(User user, int requiredAmount, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(user.HasMana(requiredAmount));
    }
}
