using Ardalis.Result;
using MediatR;

namespace RespectLedger.Application.Features.Respects.Commands.LikeRespect;

public record LikeRespectCommand(int RespectId, Guid UserId) : IRequest<Result>;
