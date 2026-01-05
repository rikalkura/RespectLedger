using Ardalis.Result;
using Mapster;
using MediatR;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Domain.Entities;
using RespectLedger.Domain.Enums;

namespace RespectLedger.Application.Features.Users.Queries.GetPendingUsers;

public class GetPendingUsersQueryHandler : IRequestHandler<GetPendingUsersQuery, Result<IEnumerable<UserDto>>>
{
    private readonly IRepository<User> _userRepository;

    public GetPendingUsersQueryHandler(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(GetPendingUsersQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<User> pendingUsers = await _userRepository.FindAsync(
            u => u.Status == UserStatus.Pending, 
            cancellationToken);

        List<UserDto> dtos = pendingUsers.Adapt<List<UserDto>>();
        return Result<IEnumerable<UserDto>>.Success(dtos);
    }
}
