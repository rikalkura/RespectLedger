using Ardalis.Result;
using MediatR;
using RespectLedger.Application.Features.Users.DTOs;

namespace RespectLedger.Application.Features.Users.Queries.GetPendingUsers;

public record GetPendingUsersQuery : IRequest<Result<IEnumerable<UserDto>>>;
