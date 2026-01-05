using Ardalis.Result;
using MediatR;

namespace RespectLedger.Application.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(Guid UserId, string? Nickname, string? Bio) : IRequest<Result<UserDto>>;
