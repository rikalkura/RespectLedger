using Ardalis.Result;
using MediatR;

namespace RespectLedger.Application.Features.Respects.Commands.GiveRespect;

public record GiveRespectCommand(
    Guid SenderId,
    Guid ReceiverId,
    string Reason,
    string? Tag = null,
    string? ImageUrl = null) : IRequest<Result<RespectDto>>;
