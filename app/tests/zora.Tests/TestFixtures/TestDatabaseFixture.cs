#region

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using zora.Infrastructure.Data;

#endregion

namespace zora.Tests.TestFixtures;

public sealed class TestDatabaseFixture : IDisposable
{
    private const string CONNECTION_STRING =
        "Server=(localdb)\\mssqllocaldb;Database=ZoraTestDb;Trusted_Connection=True;MultipleActiveResultSets=true";

    private readonly ApplicationDbContext _dbContext;

    public TestDatabaseFixture()
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(CONNECTION_STRING))
            .BuildServiceProvider();

        this._dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

        this._dbContext.Database.EnsureDeleted();
        this._dbContext.Database.EnsureCreated();

        this.SeedTestData();
    }

    public void Dispose()
    {
        this._dbContext.Database.EnsureDeleted();
        this._dbContext.Dispose();
    }

    public ApplicationDbContext GetDbContext() => this._dbContext;

    private void SeedTestData()
    {
    }
}
