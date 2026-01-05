using Ardalis.Result;
using MediatR;

namespace RespectLedger.Application.Features.Users.Commands.ApproveUser;

public record ApproveUserCommand(Guid UserId) : IRequest<Result>;
