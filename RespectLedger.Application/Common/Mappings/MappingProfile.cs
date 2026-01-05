using Mapster;
using RespectLedger.Application.Features.Respects.DTOs;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Application.Common.Mappings;

public class MappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Respect, RespectDto>()
            .Ignore(dest => dest.SenderNickname)
            .Ignore(dest => dest.SenderAvatarUrl)
            .Ignore(dest => dest.ReceiverNickname)
            .Ignore(dest => dest.ReceiverAvatarUrl)
            .Ignore(dest => dest.SeasonName)
            .Ignore(dest => dest.LikeCount)
            .Ignore(dest => dest.UserLiked);
    }
}
