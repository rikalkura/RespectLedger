using Ardalis.Result;
using Mapster;
using MediatR;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Features.Users.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly IRepository<User> _userRepository;

    public GetCurrentUserQueryHandler(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.NotFound();
        }

        UserDto dto = user.Adapt<UserDto>();
        return Result<UserDto>.Success(dto);
    }
}
