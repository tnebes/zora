#region

using AutoMapper;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;

#endregion

namespace zora.API.Mapping;

public sealed class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        this.CreateMap<User, MinimumUserDto>();
        this.CreateMap<User, FullUserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.CreatedAt,
                opt => opt.MapFrom(src => src.CreatedAt)) // TODO this displays milliseconds
            .ForMember(dest => dest.Roles, opt =>
                opt.MapFrom(src => src.UserRoles.ToDictionary(ur => ur.Role.Id, ur => ur.Role.Name)));
        this.CreateMap<CreateMinimumUserDto, User>();
    }
}
