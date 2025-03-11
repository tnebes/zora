#region

using System.Security.Claims;
using zora.Core.Domain;
using zora.Tests.TestFixtures.v1.MockBuilders;
using zora.Tests.Utils;

#endregion

namespace zora.Tests.TestFixtures.v1;

public sealed class FixtureBuilder : IFixtureBuilder
{
    private readonly MockedRepositoryFixture _fixture;
    private bool _permissionRepositorySetup;
    private bool _userRepositorySetup;

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

        if (!this._userRepositorySetup)
        {
            UserRepositoryMockBuilder.SetupUserRepository(this._fixture);
            this._userRepositorySetup = true;
        }

        return this;
    }

    public IFixtureBuilder WithPermissionRepository(List<Permission>? permissions = null)
    {
        if (permissions != null)
        {
            this._fixture.Permissions = permissions;
        }

        if (!this._permissionRepositorySetup)
        {
            PermissionRepositoryMockBuilder.SetupPermissionRepository(this._fixture);
            this._permissionRepositorySetup = true;
        }

        return this;
    }

    public IFixtureBuilder WithAuthentication(IEnumerable<Claim> claims)
    {
        this._fixture.Claims = claims;
        return this;
    }

    public IFixtureBuilder AsAdmin() => this.WithAuthentication(AuthenticationUtils.AdminClaims);

    public IFixtureBuilder AsRegularUser() => this.WithAuthentication(AuthenticationUtils.RegularUserClaims);

    public IFixtureBuilder AsAnonymous() => this.WithAuthentication(AuthenticationUtils.AnonymousUserClaims);
}
