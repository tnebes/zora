using AutoMapper;

namespace zora.API.Mapping;

public class MinimalUserMappingProfile : Profile
{
    public MinimalUserMappingProfile() => this.CreateMap<Core.Domain.User, Core.DTOs.MinimalUserDto>();
}
