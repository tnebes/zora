#region

using zora.Core;
using zora.Core.Attributes;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Singleton)]
public sealed class AssetPathService : IAssetPathService, IZoraService
{
    private readonly string _basePath;
    private readonly IEnvironmentManagerService _environmentManager;

    public AssetPathService(IEnvironmentManagerService environmentManager)
    {
        this._environmentManager = environmentManager;
        this._basePath = this.DetermineBasePath();
    }

    public string GetAssetsBasePath() =>
        Path.Combine(this._basePath, Constants.CONTENT_ASSETS_FOLDER);

    public string GetAssetWebPath(string fileName) =>
        Path.Combine(Constants.CONTENT_ASSETS_FOLDER, fileName);

    private string DetermineBasePath()
    {
        return this._environmentManager.IsDevelopment()
            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.WWW_ROOT)
            : Constants.PRODUCTION_BASE_PATH;
    }
}
