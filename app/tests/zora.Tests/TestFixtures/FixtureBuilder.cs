#region

using zora.Core.Domain;

#endregion

namespace zora.Tests.TestFixtures;

public sealed class FixtureBuilder : IFixtureBuilder
{
    private readonly MockedRepositoryFixture _fixture;

    public FixtureBuilder() => this._fixture = new MockedRepositoryFixture();

    public MockedRepositoryFixture Build() => this._fixture;

    public IFixtureBuilder WithUsers(List<User> users)
    {
        this._fixture.Users = users;
        return this;
    }

    public IFixtureBuilder WithPermissions(List<Permission> permissions)
    {
        this._fixture.Permissions = permissions;
        return this;
    }

    public IFixtureBuilder WithUserRepository(List<User>? users = null)
    {
        if (users != null)
        {
            this._fixture.Users = users;
        }

        UserRepositoryMockBuilder.SetupUserRepository(this._fixture);
        return this;
    }

    public IFixtureBuilder WithPermissionRepository(List<Permission>? permissions = null)
    {
        if (permissions != null)
        {
            this._fixture.Permissions = permissions;
        }

        PermissionRepositoryMockBuilder.SetupPermissionRepository(this._fixture);
        return this;
    }
}
