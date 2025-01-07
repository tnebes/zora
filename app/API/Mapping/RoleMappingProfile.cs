#region

using AutoMapper;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Responses;

#endregion

namespace zora.API.Mapping;

public sealed class RoleMappingProfile : Profile
{
    public RoleMappingProfile()
    {
        this.CreateMap<Role, RoleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UserIds, opt => opt.MapFrom(src => src.UserRoles.Select(u => u.UserId)))
            .ForMember(dest => dest.PermissionIds,
                opt => opt.MapFrom(src => src.RolePermissions.Select(p => p.PermissionId)));

        this.CreateMap<Role, FullRoleDto>()
            .IncludeBase<Role, RoleDto>()
            .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.UserRoles.Select(u => new { u.UserId, u.User.Username })))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.RolePermissions.Select(p => new { p.PermissionId, p.Permission.Name })));
    }
}
