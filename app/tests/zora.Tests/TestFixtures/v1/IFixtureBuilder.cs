#region

using System.Security.Claims;
using zora.Core.Domain;

#endregion

namespace zora.Tests.TestFixtures.v1;

public interface IFixtureBuilder
{
    MockedRepositoryFixture Build();
    IFixtureBuilder WithUsers(List<User> users);
    IFixtureBuilder WithPermissions(List<Permission> permissions);
    IFixtureBuilder WithUserRepository(List<User>? users = null);
    IFixtureBuilder WithPermissionRepository(List<Permission>? permissions = null);
    IFixtureBuilder WithAuthentication(IEnumerable<Claim> claims);
}
