#region

using zora.Core;
using zora.Core.Attributes;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Singleton)]
public sealed class AssetPathService : IAssetPathService, IZoraService
{
    private const string _assetsFolder = "assets";
    private const string _contentFolder = "content";
    private readonly string _basePath;
    private readonly IEnvironmentManagerService _environmentManager;

    public AssetPathService(IEnvironmentManagerService environmentManager)
    {
        this._environmentManager = environmentManager;
        this._basePath = this.DetermineBasePath();
    }

    public string GetAssetsBasePath() =>
        Path.Combine(this._basePath, AssetPathService._contentFolder, AssetPathService._assetsFolder);

    public string GetAssetWebPath(string fileName) =>
        Path.Combine(AssetPathService._contentFolder, AssetPathService._assetsFolder, fileName);

    private string DetermineBasePath()
    {
        if (this._environmentManager.IsDevelopment())
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.WWW_ROOT);
        }


        return Constants.PRODUCTION_BASE_PATH;
    }
}
