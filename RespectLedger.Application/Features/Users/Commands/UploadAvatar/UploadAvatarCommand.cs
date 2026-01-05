using Ardalis.Result;
using MediatR;

namespace RespectLedger.Application.Features.Users.Commands.UploadAvatar;

public record UploadAvatarCommand(Guid UserId, string ImageUrl) : IRequest<Result<UserDto>>;
