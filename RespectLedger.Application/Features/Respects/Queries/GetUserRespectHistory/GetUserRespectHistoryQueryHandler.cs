using Ardalis.Result;
using MediatR;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Application.Features.Respects.DTOs;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Features.Respects.Queries.GetUserRespectHistory;

public class GetUserRespectHistoryQueryHandler : IRequestHandler<GetUserRespectHistoryQuery, Result<PagedResult<RespectDto>>>
{
    private readonly IRespectRepository _respectRepository;
    private readonly IRepository<User> _userRepository;

    public GetUserRespectHistoryQueryHandler(IRespectRepository respectRepository, IRepository<User> userRepository)
    {
        _respectRepository = respectRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<RespectDto>>> Handle(GetUserRespectHistoryQuery request, CancellationToken cancellationToken)
    {
        // Verify user exists
        bool userExists = await _userRepository.ExistsAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result<PagedResult<RespectDto>>.NotFound();
        }

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

        IEnumerable<Respect> respects = await _respectRepository.GetRespectsByReceiverAsync(request.UserId, cancellationToken);

        List<Respect> respectsList = respects.ToList();
        int totalCount = respectsList.Count;

        // Manual pagination
        List<Respect> paginatedRespects = respectsList
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        List<RespectDto> dtos = paginatedRespects.Select(r => new RespectDto
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
            CreatedAt = r.CreatedAt
        }).ToList();

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
