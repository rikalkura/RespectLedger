using Ardalis.Result;
using MediatR;

namespace RespectLedger.Application.Features.Users.Queries.GetCurrentUser;

public record GetCurrentUserQuery(Guid UserId) : IRequest<Result<UserDto>>;
