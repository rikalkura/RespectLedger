using Ardalis.Result;
using MediatR;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Application.Features.Respects.DTOs;
using RespectLedger.Domain.Common;
using RespectLedger.Domain.Entities;
using RespectLedger.Domain.Enums;

namespace RespectLedger.Application.Features.Respects.Commands.GiveRespect;

public class GiveRespectCommandHandler : IRequestHandler<GiveRespectCommand, Result<RespectDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Season> _seasonRepository;
    private readonly IRespectRepository _respectRepository;
    private readonly IManaService _manaService;
    private readonly IUnitOfWork _unitOfWork;

    public GiveRespectCommandHandler(
        IRepository<User> userRepository,
        IRepository<Season> seasonRepository,
        IRespectRepository respectRepository,
        IManaService manaService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _seasonRepository = seasonRepository;
        _respectRepository = respectRepository;
        _manaService = manaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RespectDto>> Handle(GiveRespectCommand request, CancellationToken cancellationToken)
    {
        // Get sender
        User? sender = await _userRepository.GetByIdAsync(request.SenderId, cancellationToken);
        if (sender == null || !sender.IsActive())
        {
            return Result<RespectDto>.Invalid(new List<ValidationError>
            {
                new() { Identifier = nameof(request.SenderId), ErrorMessage = DomainErrors.User.InactiveAccount }
            });
        }

        // Get receiver
        User? receiver = await _userRepository.GetByIdAsync(request.ReceiverId, cancellationToken);
        if (receiver == null || !receiver.IsActive())
        {
            return Result<RespectDto>.Invalid(new List<ValidationError>
            {
                new() { Identifier = nameof(request.ReceiverId), ErrorMessage = DomainErrors.User.NotFound }
            });
        }

        // Check self-respect
        if (request.SenderId == request.ReceiverId)
        {
            return Result<RespectDto>.Invalid(new List<ValidationError>
            {
                new() { Identifier = nameof(request.ReceiverId), ErrorMessage = DomainErrors.User.SelfRespectNotAllowed }
            });
        }

        // Ensure mana reset and check mana
        await _manaService.EnsureManaResetAsync(sender, cancellationToken);
        sender = await _userRepository.GetByIdAsync(request.SenderId, cancellationToken); // Reload

        if (sender == null || !await _manaService.HasSufficientManaAsync(sender, 1, cancellationToken))
        {
            return Result<RespectDto>.Invalid(new List<ValidationError>
            {
                new() { Identifier = "Mana", ErrorMessage = DomainErrors.User.InsufficientMana }
            });
        }

        // Check cooldown (1 hour)
        bool hasRecentRespect = await _respectRepository.HasRecentRespectAsync(
            request.SenderId,
            request.ReceiverId,
            TimeSpan.FromHours(1),
            cancellationToken);

        if (hasRecentRespect)
        {
            return Result<RespectDto>.Invalid(new List<ValidationError>
            {
                new() { Identifier = nameof(request.ReceiverId), ErrorMessage = DomainErrors.User.CooldownActive }
            });
        }

        // Get current active season
        Season? currentSeason = await _seasonRepository.FirstOrDefaultAsync(
            s => s.IsActive && s.StartDate <= DateTime.UtcNow && s.EndDate >= DateTime.UtcNow,
            cancellationToken);

        if (currentSeason == null)
        {
            return Result<RespectDto>.Invalid(new List<ValidationError>
            {
                new() { Identifier = "Season", ErrorMessage = DomainErrors.Season.NoActiveSeason }
            });
        }

        // Create respect transaction
        Respect respect = new(
            request.SenderId,
            request.ReceiverId,
            currentSeason.Id,
            request.Reason,
            request.Tag,
            request.ImageUrl);

        _respectRepository.Add(respect);

        // Consume mana
        sender.ConsumeMana(1);

        // Update receiver XP and season score
        receiver.ReceiveRespect(1);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO
        RespectDto dto = new()
        {
            Id = respect.Id,
            SenderId = respect.SenderId,
            SenderNickname = sender.Nickname,
            SenderAvatarUrl = sender.AvatarUrl,
            ReceiverId = respect.ReceiverId,
            ReceiverNickname = receiver.Nickname,
            ReceiverAvatarUrl = receiver.AvatarUrl,
            SeasonId = respect.SeasonId,
            SeasonName = currentSeason.Name,
            Amount = respect.Amount,
            Reason = respect.Reason,
            Tag = respect.Tag,
            ImageUrl = respect.ImageUrl,
            LikeCount = 0,
            UserLiked = false,
            CreatedAt = respect.CreatedAt
        };

        return Result<RespectDto>.Success(dto);
    }
}
