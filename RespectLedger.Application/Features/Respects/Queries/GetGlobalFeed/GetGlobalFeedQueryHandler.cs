using Ardalis.Result;
using MediatR;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Application.Features.Respects.DTOs;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Features.Respects.Queries.GetGlobalFeed;

public class GetGlobalFeedQueryHandler : IRequestHandler<GetGlobalFeedQuery, Result<PagedResult<RespectDto>>>
{
    private readonly IRespectRepository _respectRepository;
    private readonly IRespectLikeRepository _respectLikeRepository;

    public GetGlobalFeedQueryHandler(
        IRespectRepository respectRepository,
        IRespectLikeRepository respectLikeRepository)
    {
        _respectRepository = respectRepository;
        _respectLikeRepository = respectLikeRepository;
    }

    public async Task<Result<PagedResult<RespectDto>>> Handle(GetGlobalFeedQuery request, CancellationToken cancellationToken)
    {
        if (request.PageNumber < 1)
        {
            return Result<PagedResult<RespectDto>>.Invalid(new List<ValidationError>
            {
                new() { Identifier = nameof(request.PageNumber), ErrorMessage = "Page number must be greater than 0" }
            });
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return Result<PagedResult<RespectDto>>.Invalid(new List<ValidationError>
            {
                new() { Identifier = nameof(request.PageSize), ErrorMessage = "Page size must be between 1 and 100" }
            });
        }

        IEnumerable<Respect> respects = await _respectRepository.GetGlobalFeedAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        int totalCount = await _respectRepository.CountAsync(cancellationToken: cancellationToken);

        List<RespectDto> dtos = new();
        foreach (Respect r in respects)
        {
            int likeCount = await _respectLikeRepository.GetLikeCountAsync(r.Id, cancellationToken);

            dtos.Add(new RespectDto
            {
                Id = r.Id,
                SenderId = r.SenderId,
                SenderNickname = r.Sender?.Nickname ?? string.Empty,
                SenderAvatarUrl = r.Sender?.AvatarUrl,
                ReceiverId = r.ReceiverId,
                ReceiverNickname = r.Receiver?.Nickname ?? string.Empty,
                ReceiverAvatarUrl = r.Receiver?.AvatarUrl,
                SeasonId = r.SeasonId,
                SeasonName = r.Season?.Name ?? string.Empty,
                Amount = r.Amount,
                Reason = r.Reason,
                Tag = r.Tag,
                ImageUrl = r.ImageUrl,
                LikeCount = likeCount,
                UserLiked = false, // Will be set in API layer based on current user
                CreatedAt = r.CreatedAt
            });
        }

        PagedResult<RespectDto> result = new()
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<RespectDto>>.Success(result);
    }
}
