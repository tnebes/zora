#region

using zora.Core.Domain;

#endregion

namespace zora.Tests.TestFixtures;

public interface IFixtureBuilder
{
    MockedRepositoryFixture Build();
    IFixtureBuilder WithUsers(List<User> users);
    IFixtureBuilder WithPermissions(List<Permission> permissions);
    IFixtureBuilder WithUserRepository(List<User>? users = null);
    IFixtureBuilder WithPermissionRepository(List<Permission>? permissions = null);
}
