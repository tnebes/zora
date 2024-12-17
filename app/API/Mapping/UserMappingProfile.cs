#region

using AutoMapper;
using zora.Core.Domain;
using zora.Core.DTOs;

#endregion

namespace zora.API.Mapping;

public class MinimalUserMappingProfile : Profile
{
    public MinimalUserMappingProfile() => this.CreateMap<User, MinimalUserDto>();
}
