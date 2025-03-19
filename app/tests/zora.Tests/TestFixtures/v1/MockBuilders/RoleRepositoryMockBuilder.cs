#region

using FluentResults;
using Moq;
using zora.Core.Domain;

#endregion

namespace zora.Tests.TestFixtures.v1.MockBuilders;

internal static class RoleRepositoryMockBuilder
{
    internal static void SetupRoleRepository(MockedRepositoryFixture fixture)
    {
        fixture.MockRoleRepository
            .Setup(repo => repo.GetRolesByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<bool>()))
            .ReturnsAsync((IEnumerable<long> roleIds, bool includeProperties) =>
                fixture.Roles.Where(r => roleIds.Contains(r.Id)));

        fixture.MockRoleRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>(), It.IsAny<bool>()))
            .ReturnsAsync((long id, bool includeProperties) =>
            {
                Role? role = fixture.Roles.FirstOrDefault(r => r.Id == id);
                return role != null ? Result.Ok(role) : Result.Fail<Role>(new Error("Role not found"));
            });
    }
} 