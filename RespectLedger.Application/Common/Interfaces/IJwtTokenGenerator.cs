using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
