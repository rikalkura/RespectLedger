using Mapster;
using RespectLedger.Application.Features.Users.DTOs;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Common.Mappings;

public class MappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Role, src => src.Role.ToString());
    }
}
