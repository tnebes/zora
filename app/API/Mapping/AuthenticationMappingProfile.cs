#region

using AutoMapper;
using zora.Core.Domain;
using zora.Core.DTOs;

#endregion

namespace zora.API.Mapping;

public class AuthenticationMappingProfile : Profile
{
    public AuthenticationMappingProfile()
    {
        this.CreateMap<User, AuthenticationStatusDto>()
            .ForMember(dest => dest.IsAuthenticated, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Roles, opt =>
                opt.MapFrom(src => Enumerable.Select(src.UserRoles, ur => ur.Role.Name)));
    }
}
