using Ardalis.Result;
using Mapster;
using MediatR;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Features.Users.Commands.UploadAvatar;

public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, Result<UserDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UploadAvatarCommandHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.NotFound();
        }

        user.UpdateAvatar(request.ImageUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        UserDto dto = user.Adapt<UserDto>();
        return Result<UserDto>.Success(dto);
    }
}
