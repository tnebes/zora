#region

using FluentResults;
using Moq;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Tests.TestFixtures.MockBuilders;

internal static class PermissionRepositoryMockBuilder
{
    internal static void SetupPermissionRepository(MockedRepositoryFixture fixture)
    {
        fixture.MockPermissionRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<bool>()))
            .ReturnsAsync(() => Result.Ok(fixture.Permissions.AsEnumerable()));

        fixture.MockPermissionRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>(), It.IsAny<bool>()))
            .ReturnsAsync((long id, bool includeProperties) =>
            {
                Permission? permission = fixture.Permissions.FirstOrDefault(p => p.Id == id);
                return permission != null
                    ? Result.Ok(permission)
                    : Result.Fail<Permission>(new Error("Permission not found"));
            });

        fixture.MockPermissionRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<Permission>()))
            .ReturnsAsync((Permission permission) =>
            {
                long newId = fixture.Permissions.Any() ? fixture.Permissions.Max(p => p.Id) + 1 : 1;
                permission.Id = newId;
                fixture.Permissions.Add(permission);
                return Result.Ok(permission);
            });

        fixture.MockPermissionRepository
            .Setup(repo => repo.DeleteAsync(It.IsAny<Permission>()))
            .ReturnsAsync((Permission permission) =>
            {
                Permission? existingPermission = fixture.Permissions.FirstOrDefault(p => p.Id == permission.Id);
                if (existingPermission != null)
                {
                    existingPermission.Deleted = true;
                    return true;
                }

                return false;
            });

        fixture.MockPermissionRepository
            .Setup(repo => repo.GetPagedAsync(It.IsAny<QueryParamsDto>(), It.IsAny<bool>()))
            .ReturnsAsync((QueryParamsDto queryParams, bool includeProperties) =>
            {
                int skip = (queryParams.Page - 1) * queryParams.PageSize;
                IEnumerable<Permission> pagedPermissions = fixture.Permissions.Skip(skip).Take(queryParams.PageSize);
                return Result.Ok((pagedPermissions, fixture.Permissions.Count));
            });

        fixture.MockPermissionRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<Permission>()))
            .ReturnsAsync((Permission permission) =>
            {
                int index = fixture.Permissions.FindIndex(p => p.Id == permission.Id);
                if (index >= 0)
                {
                    fixture.Permissions[index] = permission;
                    return Result.Ok(permission);
                }

                return Result.Fail<Permission>(new Error("Permission not found"));
            });
    }
}
