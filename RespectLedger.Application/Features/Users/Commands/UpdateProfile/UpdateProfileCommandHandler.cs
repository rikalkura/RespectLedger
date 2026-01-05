using Ardalis.Result;
using Mapster;
using MediatR;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Domain.Common;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.NotFound();
        }

        // Check nickname uniqueness if changing
        if (!string.IsNullOrWhiteSpace(request.Nickname) && request.Nickname != user.Nickname)
        {
            bool nicknameExists = await _userRepository.ExistsAsync(u => u.Nickname == request.Nickname && u.Id != request.UserId, cancellationToken);
            if (nicknameExists)
            {
                return Result<UserDto>.Invalid(new List<ValidationError>
                {
                    new() { Identifier = nameof(request.Nickname), ErrorMessage = DomainErrors.User.NicknameAlreadyExists }
                });
            }
        }

        user.UpdateProfile(request.Nickname, request.Bio);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        UserDto dto = user.Adapt<UserDto>();
        return Result<UserDto>.Success(dto);
    }
}
