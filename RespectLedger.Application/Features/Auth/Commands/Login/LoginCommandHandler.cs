using Ardalis.Result;
using MediatR;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Domain.Common;
using RespectLedger.Domain.Entities;
using RespectLedger.Domain.Enums;

namespace RespectLedger.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            return Result<AuthResponseDto>.Invalid(new List<ValidationError>
            {
                new() { Identifier = nameof(request.Email), ErrorMessage = DomainErrors.User.InvalidCredentials }
            });
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Invalid(new List<ValidationError>
            {
                new() { Identifier = nameof(request.Password), ErrorMessage = DomainErrors.User.InvalidCredentials }
            });
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<AuthResponseDto>.Invalid(new List<ValidationError>
            {
                new() { Identifier = "Status", ErrorMessage = DomainErrors.User.InactiveAccount }
            });
        }

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
