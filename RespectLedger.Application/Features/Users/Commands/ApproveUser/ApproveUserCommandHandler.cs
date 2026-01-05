using Ardalis.Result;
using MediatR;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Features.Users.Commands.ApproveUser;

public class ApproveUserCommandHandler : IRequestHandler<ApproveUserCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveUserCommandHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ApproveUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            return Result.NotFound();
        }

        user.Approve();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
