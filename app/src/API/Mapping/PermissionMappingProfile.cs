#region

using AutoMapper;
using zora.Core.Domain;
using zora.Core.DTOs.Responses;

#endregion

namespace zora.API.Mapping;

public sealed class PermissionMappingProfile : Profile
{
    public PermissionMappingProfile()
    {
        this.CreateMap<Permission, PermissionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.PermissionString, opt => opt.MapFrom(src => src.PermissionString))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.RoleIds, opt => opt.MapFrom(src =>
                src.RolePermissions != null ? src.RolePermissions.Select(rp => rp.RoleId) : Enumerable.Empty<long>()));
    }
}
