using Ardalis.Result;
using MediatR;

namespace RespectLedger.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Email, string Nickname, string Password) : IRequest<Result<AuthResponseDto>>;
