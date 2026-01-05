using Ardalis.Result;
using MediatR;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Domain.Common;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Features.Respects.Commands.LikeRespect;

public class LikeRespectCommandHandler : IRequestHandler<LikeRespectCommand, Result>
{
    private readonly IRepository<Respect> _respectRepository;
    private readonly IRespectLikeRepository _respectLikeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LikeRespectCommandHandler(
        IRepository<Respect> respectRepository,
        IRespectLikeRepository respectLikeRepository,
        IUnitOfWork unitOfWork)
    {
        _respectRepository = respectRepository;
        _respectLikeRepository = respectLikeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LikeRespectCommand request, CancellationToken cancellationToken)
    {
        // Verify respect exists
        Respect? respect = await _respectRepository.GetByIdAsync(request.RespectId, cancellationToken);
        if (respect == null)
        {
            return Result.NotFound();
        }

        // Check if already liked
        bool alreadyLiked = await _respectLikeRepository.HasUserLikedAsync(request.RespectId, request.UserId, cancellationToken);
        if (alreadyLiked)
        {
            // Unlike - remove the like
            RespectLike? existingLike = await _respectLikeRepository.GetUserLikeAsync(request.RespectId, request.UserId, cancellationToken);
            if (existingLike != null)
            {
                _respectLikeRepository.Remove(existingLike);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            return Result.Success();
        }

        // Create new like
        RespectLike like = new(request.RespectId, request.UserId);
        _respectLikeRepository.Add(like);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
