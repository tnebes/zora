#region

using System.Security.Claims;
using AutoMapper;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Helpers;
using Constants = zora.Core.Constants;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class RoleService : IRoleService, IZoraService
{
    private readonly ILogger<RoleService> _logger;
    private readonly IMapper _mapper;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;

    public RoleService(IRoleRepository roleRepository, IUserRoleRepository userRoleRepository, IMapper mapper,
        ILogger<RoleService> logger)
    {
        this._roleRepository = roleRepository;
        this._userRoleRepository = userRoleRepository;
        this._mapper = mapper;
        this._logger = logger;
    }

    public bool IsRole(ClaimsPrincipal httpContextUser, string role)
    {
        if (httpContextUser == null)
        {
            return false;
        }

        return httpContextUser.IsInRole(role);
    }

    public bool IsAdmin(ClaimsPrincipal httpContextUser) => this.IsRole(httpContextUser, Constants.ADMIN);

    public async Task<bool> AssignRoles(User user, IEnumerable<long> roleIds)
    {
        List<long> roles = roleIds.ToList();
        if (!await this.IsValid(roles))
        {
            return false;
        }

        return await this._userRoleRepository.AssignRoles(user, roles);
    }

    public async Task<(IEnumerable<Role>, int total)> GetRolesAsync(QueryParamsDto queryParams) =>
        await this._roleRepository.GetPagedAsync(queryParams);

    public async Task<RoleResponseDto> GetRolesDtoAsync(QueryParamsDto queryParams)
    {
        (IEnumerable<Role> roles, int total) = await this.GetRolesAsync(queryParams);
        return roles.AsEnumerable().ToRoleResponseDto(total, queryParams.Page, queryParams.PageSize, this._mapper);
    }

    private async Task<bool> IsValid(IEnumerable<long> roleIds)
    {
        try
        {
            IEnumerable<long> ids = roleIds.ToList();
            IEnumerable<Role> roles = await this._roleRepository.GetRolesByIdsAsync(ids);
            return roles.Count() == ids.Count();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error while checking if roles are valid");
            return false;
        }
    }
}
