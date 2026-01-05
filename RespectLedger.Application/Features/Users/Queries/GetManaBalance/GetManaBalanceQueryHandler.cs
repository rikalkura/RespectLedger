using Ardalis.Result;
using MediatR;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Features.Users.Queries.GetManaBalance;

public class GetManaBalanceQueryHandler : IRequestHandler<GetManaBalanceQuery, Result<ManaBalanceDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IManaService _manaService;

    public GetManaBalanceQueryHandler(IRepository<User> userRepository, IManaService manaService)
    {
        _userRepository = userRepository;
        _manaService = manaService;
    }

    public async Task<Result<ManaBalanceDto>> Handle(GetManaBalanceQuery request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<ManaBalanceDto>.NotFound();
        }

        // Ensure mana is reset if needed
        await _manaService.EnsureManaResetAsync(user, cancellationToken);

        // Reload user to get updated mana
        user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        DateTime? nextReset = user.LastManaReset?.Date.AddDays(1).AddHours(23).AddMinutes(59).AddSeconds(59);

        ManaBalanceDto dto = new()
        {
            CurrentMana = user.CurrentMana,
            MaxMana = 3,
            LastReset = user.LastManaReset,
            NextReset = nextReset
        };

        return Result<ManaBalanceDto>.Success(dto);
    }
}
