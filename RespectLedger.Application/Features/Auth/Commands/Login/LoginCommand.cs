using Ardalis.Result;
using MediatR;

namespace RespectLedger.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponseDto>>;
