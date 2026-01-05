using Ardalis.Result;
using MediatR;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Domain.Common;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        bool emailExists = await _userRepository.ExistsAsync(u => u.Email == request.Email, cancellationToken);
        if (emailExists)
        {
            return Result<AuthResponseDto>.Invalid(new List<ValidationError>
            {
                new() { Identifier = nameof(request.Email), ErrorMessage = DomainErrors.User.EmailAlreadyExists }
            });
        }

        // Check if nickname already exists
        bool nicknameExists = await _userRepository.ExistsAsync(u => u.Nickname == request.Nickname, cancellationToken);
        if (nicknameExists)
        {
            return Result<AuthResponseDto>.Invalid(new List<ValidationError>
            {
                new() { Identifier = nameof(request.Nickname), ErrorMessage = DomainErrors.User.NicknameAlreadyExists }
            });
        }

        // Create user
        string passwordHash = _passwordHasher.HashPassword(request.Password);
        User user = new(request.Nickname, request.Email, passwordHash);

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate token
        string token = _jwtTokenGenerator.GenerateToken(user);

        AuthResponseDto response = new()
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Nickname = user.Nickname,
            Role = user.Role.ToString(),
            Status = user.Status.ToString()
        };

        return Result<AuthResponseDto>.Success(response);
    }
}
